using System;
using UnityEngine;
using Matchplay.Shared;

namespace Matchplay.Server
{
    public class ServerGameManager : MonoBehaviour
    {
        public MatchplayNetworkServer networkServer => m_NetworkServer;
        MatchplayNetworkServer m_NetworkServer;
        string m_ServerIP = "0.0.0.0";
        int m_ServerPort = 7777;
        int m_QueryPort = 7787;
        UnitySqp m_UnitySqp;

        bool m_GameSetup = false;

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
        }

        public void BeginServer()
        {
            m_ServerIP = ApplicationData.IP();
            m_ServerPort = ApplicationData.Port();
            m_QueryPort = ApplicationData.QPort();
            networkServer.StartServer(m_ServerIP, m_ServerPort);
            m_UnitySqp = new UnitySqp();
            m_UnitySqp.StartSqp(m_ServerIP, m_ServerPort, m_QueryPort);
            networkServer.ToWaitingScene();
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void OnDestroy()
        {
            m_UnitySqp?.Dispose();
            networkServer?.Dispose();
        }
    }
}
