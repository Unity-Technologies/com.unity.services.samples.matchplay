using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Matchplay.Networking;
using UnityEngine.SceneManagement;

namespace Matchplay.Client
{
    public class MatchplayNetworkClient : IDisposable
    {
        public event Action<ConnectStatus> OnLocalConnection;
        public event Action<ConnectStatus> OnLocalDisconnection;

        /// <summary>
        /// Time in seconds before the client considers a lack of server response a timeout
        /// </summary>
        const int k_TimeoutDuration = 10;
        NetworkManager m_NetworkManager;

        /// <summary>
        /// If a disconnect occurred this will be populated with any contextual information that was available to explain why.
        /// </summary>
        DisconnectReason DisconnectReason { get; } = new DisconnectReason();

        public MatchplayNetworkClient()
        {
            m_NetworkManager = NetworkManager.Singleton;
            m_NetworkManager.OnClientDisconnectCallback += RemoteDisconnect;
        }

        /// <summary>
        /// Wraps the invocation of NetworkManager.BootClient, including our GUID as the payload.
        /// </summary>
        /// <param name="ipaddress">the IP address of the host to connect to. (currently IPV4 only)</param>
        /// <param name="port">The port of the host to connect to. </param>
        public void StartClient(string ipaddress, int port)
        {
            var unityTransport = m_NetworkManager.gameObject.GetComponent<UnityTransport>();
            unityTransport.SetConnectionData(ipaddress, (ushort)port);
            ConnectClient();
        }

        public void DisconnectClient()
        {
            DisconnectReason.SetDisconnectReason(ConnectStatus.UserRequestedDisconnect);
            NetworkShutdown();
        }

        /// <summary>
        /// Sends some additional data to the server about the client and begins connecting them.
        /// </summary>
        void ConnectClient()
        {
            var userData = ClientSingleton.Instance.Manager.User.Data;
            var payload = JsonUtility.ToJson(userData);

            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            m_NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
            m_NetworkManager.NetworkConfig.ClientConnectionBufferTimeout = k_TimeoutDuration;

            //  If the socket connection fails, we'll hear back by getting an ReceiveLocalClientDisconnectStatus callback for ourselves and get a message telling us the reason
            //  If the socket connection succeeds, we'll get our  ReceiveLocalClientConnectStatus callback This is where game-layer failures will be reported.
            if (m_NetworkManager.StartClient())
            {
                Debug.Log("Starting Client!");
                MatchplayNetworkMessenger.RegisterListener(NetworkMessage.LocalClientConnected,
                    ReceiveLocalClientConnectStatus);
                MatchplayNetworkMessenger.RegisterListener(NetworkMessage.LocalClientDisconnected,
                    ReceiveLocalClientDisconnectStatus);
            }
            else
            {
                Debug.LogWarning($"Could not Start Client!");
                OnLocalDisconnection?.Invoke(ConnectStatus.Undefined);
            }
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

        void RemoteDisconnect(ulong clientId)
        {
            Debug.Log($"Got Client Disconnect callback for {clientId}");
            if (clientId == m_NetworkManager.LocalClientId)
                return;
            NetworkShutdown();
        }

        void NetworkShutdown()
        {
            //On a client disconnect we want to take them back to the main menu.
            //We have to check here in SceneManager if our active scene is the main menu, as if it is, it means we timed out rather than a raw disconnect;
            if (SceneManager.GetActiveScene().name != "mainMenu")
                SceneManager.LoadScene("mainMenu");
                
            // We're not at the main menu, so we obviously had a connection before... thus, we aren't in a timeout scenario.
            // Just shut down networking and switch back to main menu.
            if (m_NetworkManager.IsConnectedClient)
                m_NetworkManager.Shutdown(true);
            OnLocalDisconnection?.Invoke(DisconnectReason.Reason);
            MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.LocalClientConnected);
            MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.LocalClientDisconnected);
        }

        public void Dispose()
        {
            if (m_NetworkManager != null && m_NetworkManager.CustomMessagingManager != null)
            {
                m_NetworkManager.OnClientDisconnectCallback -= RemoteDisconnect;
            }
        }
    }
}