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
    public class ServerGameManager : MonoBehaviour
    {
        string m_ServerIP = "0.0.0.0";
        int m_ServerPort = 7777;
        int m_QueryPort = 7787;

        MatchplayGameInfo m_serverGameInfo = new MatchplayGameInfo
        {
            maxPlayers = 10,
            gameMode = GameMode.Staring,
            map = Map.Lab,
            gameQueue = GameQueue.Casual
        };

        UnitySqp m_UnitySqp;
        MatchplayServer m_Server;
        NetworkManager m_NetworkManager;

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
            m_ServerIP = ApplicationData.IP();
            m_ServerPort = ApplicationData.Port();
            m_QueryPort = ApplicationData.QPort();
            m_UnitySqp.StartSqp(m_ServerIP, m_ServerPort, m_QueryPort, m_serverGameInfo);
            m_Server.StartServer(m_ServerIP, m_ServerPort);
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            m_UnitySqp = new UnitySqp();
            m_NetworkManager = NetworkManager.Singleton;
            m_Server = new MatchplayServer();
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

        public void OnDestroy()
        {
            m_UnitySqp.Dispose();
            m_Server.Dispose();
            if (m_NetworkManager == null)
                return;
            m_NetworkManager.OnServerStarted -= OnServerStarted;
        }
    }
}
