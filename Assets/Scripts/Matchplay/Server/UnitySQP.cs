using System;
using System.Net;
using Unity.Netcode;
using Unity.Ucg.Usqp;
using UnityEngine;
using Matchplay.Shared;
using Matchplay.Shared.Infrastructure;

namespace Matchplay.Server
{
    public class UnitySqp : IDisposable
    {
        ServerInfo.Data m_SqpServerData;
        UsqpServer m_Server;
        UpdateRunner m_UpdateRunner;

        public void StartSqp(string ip, int port, int sqpPort, MatchplayGameInfo matchplayGameMode)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerCountChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerCountChanged;

            //m_NetworkManager.SceneManager.OnSceneEvent += OnSceneChanged;

            m_SqpServerData = new ServerInfo.Data
            {
                BuildId = "1",
                CurrentPlayers = 0,
                GameType = matchplayGameMode.CurrentGameMode.ToString(),
                Map = matchplayGameMode.CurrentMap.ToString(),
                MaxPlayers = (ushort)matchplayGameMode.MaxPlayers,
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
        void InjectDependencies(UpdateRunner updateRunner)
        {
            m_UpdateRunner = updateRunner;
        }

        void Update(float deltaTime)
        {
            m_Server?.Update();
        }

        void OnPlayerCountChanged(ulong clientId)
        {
            m_SqpServerData.CurrentPlayers = (ushort)NetworkManager.Singleton.ConnectedClients.Count;
        }

        void OnSceneChanged(SceneEvent sceneEvent)
        {
            m_SqpServerData.Map = sceneEvent.SceneName;
        }

        public void Dispose()
        {
            if (NetworkManager.Singleton == null)
                return;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerCountChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerCountChanged;
            m_UpdateRunner?.Unsubscribe(Update);
            m_Server?.Dispose();
        }
    }
}
