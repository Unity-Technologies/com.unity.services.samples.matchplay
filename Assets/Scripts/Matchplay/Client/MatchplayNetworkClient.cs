using System;
using UnityEngine;
using Unity.Netcode;
using Matchplay.Networking;
using Matchplay.Server;
using Matchplay.Shared;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;

namespace Matchplay.Client
{
    public class MatchplayNetworkClient : IDisposable
    {
        public event Action<Map> OnServerChangedMap;
        public event Action<GameMode> OnServerChangedMode;
        public event Action<GameQueue> OnServerChangedQueue;

        public event Action<ConnectStatus> OnLocalConnection;
        public event Action<ConnectStatus> OnLocalDisconnection;

        ulong networkClientId => m_NetworkManager.LocalClientId;

        /// <summary>
        /// Time in seconds before the client considers a lack of server response a timeout
        /// </summary>
        const int k_TimeoutDuration = 10;
        NetworkManager m_NetworkManager;

        /// <summary>
        /// If a disconnect occurred this will be populated with any contextual information that was available to explain why.
        /// </summary>
        DisconnectReason DisconnectReason { get; } = new DisconnectReason();

        /// <summary>
        /// Wraps the invocation of NetworkManager.BootClient, including our GUID as the payload.
        /// </summary>
        /// <remarks>
        /// This method must be static because, when it is invoked, the client still doesn't know it's a client yet, and in particular, GameNetPortal hasn't
        /// yet initialized its client and server GNP-Logic objects yet (which it does in OnNetworkSpawn, based on the role that the current player is performing).
        /// </remarks>
        /// <param name="portal"> </param>
        /// <param name="ipaddress">the IP address of the host to connect to. (currently IPV4 only)</param>
        /// <param name="port">The port of the host to connect to. </param>
        public void StartClient(string ipaddress, int port)
        {
            var unityTransport = m_NetworkManager.gameObject.GetComponent<UnityTransport>();
            unityTransport.SetConnectionData(ipaddress, (ushort)port);
            ConnectClient();
        }

        /// <summary>
        /// Invoked when the user has requested a disconnect via the UI, e.g. when hitting "Return to Main Menu" in the post-game scene.
        /// </summary>
        public void OnUserDisconnectRequest()
        {
            if (m_NetworkManager.IsClient)
            {
                DisconnectReason.SetDisconnectReason(ConnectStatus.UserRequestedDisconnect);
                m_NetworkManager.DisconnectClient(networkClientId);
            }
        }

        public MatchplayNetworkClient()
        {
            m_NetworkManager = NetworkManager.Singleton;
            m_NetworkManager.OnClientDisconnectCallback += LocalClientDisconnect;
        }

        Matchplayer GetMatchPlayer(ulong clientId)
        {
            var client = m_NetworkManager.ConnectedClients[clientId].PlayerObject;
            return client.GetComponent<Matchplayer>();
        }

        /// <summary>
        /// Sends some additional data to the server about the client and begins connecting them.
        /// </summary>
        void ConnectClient()
        {
            var userData = ClientGameManager.Singleton.observableUser.Data;
            var payload = JsonUtility.ToJson(userData);

            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            m_NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
            m_NetworkManager.NetworkConfig.ClientConnectionBufferTimeout = k_TimeoutDuration;

            //  If the socket connection fails, we'll hear back by getting an ReceiveLocalClientDisconnectStatus callback for ourselves and get a message telling us the reason
            //  If the socket connection succeeds, we'll get our  ReceiveLocalClientConnectStatus callback This is where game-layer failures will be reported.
            m_NetworkManager.StartClient();
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.LocalClientConnected, ReceiveLocalClientConnectStatus);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.LocalClientDisconnected, ReceiveLocalClientDisconnectStatus);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.ServerChangedMap, ReceiveServerMap);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.ServerChangedQueue, ReceiveServerMode);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.ServerChangedGameMode, ReceiveServerQueue);
        }

        void ReceiveLocalClientConnectStatus(ulong clientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ConnectStatus status);
            Debug.Log("ReceiveLocalClientConnectStatus: " + status);

            //this indicates a game level failure, rather than a network failure. See note in ServerGameNetPortal.
            if (status != ConnectStatus.Success)
                DisconnectReason.SetDisconnectReason(status);

            OnLocalConnection?.Invoke(status);
        }

        void ReceiveLocalClientDisconnectStatus(ulong clientId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ConnectStatus status);
            Debug.Log("ReceiveLocalClientDisconnectStatus: " + status);
            DisconnectReason.SetDisconnectReason(status);
        }


        //TODO RECIEVED MESSAGES FROM SERVER ARE WRONG?!
        //These Messages are sent to all clients
        void ReceiveServerMap(ulong unused, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int map);
            var serverMap = (Map)map;
            Debug.Log($"ReceiveServerMap: {serverMap}");
            OnServerChangedMap?.Invoke(serverMap);
        }

        void ReceiveServerMode(ulong unused, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int gameInfo);
            var serverMode = (GameMode)gameInfo;
            Debug.Log($"ReceiveServerMode: {serverMode}");
            OnServerChangedMode?.Invoke(serverMode);
        }

        void ReceiveServerQueue(ulong unused, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int gamequeue);
            var serverGameQueue = (GameQueue)gamequeue;
            Debug.Log($"ReceiveServerQueue: {serverGameQueue}");
            OnServerChangedQueue?.Invoke(serverGameQueue);
        }

        void LocalClientDisconnect(ulong clientId)
        {
            if (clientId == networkClientId)
                return;

            //On a client disconnect we want to take them back to the main menu.
            //We have to check here in SceneManager if our active scene is the main menu, as if it is, it means we timed out rather than a raw disconnect;
            if (SceneManager.GetActiveScene().name == "mainMenu")
                return;

            // We're not at the main menu, so we obviously had a connection before... thus, we aren't in a timeout scenario.
            // Just shut down networking and switch back to main menu.
            m_NetworkManager.Shutdown();

            OnLocalDisconnection?.Invoke(DisconnectReason.Reason);

            SceneManager.LoadScene("mainMenu");
        }

        public void Dispose()
        {
            if (m_NetworkManager != null && m_NetworkManager.CustomMessagingManager != null)
            {
                m_NetworkManager.OnClientDisconnectCallback -= LocalClientDisconnect;
                MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.LocalClientConnected);
                MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.LocalClientDisconnected);
                MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.ServerChangedMap);
                MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.ServerChangedQueue);
                MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.ServerChangedGameMode);
            }
        }
    }
}
