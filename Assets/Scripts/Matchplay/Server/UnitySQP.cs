using System;
using System.Net;
using Unity.Netcode;
using Unity.Ucg.Usqp;
using UnityEngine;
using Matchplay.Shared;
using Matchplay.Infrastructure;

namespace Matchplay.Server
{
    public class UnitySqp : IDisposable
    {
        ServerInfo.Data m_SqpServerData;
        UsqpServer m_Server;
        UpdateRunner m_UpdateRunner;
        NetworkManager m_NetworkManager;

        public void StartSqp(string ip, int port, int sqpPort, MatchplayGameInfo matchplayGameMode)
        {
            m_NetworkManager.OnClientConnectedCallback += OnPlayerCountChanged;
            m_NetworkManager.OnClientDisconnectCallback += OnPlayerCountChanged;

            //m_NetworkManager.SceneManager.OnSceneEvent += OnSceneChanged;

            m_SqpServerData = new ServerInfo.Data
            {
                BuildId = "1",
                CurrentPlayers = 0,
                GameType = matchplayGameMode.gameMode.ToString(),
                Map = matchplayGameMode.map.ToString(),
                MaxPlayers = (ushort)matchplayGameMode.maxPlayers,
                Port = (ushort)port,
                ServerName = "Matchplay Server"
            };

            var parsedIP = IPAddress.Parse(ip);
            var endpoint = new IPEndPoint(parsedIP, sqpPort);
            m_Server = new UsqpServer(endpoint)
            {
                // Use our GameObject's SQP data as the server's data
                ServerInfoData = m_SqpServerData
            };
            m_UpdateRunner?.Subscribe(Update, 0.5f);
        }

        [Inject]
        void InjectDependencies(NetworkManager manager, UpdateRunner updateRunner)
        {
            m_UpdateRunner = updateRunner;
            m_NetworkManager = manager;
        }

        void Update(float deltaTime)
        {
            m_Server?.Update();
        }

        void OnPlayerCountChanged(ulong clientId)
        {
            m_SqpServerData.CurrentPlayers = (ushort)m_NetworkManager.ConnectedClients.Count;
        }

        void OnSceneChanged(SceneEvent sceneEvent)
        {
            m_SqpServerData.Map = sceneEvent.SceneName;
        }

        public void Dispose()
        {
            if (m_NetworkManager == null)
                return;
            m_NetworkManager.OnClientConnectedCallback -= OnPlayerCountChanged;
            m_NetworkManager.OnClientDisconnectCallback -= OnPlayerCountChanged;
            m_UpdateRunner?.Unsubscribe(Update);
            m_Server?.Dispose();
        }
    }
}
