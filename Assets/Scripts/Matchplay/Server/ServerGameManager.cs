using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Matchplay.Shared;
using Matchplay.Shared.Tools;
using Unity.Netcode;
using Random = UnityEngine.Random;

namespace Matchplay.Server
{
    public class ServerGameManager : IDisposable
    {
        public bool StartedServices => m_StartedServices;
        public MatchplayNetworkServer NetworkServer => m_NetworkServer;
        public SynchedServerData ServerData => m_SynchedServerData;

        MatchplayNetworkServer m_NetworkServer;
        MatchplayBackfiller m_Backfiller;
        string connectionString => $"{m_ServerIP}:{m_ServerPort}";
        string m_ServerIP = "0.0.0.0";
        int m_ServerPort = 7777;
        int m_QueryPort = 7787;
        const int k_MultiplayServiceTimeout = 15000;
        bool m_StartedServices;
        MultiplayAllocationService m_MultiplayAllocationService;
        SynchedServerData m_SynchedServerData;

        public ServerGameManager(string serverIP, int serverPort, int serverQPort, NetworkManager manager)
        {
            m_ServerIP = serverIP;
            m_ServerPort = serverPort;
            m_QueryPort = serverQPort;
            m_NetworkServer = new MatchplayNetworkServer(manager);
            m_MultiplayAllocationService = new MultiplayAllocationService();
        }

        /// <summary>
        /// Attempts to initialize the server with services (If we are on Multiplay) and if we time out, we move on to default setup for local testing.
        /// </summary>
        public async Task StartGameServerAsync(GameInfo startingGameInfo)
        {
            Debug.Log($"Starting server with:{startingGameInfo}.");

            try
            {
                var matchmakerPayload = await GetPayloadWithinTimeout(k_MultiplayServiceTimeout);

                if (matchmakerPayload != null)
                {
                    Debug.Log($"Got payload: {matchmakerPayload}");
                    startingGameInfo = PickSharedGameInfo(matchmakerPayload);

                    await StartAllocationService(startingGameInfo,
                        (ushort)matchmakerPayload.MatchProperties.Players.Count);
                    await CreateAndStartBackfilling(matchmakerPayload, startingGameInfo);
                    m_NetworkServer.OnPlayerJoined += UserJoinedServer;
                    m_NetworkServer.OnPlayerLeft += UserLeft;
                    m_StartedServices = true;
                }
                else
                {
                    Debug.LogWarning("Connecting to Multiplay Timed out, starting with defaults.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Something went wrong trying to set up the Services:\n{ex} ");
            }

            if (!m_NetworkServer.StartServer(m_ServerIP, m_ServerPort, startingGameInfo))
            {
                Debug.LogError("NetworkServer did not start as expected.");
                return;
            }

            //Changes gameMap and sets the synched shared variables to the starting info
            m_SynchedServerData = await m_NetworkServer.SetupServer(startingGameInfo);
            if (m_SynchedServerData == null)
            {
                Debug.LogError("NetworkServer did not Set up as expected.");
                return;
            }

            m_SynchedServerData.map.OnValueChanged += OnServerChangedMap;
            m_SynchedServerData.gameMode.OnValueChanged += OnServerChangedMode;
        }

        async Task<MatchmakerAllocationPayload> GetPayloadWithinTimeout(int timeout)
        {
            if (m_MultiplayAllocationService == null)
                return null;

            //Try to get the matchmaker allocation payload from the multiplay services, and init the services if we do.
            var matchmakerPayloadTask = m_MultiplayAllocationService.BeginMatchplayServerAndAwaitMatchmakerAllocation();
            if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(timeout)) == matchmakerPayloadTask)
            {
                return matchmakerPayloadTask.Result;
            }

            return null;
        }

        async Task StartAllocationService(GameInfo startingGameInfo, ushort playerCount)
        {
            await m_MultiplayAllocationService.BeginServerCheck(startingGameInfo);
            m_MultiplayAllocationService.SetPlayerCount(playerCount);
        }

        async Task CreateAndStartBackfilling(MatchmakerAllocationPayload payload, GameInfo startingGameInfo)
        {
            m_Backfiller = new MatchplayBackfiller(connectionString, payload.QueueName, payload.MatchProperties,
                startingGameInfo.MaxUsers);

            if (m_Backfiller.NeedsPlayers())
            {
                await m_Backfiller.BeginBackfilling();
            }
        }

        #region ServerSynching

        //There are three data locations that need to be kept in sync, Game Server, Backfill Match Ticket, and the Multiplay Server
        //The Netcode Game Server is the source of truth, and we need to propagate the state of it to the multiplay server.
        //For the matchmaking ticket, it should already have knowledge of the players, unless a player joined outside of matchmaking.

        //For now we don't have any mechanics to change the gameMap or mode mid-game. But if we did, we would update the backfill ticket to reflect that too.
        void OnServerChangedMap(Map oldMap, Map newMap)
        {
            m_MultiplayAllocationService.ChangedMap(newMap);
        }

        void OnServerChangedMode(GameMode oldMode, GameMode newMode)
        {
            m_MultiplayAllocationService.ChangedMode(newMode);
        }

        void UserJoinedServer(UserData joinedUser)
        {
            m_Backfiller.AddPlayerToMatch(joinedUser);
            m_MultiplayAllocationService.AddPlayer();
            if (!m_Backfiller.NeedsPlayers() && m_Backfiller.Backfilling)
            {
#pragma warning disable 4014
                m_Backfiller.StopBackfill();
#pragma warning restore 4014
            }
        }

        void UserLeft(UserData leftUser)
        {
            m_Backfiller.RemovePlayerFromMatch(leftUser.userAuthId);
            m_MultiplayAllocationService.RemovePlayer();
            var playerCount = m_NetworkServer.PlayerCount;
            if (playerCount <= 0)
            {
#pragma warning disable 4014
                CloseServer();
#pragma warning restore 4014
                return;
            }

            if (m_Backfiller.NeedsPlayers() && !m_Backfiller.Backfilling)
            {
#pragma warning disable 4014
                m_Backfiller.BeginBackfilling();
#pragma warning restore 4014
            }
        }

        #endregion

        /// <summary>
        /// Take the list of players and find the most popular game preferences and run the server with those
        /// </summary>
        public static GameInfo PickSharedGameInfo(MatchmakerAllocationPayload mmAllocation)
        {
            var mapCounter = new Dictionary<Map, int>();
            var modeCounter = new Dictionary<GameMode, int>();

            //Gather all the modes all the players have selected
            foreach (var player in mmAllocation.MatchProperties.Players)
            {
                var playerGameInfo = player.CustomData.GetAs<GameInfo>();

                //Since we are using flags, each player might have more than one gameMap selected.
                foreach (var map in playerGameInfo.GetMap().GetUniqueFlags())
                {
                    if (mapCounter.ContainsKey(map))
                        mapCounter[map] += 1;
                    else
                        mapCounter[map] = 1;
                }

                foreach (var mode in playerGameInfo.GetMode().GetUniqueFlags())
                    if (modeCounter.ContainsKey(mode))
                        modeCounter[mode] += 1;
                    else
                        modeCounter[mode] = 1;
            }

            Map mostPopularMap = Map.None;
            int highestCount = 0;

            foreach (var (map, count) in mapCounter)
            {
                //Flip a coin for equally popular maps
                if (count == highestCount)
                {
                    if (Random.Range(0, 2) != 0)
                        mostPopularMap = map;
                    continue;
                }

                if (count > highestCount)
                {
                    mostPopularMap = map;
                    highestCount = count;
                }
            }

            GameMode mostPopularMode = GameMode.None;
            highestCount = 0;
            foreach (var (gameMode, count) in modeCounter)
            {
                //Flip a coin for equally popular modes
                if (count == highestCount)
                {
                    if (Random.Range(0, 2) != 0)
                        mostPopularMode = gameMode;
                    continue;
                }

                if (count > highestCount)
                {
                    mostPopularMode = gameMode;
                    highestCount = count;
                }
            }

            //Convert from the multiplay queue values to local enums
            var queue = GameInfo.ToGameQueue(mmAllocation.QueueName);
            var gameInfo = new GameInfo(queue, mostPopularMap, mostPopularMode);

            return gameInfo;
        }

        async Task CloseServer()
        {
            await m_Backfiller.StopBackfill();
            Dispose();
            Application.Quit();
        }

        public void Dispose()
        {
            if (!m_StartedServices)
            {
                if (m_NetworkServer.OnPlayerJoined != null) m_NetworkServer.OnPlayerJoined -= UserJoinedServer;
                if (m_NetworkServer.OnPlayerLeft != null) m_NetworkServer.OnPlayerLeft -= UserLeft;
            }

            if (m_SynchedServerData != null)
            {
                if (m_SynchedServerData.map.OnValueChanged != null)
                    m_SynchedServerData.map.OnValueChanged -= OnServerChangedMap;
                if (m_SynchedServerData.gameMode.OnValueChanged != null)
                    m_SynchedServerData.gameMode.OnValueChanged -= OnServerChangedMode;
            }

            m_Backfiller?.Dispose();
            m_MultiplayAllocationService?.Dispose();
            NetworkServer?.Dispose();
        }
    }
}