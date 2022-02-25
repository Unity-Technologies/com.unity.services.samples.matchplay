using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Matchplay.Networking;
using Matchplay.Server;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Matchplay.Shared;

namespace Matchplay.Server
{
    public class MatchplayServer : IDisposable
    {
        public NetworkVariable<MatchplayGameInfo> MatchInfo;
        public UnityEvent<PlayerData?> OnPlayerConnected;
        public UnityEvent<PlayerData?> OnPlayerDisconnected;

        // used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
        private const int k_MaxConnectPayload = 1024;

        /// <summary>
        /// Map a given client guid to the data for a given client player.
        /// </summary>
        private Dictionary<string, PlayerData> m_ClientData = new Dictionary<string, PlayerData>();

        /// <summary>
        /// Map to allow us to cheaply map from guid to player data.
        /// </summary>
        private Dictionary<ulong, string> m_ClientIdToGuid = new Dictionary<ulong, string>();

        /// <summary>
        /// Convenience method to get player name from player data
        /// Returns name in data or default name using playerNum
        /// </summary>
        public string GetPlayerName(ulong clientId, int playerNum)
        {
            var playerData = GetPlayerData(clientId);
            return (playerData != null) ? playerData.Value.m_PlayerName : ("Player" + playerNum);
        }

        public void StartServer(string ip, int port)
        {
            var unityTransport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
            unityTransport.SetConnectionData(ip, (ushort)port);
            NetworkManager.Singleton.StartServer();
        }

        /// <summary>
        /// Handles the flow when a user has requested a disconnect via UI (which can be invoked on the Host, and thus must be
        /// handled in server code).
        /// </summary>
        public void OnUserDisconnectRequest()
        {
            Clear();
        }

        public void Init()
        {
            // we add ApprovalCheck callback BEFORE OnNetworkSpawn to avoid spurious Netcode for GameObjects (Netcode)
            // warning: "No ConnectionApproval callback defined. Connection approval will timeout"
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="clientId"> guid of the client whose data is requested</param>
        /// <returns>Player data struct matching the given ID</returns>
        PlayerData? GetPlayerData(ulong clientId)
        {
            //First see if we have a guid matching the clientID given.
            Debug.Log($"Attempting to get player data for: {clientId}");
            if (m_ClientIdToGuid.TryGetValue(clientId, out var clientguid))
            {
                if (m_ClientData.TryGetValue(clientguid, out var playerData))
                    return playerData;

                Debug.LogError($"No PlayerData of matching guid found: {clientguid}");
            }
            else
            {
                Debug.LogError($"No client guid found mapped to the given client ID: {clientId}");
            }

            return null;
        }

        void OnNetworkReady()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        void OnClientConnected(ulong clientId)
        {
            var player = GetPlayerData(clientId);
            if (player != null)
            {
                OnPlayerConnected?.Invoke(player);
            }
        }

        /// <summary>
        /// Handles the case where NetworkManager has told us a client has disconnected. This includes ourselves, if we're the host,
        /// and the server is stopped."
        /// </summary>
        private void OnClientDisconnect(ulong clientId)
        {
            if (m_ClientIdToGuid.TryGetValue(clientId, out var guid))
            {
                OnPlayerDisconnected.Invoke(GetPlayerData(clientId));
                m_ClientIdToGuid.Remove(clientId);

                if (m_ClientData[guid].m_ClientID == clientId)
                {
                    //be careful to only remove the ClientData if it is associated with THIS clientId; in a case where a new connection
                    //for the same GUID kicks the old connection, this could get complicated. In a game that fully supported the reconnect flow,
                    //we would NOT remove ClientData here, but instead time it out after a certain period, since the whole point of it is
                    //to remember client information on a per-guid basis after the connection has been lost.
                    m_ClientData.Remove(guid);
                }
            }
        }

        private void Clear()
        {
            //resets all our runtime state.
            m_ClientData.Clear();
            m_ClientIdToGuid.Clear();
        }

        /// <summary>
        /// This logic plugs into the "ConnectionApprovalCallback" exposed by Netcode.NetworkManager, and is run every time a client connects to us.
        /// See MatchplayClient.BootClient for the complementary logic that runs when the client starts its connection.
        /// </summary>
        /// <remarks>
        /// Since our game doesn't have to interact with some third party authentication service to validate the identity of the new connection, our ApprovalCheck
        /// method is simple, and runs synchronously, invoking "callback" to signal approval at the end of the method. Netcode currently doesn't support the ability
        /// to send back more than a "true/false", which means we have to work a little harder to provide a useful error return to the client. To do that, we invoke a
        /// custom message in the same channel that Netcode uses for its connection callback. Since the delivery is NetworkDelivery.ReliableSequenced, we can be
        /// confident that our login result message will execute before any disconnect message.
        /// </remarks>
        /// <param name="connectionData">binary data passed into BootClient. In our case this is the client's GUID, which is a unique identifier for their install of the game that persists across app restarts. </param>
        /// <param name="clientId">This is the clientId that Netcode assigned us on login. It does not persist across multiple logins from the same client. </param>
        /// <param name="connectionApprovedCallback">The delegate we must invoke to signal that the connection was approved or not. </param>
        private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate connectionApprovedCallback)
        {
            if (connectionData.Length > k_MaxConnectPayload)
            {
                connectionApprovedCallback(false, 0, false, null, null);
                return;
            }

            ConnectStatus gameReturnStatus = ConnectStatus.Success;

            string payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html

            Debug.Log("Host ApprovalCheck: connecting client GUID: " + connectionPayload.clientGUID);

            //Test for Duplicate Login.
            if (m_ClientData.ContainsKey(connectionPayload.clientGUID))
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"Client GUID {connectionPayload.clientGUID} already exists. Because this is a debug build, we will still accept the connection");
                    while (m_ClientData.ContainsKey(connectionPayload.clientGUID)) { connectionPayload.clientGUID += "_Secondary"; }
                }
                else
                {
                    ulong oldClientId = m_ClientData[connectionPayload.clientGUID].m_ClientID;

                    // kicking old client to leave only current
                    SendServerToClientSetDisconnectReason(oldClientId, ConnectStatus.LoggedInAgain);
                    WaitToDisconnect(clientId);
                    return;
                }
            }

            SendServerToClientConnectResult(clientId, gameReturnStatus);

            //Populate our dictionaries with the playerData
            m_ClientIdToGuid[clientId] = connectionPayload.clientGUID;
            m_ClientData[connectionPayload.clientGUID] = new PlayerData(connectionPayload.playerName, clientId);

            connectionApprovedCallback(true, null, true, Vector3.zero, Quaternion.identity);

            // connection approval will create a player object for you
            AssignPlayerName(clientId, connectionPayload.playerName);
        }

        private async void WaitToDisconnect(ulong clientId)
        {
            await Task.Delay(500);
            NetworkManager.Singleton.DisconnectClient(clientId);
        }

        /// <summary>
        /// Sends a DisconnectReason to the indicated client. This should only be done on the server, prior to disconnecting the client.
        /// </summary>
        /// <param name="clientID"> id of the client to send to </param>
        /// <param name="status"> The reason for the upcoming disconnect.</param>
        void SendServerToClientSetDisconnectReason(ulong clientID, ConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            MatchplayNetworkMessenger.SendMessage(NetworkMessage.DisconnectionResult, clientID, writer);

            //TODO Messaging system for disconnect messages?
        }

        /// <summary>
        /// Responsible for the Server->Client custom message of the connection result.
        /// </summary>
        /// <param name="clientID"> id of the client to send to </param>
        /// <param name="status"> the status to pass to the client</param>
        void SendServerToClientConnectResult(ulong clientID, ConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);

            MatchplayNetworkMessenger.SendMessage(NetworkMessage.ConnectionResult, clientID, writer);
        }

        void AssignPlayerName(ulong clientId, string playerName)
        {
            // get this client's player NetworkObject
            var networkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);

            networkObject.GetComponent<Matchplayer>().SetName_ServerRpc(playerName);
        }

        public void Dispose()
        {
            if (NetworkManager.Singleton == null)
                return;
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            NetworkManager.Singleton.OnServerStarted -= OnNetworkReady;
        }
    }
}
