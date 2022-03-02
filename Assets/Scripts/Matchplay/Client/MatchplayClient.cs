using System;
using UnityEngine;
using Unity.Netcode;
using Matchplay.Networking;
using Matchplay.Shared;
using Matchplay.Infrastructure;
using UnityEngine.SceneManagement;

namespace Matchplay.Client
{
    public class MatchplayClient : IDisposable
    {
        /// <summary>
        /// If a disconnect occurred this will be populated with any contextual information that was available to explain why.
        /// </summary>
        public DisconnectReason DisconnectReason { get; } = new DisconnectReason();

        /// <summary>
        /// Time in seconds before the client considers a lack of server response a timeout
        /// </summary>
        private const int k_TimeoutDuration = 10;

        public event Action<ConnectStatus> ConnectFinished;

        /// <summary>
        /// This event fires when the client sent out a request to start the client, but failed to hear back after an allotted amount of
        /// time from the host.
        /// </summary>
        public event Action NetworkTimedOut;

        NetworkManager m_NetworkManager;

        /// <summary>
        /// Invoked when the user has requested a disconnect via the UI, e.g. when hitting "Return to Main Menu" in the post-game scene.
        /// </summary>
        public void OnUserDisconnectRequest()
        {
            if (m_NetworkManager.IsClient)
            {
                DisconnectReason.SetDisconnectReason(ConnectStatus.UserRequestedDisconnect);
                m_NetworkManager.DisconnectClient(NetworkManager.Singleton.LocalClientId);
            }
        }

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

        public MatchplayClient()
        {
            m_NetworkManager = NetworkManager.Singleton;
            m_NetworkManager.OnClientDisconnectCallback += OnDisconnectOrTimeout;
        }

        void ConnectClient()
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload
            {
                clientGUID = ClientPrefs.GetGuid(),
                playerName = ClientPrefs.PlayerName
            });

            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            m_NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
            m_NetworkManager.NetworkConfig.ClientConnectionBufferTimeout = k_TimeoutDuration;

            //and...we're off! Netcode will establish a socket connection to the host.
            //  If the socket connection fails, we'll hear back by getting an OnClientDisconnect callback for ourselves and get a message telling us the reason
            //  If the socket connection succeeds, we'll get our RecvConnectFinished invoked. This is where game-layer failures will be reported.
            m_NetworkManager.StartClient();

            // should only do this once BootClient has been called (start client will initialize CustomMessagingManager
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.ConnectionResult, ReceiveServerToClientConnectResult_CustomMessage);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.DisconnectionResult, ReceiveServerToClientSetDisconnectReason_CustomMessage);

            //TODO YAGNI Custom Disconnect messags?
        }

        void ReceiveServerToClientConnectResult_CustomMessage(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ConnectStatus status);
            OnConnectFinished(status);
        }

        void ReceiveServerToClientSetDisconnectReason_CustomMessage(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ConnectStatus status);
            OnDisconnectReasonReceived(status);
        }

        public void RecieveMatchplayGameInfo_CustomMessage(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out MatchplayGameInfo gameInfo);
            Debug.Log($"Got GameInfo from server. {gameInfo}");
        }

        void OnConnectFinished(ConnectStatus status)
        {
            //on success, there is nothing to do (the Netcode for GameObjects (Netcode) scene management system will take us to the next scene).
            //on failure, we must raise an event so that the UI layer can display something.
            Debug.Log("RecvConnectFinished Got status: " + status);

            if (status != ConnectStatus.Success)
            {
                //this indicates a game level failure, rather than a network failure. See note in ServerGameNetPortal.
                DisconnectReason.SetDisconnectReason(status);
            }

            ConnectFinished?.Invoke(status);
        }

        void OnDisconnectReasonReceived(ConnectStatus status)
        {
            DisconnectReason.SetDisconnectReason(status);
        }

        void OnDisconnectOrTimeout(ulong clientID)
        {
            // we could also check whether the disconnect was us or the host, but the "interesting" question is whether
            //following the disconnect, we're no longer a Connected Client, so we just explicitly check that scenario.
            if (!NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsHost)
            {
                //On a client disconnect we want to take them back to the main menu.
                //We have to check here in SceneManager if our active scene is the main menu, as if it is, it means we timed out rather than a raw disconnect;
                if (SceneManager.GetActiveScene().name != "mainMenu")
                {
                    // we're not at the main menu, so we obviously had a connection before... thus, we aren't in a timeout scenario.
                    // Just shut down networking and switch back to main menu.
                    m_NetworkManager.Shutdown();
                    if (!DisconnectReason.HasTransitionReason)
                    {
                        //disconnect that happened for some other reason than user UI interaction--should display a message.
                        DisconnectReason.SetDisconnectReason(ConnectStatus.GenericDisconnect);
                    }

                    SceneManager.LoadScene("mainMenu");
                }
                else if (DisconnectReason.Reason == ConnectStatus.GenericDisconnect || DisconnectReason.Reason == ConnectStatus.Undefined)
                {
                    // only call this if generic disconnect. Else if there's a reason, there's already code handling that popup
                    NetworkTimedOut?.Invoke();
                }
            }
        }

        public void Dispose()
        {
            if (NetworkManager.Singleton != null && m_NetworkManager.CustomMessagingManager != null)
            {
                m_NetworkManager.OnClientDisconnectCallback -= OnDisconnectOrTimeout;

                MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.ConnectionResult);
                MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.DisconnectionResult);
            }
        }
    }
}
