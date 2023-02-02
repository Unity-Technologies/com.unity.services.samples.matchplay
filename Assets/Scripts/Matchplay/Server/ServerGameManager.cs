using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Matchplay.Shared;
using Matchplay.Shared.Tools;
using Unity.Netcode;
using Random = UnityEngine.Random;
using Unity.Services.Matchmaker.Models;

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
        const int k_MultiplayServiceTimeout = 20000;
        bool m_StartedServices;
        MultiplayAllocationService m_MultiplayAllocationService;
        MultiplayServerQueryService m_MultiplayServerQueryService;
        SynchedServerData m_SynchedServerData;
        string m_ServerName = "Matchplay Server";

        public ServerGameManager(string serverIP, int serverPort, int serverQPort, NetworkManager manager)
        {
            m_ServerIP = serverIP;
            m_ServerPort = serverPort;
            m_QueryPort = serverQPort;
            m_NetworkServer = new MatchplayNetworkServer(manager);
            m_MultiplayServerQueryService = new MultiplayServerQueryService();
            m_ServerName = NameGenerator.GetName(Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Attempts to initialize the server with services (If we are on Multiplay) and if we time out, we move on to default setup for local testing.
        /// </summary>
        public async Task StartGameServerAsync(GameInfo startingGameInfo)
        {
            Debug.Log($"Starting server with:{startingGameInfo}.");

            // The server should respond to query requests irrespective of the server being allocated.
            // Hence, start the handler as soon as we can.
            await m_MultiplayServerQueryService.BeginServerQueryHandler();

            try
            {
                var matchmakerPayload = await GetMatchmakerPayload(k_MultiplayServiceTimeout);

                if (matchmakerPayload != null)
                {
                    Debug.Log($"Got payload: {matchmakerPayload}");
                    startingGameInfo = PickGameInfo(matchmakerPayload);

                    StartAllocationService(startingGameInfo,
                        (ushort)matchmakerPayload.MatchProperties.Players.Count);
                    await StartBackfill(matchmakerPayload, startingGameInfo);
                    m_NetworkServer.OnPlayerJoined += UserJoinedServer;
                    m_NetworkServer.OnPlayerLeft += UserLeft;
                    m_StartedServices = true;
                }
                else
                {
                    Debug.LogWarning("Getting the Matchmaker Payload timed out, starting with defaults.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Something went wrong trying to set up the Services:\n{ex} ");
            }

            if (!m_NetworkServer.OpenConnection(m_ServerIP, m_ServerPort, startingGameInfo))
            {
                Debug.LogError("NetworkServer did not start as expected.");
                return;
            }

            //Changes Map and sets the synched shared variables to the starting info
            m_SynchedServerData = await m_NetworkServer.ConfigureServer(startingGameInfo);
            if (m_SynchedServerData == null)
            {
                Debug.LogError("Could not find the synchedServerData.");
                return;
            }

            m_SynchedServerData.serverID.Value = m_ServerName;

            m_SynchedServerData.map.OnValueChanged += OnServerChangedMap;
            m_SynchedServerData.gameMode.OnValueChanged += OnServerChangedMode;
        }

        async Task<MatchmakingResults> GetMatchmakerPayload(int timeout)
        {
            if (m_MultiplayAllocationService == null)
                return null;

            //Try to get the matchmaker allocation payload from the multiplay services, and init the services if we do.
            var matchmakerPayloadTask = m_MultiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

            //If we don't get the payload by the timeout, we stop trying.
            if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(timeout)) == matchmakerPayloadTask)
            {
                return matchmakerPayloadTask.Result;
            }

            return null;
        }

        private void StartAllocationService(GameInfo startingGameInfo, ushort playerCount)
        {
            //Create a unique name for the server to show that we are joining the same one

            m_MultiplayServerQueryService.SetServerName(m_ServerName);
            m_MultiplayServerQueryService.SetPlayerCount(playerCount);
            m_MultiplayServerQueryService.SetMaxPlayers(10);
            m_MultiplayServerQueryService.SetBuildID("0");
            m_MultiplayServerQueryService.SetMap(startingGameInfo.map.ToString());
            m_MultiplayServerQueryService.SetMode(startingGameInfo.gameMode.ToString());
        }

        async Task StartBackfill(MatchmakingResults payload, GameInfo startingGameInfo)
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

        //For now we don't have any mechanics to change the map or mode mid-game. But if we did, we would update the backfill ticket to reflect that too.
        void OnServerChangedMap(Map oldMap, Map newMap)
        {
            m_MultiplayServerQueryService.SetMap(newMap.ToString());
        }

        void OnServerChangedMode(GameMode oldMode, GameMode newMode)
        {
            m_MultiplayServerQueryService.SetMode(newMode.ToString());
        }
        void UserJoinedServer(UserData joinedUser)
        {
            Debug.Log($"{joinedUser} joined the game");
            m_Backfiller.AddPlayerToMatch(joinedUser);
            m_MultiplayServerQueryService.AddPlayer();
            if (!m_Backfiller.NeedsPlayers() && m_Backfiller.Backfilling)
            {
#pragma warning disable 4014
                m_Backfiller.StopBackfill();
#pragma warning restore 4014
            }
        }

        void UserLeft(UserData leftUser)
        {
            var playerCount = m_Backfiller.RemovePlayerFromMatch(leftUser.userAuthId);
            m_MultiplayServerQueryService.RemovePlayer();

            Debug.Log($"player '{leftUser?.userName}' left the game, {playerCount} players left in game.");
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
        public static GameInfo PickGameInfo(MatchmakingResults mmAllocation)
        {
            //All the players should have the same info, so we just pick the first one to use as the starter.
            var chosenMap = Map.Lab;
            var chosenMode = GameMode.Staring;

            foreach (var player in mmAllocation.MatchProperties.Players)
            {
                var playerGameInfo = player.CustomData.GetAs<GameInfo>();
                chosenMap = playerGameInfo.map;
                chosenMode = playerGameInfo.gameMode;
            }

            var queue = GameInfo.ToGameQueue(mmAllocation.QueueName);
            return new GameInfo { map = chosenMap, gameMode = chosenMode, gameQueue = queue };
        }

        async Task CloseServer()
        {
            Debug.Log($"Closing Server");
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