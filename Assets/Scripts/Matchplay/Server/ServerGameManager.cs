using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Matchplay.Shared;
using Matchplay.Infrastructure;
using Matchplay.Tools;

namespace Matchplay.Server
{
    public class ServerGameManager : IDisposable
    {
        string m_ServerIP = "0.0.0.0";
        int m_ServerPort = 7777;
        int m_QueryPort = 7787;

        MatchplayGameInfo m_serverGameInfo = new MatchplayGameInfo()
        {
            maxPlayers = 10,
            gameMode = GameMode.Staring,
            map = Map.Lab,
            gameQueue = GameQueue.Casual
        };

        UnitySqp m_UnitySqp;
        MatchplayServer m_Server;
        ApplicationData m_Data;
        NetworkManager m_NetworkManager;

        [Inject]
        void InjectDependencies(NetworkManager manager, MatchplayServer server, UnitySqp sqpServer, ApplicationData data)
        {
            m_NetworkManager = manager;
            m_Data = data;
            m_UnitySqp = sqpServer;
            m_Server = server;
            m_Server.Init();
        }

        public void SetGameMode(GameMode toGameMode)
        {
            m_serverGameInfo.gameMode = toGameMode;
        }

        public void ChangeMap(Map toMap)
        {
            m_serverGameInfo.map = toMap;
            var sceneString = ToScene(m_serverGameInfo.map);
            if (string.IsNullOrEmpty(sceneString))
            {
                Debug.LogError($"Cant Change map, no valid map selection in {toMap}.");
                return;
            }

            m_NetworkManager.SceneManager.LoadScene(ToScene(m_serverGameInfo.map), LoadSceneMode.Single);
        }

        public void BeginServer()
        {
            m_NetworkManager.OnServerStarted += OnServerStarted;
            m_ServerIP = m_Data.IP();
            m_ServerPort = m_Data.Port();
            m_QueryPort = m_Data.QPort();
            m_UnitySqp.StartSqp(m_ServerIP, m_ServerPort, m_QueryPort, m_serverGameInfo);
            m_Server.StartServer(m_ServerIP, m_ServerPort);
        }

        /// <summary>
        /// Convert the map flag enum to a scene name.
        /// </summary>
        string ToScene(Map maps)
        {
            var mapSelection = new List<Map>(maps.GetUniqueFlags());
            if (mapSelection.Count < 1)
            {
                return "";
            }

            var topMap = mapSelection.First();
            if (topMap.HasFlag(Map.Lab))
            {
                return "game_lab";
            }

            if (topMap.HasFlag(Map.Space))
            {
                return "game_space";
            }

            return "";
        }

        /// <summary>
        /// Will assure clients connect and join the same map.
        /// </summary>
        void OnServerStarted()
        {
            ChangeMap(Map.Lab);
        }

        public void Dispose()
        {
            if (m_NetworkManager == null)
                return;
            m_NetworkManager.OnServerStarted -= OnServerStarted;
        }
    }
}
