using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Matchplay.Shared;
using Matchplay.Tools;
using Random = UnityEngine.Random;

namespace Matchplay.Server
{
    public class ServerGameManager : MonoBehaviour
    {
        public MatchplayNetworkServer networkServer => m_NetworkServer;

        MatchplayNetworkServer m_NetworkServer;
        MatchplayBackfiller m_Backfiller;
        string m_ConnectionString => $"{m_ServerIP}:{m_ServerPort}";
        string m_ServerIP = "0.0.0.0";
        int m_ServerPort = 7777;
        int m_QueryPort = 7787;
        int m_matchmakerInfoTimeout = 12000;

        MultiplayService m_MultiplayService;

        public static ServerGameManager Singleton
        {
            get
            {
                if (s_ServerGameManager != null) return s_ServerGameManager;
                s_ServerGameManager = FindObjectOfType<ServerGameManager>();
                if (s_ServerGameManager == null)
                {
                    Debug.LogError("No ClientGameManager in scene, did you run this from the bootStrap scene?");
                    return null;
                }

                return s_ServerGameManager;
            }
        }

        static ServerGameManager s_ServerGameManager;

        public void Init()
        {
            m_NetworkServer = new MatchplayNetworkServer();
            m_MultiplayService = new MultiplayService();
        }

        /// <summary>
        /// Attempts to initialize the server with services (If we are on Multiplay) and if we time out, we move on to default setup for local testing.
        /// </summary>
        public async Task BeginServerAsync()
        {
            m_ServerIP = CommandParser.IP();
            m_ServerPort = CommandParser.Port();
            m_QueryPort = CommandParser.QPort();

            var startingGameInfo = new GameInfo
            {
                gameMode = GameMode.Staring,
                map = Map.Lab,
                gameQueue = GameQueue.Casual
            };

            var matchmakerPayloadTask = m_MultiplayService.BeginServerAndAwaitMatchmakerAllocation();

            //Try to get the matchmaker allocation payload from the multiplay services, and init the services if we do.
            if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(m_matchmakerInfoTimeout)) == matchmakerPayloadTask)
            {
                if (matchmakerPayloadTask.Result != null)
                {
                    var matchmakerPayload = matchmakerPayloadTask.Result;
                    startingGameInfo = PayloadToMatchInfo(matchmakerPayload);

                    await m_MultiplayService.BeginServerCheck(startingGameInfo);

                    m_NetworkServer.OnPlayerJoined += UserJoinedServer;
                    m_NetworkServer.OnPlayerLeft += UserLeft;

                    m_Backfiller = new MatchplayBackfiller(m_ConnectionString, matchmakerPayload.QueueName, matchmakerPayload.MatchProperties, startingGameInfo.MaxUsers);

                    if (m_Backfiller.NeedsPlayers())
                    {
                        await m_Backfiller.CreateNewbackfillTicket();
                    }
                }
            }
            else
            {
                Debug.LogWarning("Connecting to Multiplay Timed out, starting with defaults.");
            }

            m_NetworkServer.StartServer(m_ServerIP, m_ServerPort, startingGameInfo); //Use Network transforms on the chairs/players to sync positions
        }

        void UserJoinedServer(UserData joinedUser)
        {
            m_Backfiller.AddPlayerToMatch(joinedUser);
            if (!m_Backfiller.NeedsPlayers() && m_Backfiller.Backfilling)
                Task.Run(() => m_Backfiller.StopBackfill());
        }

        void UserLeft(UserData leftUser)
        {
            m_Backfiller.RemovePlayerFromMatch(leftUser.userAuthId);
            if (m_Backfiller.NeedsPlayers() && !m_Backfiller.Backfilling)
                Task.Run(() => m_Backfiller.CreateNewbackfillTicket());
        }

        /// <summary>
        /// Take the list of players and find the most popular game preferences and run the server with those
        /// </summary>
        GameInfo PayloadToMatchInfo(MatchmakerAllocationPayload mmAllocation)
        {
            var mapCounter = new Dictionary<Map, int>();
            var modeCounter = new Dictionary<GameMode, int>();

            //Gather all the modes all the players have selected
            foreach (var player in mmAllocation.MatchProperties.Players)
            {
                var playerGameInfo = player.CustomData.GetAs<GameInfo>();

                //Since we are using flags, each player might have more than one map selected.
                foreach (var flag in playerGameInfo.map.GetUniqueFlags())
                    mapCounter[flag] += 1;
                foreach (var mode in playerGameInfo.gameMode.GetUniqueFlags())
                    modeCounter[mode] += 1;
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
                //Flip a coin for equally popular maps
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

            return new GameInfo { map = mostPopularMap, gameMode = mostPopularMode, gameQueue = queue };
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void OnDestroy()
        {
            if (m_NetworkServer.OnPlayerJoined != null) m_NetworkServer.OnPlayerJoined -= UserJoinedServer;
            if (m_NetworkServer.OnPlayerLeft != null) m_NetworkServer.OnPlayerLeft -= UserLeft;
            m_Backfiller?.Dispose();
            m_MultiplayService?.Dispose();
            networkServer?.Dispose();
        }
    }
}
