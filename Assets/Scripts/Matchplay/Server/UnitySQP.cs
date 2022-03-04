using System;
using System.Net;
using Unity.Netcode;
using Unity.Ucg.Usqp;
using UnityEngine;
using Matchplay.Shared;
using Matchplay.Infrastructure;
using Matchplay.Networking;

namespace Matchplay.Server
{
    public class UnitySqp : IDisposable
    {
        ServerInfo.Data m_SqpServerData;
        UsqpServer m_Server;
        UpdateRunner m_UpdateRunner;

        public void StartSqp(string ip, int port, int sqpPort)
        {
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.ConnectionResult, OnPlayerAdded);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.DisconnectionResult, OnPlayerRemoved);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.ServerChangedMap, OnMapChanged);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.ServerChangedGameMode, OnModeChanged);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.ServerChangedQueue, OnGameQueueChanged);

            //m_NetworkManager.SceneManager.OnSceneEvent += OnSceneChanged;
            m_SqpServerData = new ServerInfo.Data
            {
                BuildId = "1",
                CurrentPlayers = 0,
                GameType = "",
                Map = "",
                MaxPlayers = 10,
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

        void Update(float deltaTime)
        {
            m_Server?.Update();
        }

        void OnPlayerAdded(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ConnectStatus status);
            if (status == ConnectStatus.Success)
                m_SqpServerData.CurrentPlayers += 1;
        }

        void OnPlayerRemoved(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ConnectStatus status);
            if (status == ConnectStatus.GenericDisconnect)
                m_SqpServerData.CurrentPlayers -= 1;
        }

        void OnMapChanged(ulong unused, FastBufferReader reader)
        {
            reader.ReadValueSafe(out Map map);
            m_SqpServerData.Map = map.ToString();
        }

        void OnModeChanged(ulong unused, FastBufferReader reader)
        {
            reader.ReadValueSafe(out GameMode mode);
            m_SqpServerData.GameType = mode.ToString();
        }

        void OnGameQueueChanged(ulong unused, FastBufferReader reader)
        {
            reader.ReadValueSafe(out GameQueue gameQueue);
            m_SqpServerData.ServerName = $"{gameQueue.ToString()} Matchplay Server";
        }

        public void Dispose()
        {
            MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.ConnectionResult);
            MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.DisconnectionResult);
            MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.ServerChangedMap);
            MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.ServerChangedGameMode);
            MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.ServerChangedQueue);
            m_UpdateRunner?.Unsubscribe(Update);
            m_Server?.Dispose();
        }
    }
}
