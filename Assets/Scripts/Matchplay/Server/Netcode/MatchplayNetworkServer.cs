using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Matchplay.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Matchplay.Shared;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Matchplay.Server
{
    public class MatchplayNetworkServer : IDisposable
    {
        public Action<Matchplayer> OnServerPlayerSpawned;
        public Action<Matchplayer> OnServerPlayerDespawned;

        public Action<UserData> OnPlayerLeft;
        public Action<UserData> OnPlayerJoined;

        SynchedServerData m_SynchedServerData;
        bool m_InitializedServer;
        NetworkManager m_NetworkManager;

        // used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
        const int k_MaxConnectPayload = 1024;

        /// <summary>
        /// map a given client guid to the data for a given client player.
        /// </summary>
        Dictionary<string, UserData> m_ClientData = new Dictionary<string, UserData>();

        /// <summary>
        /// map to allow us to cheaply map from guid to player data.
        /// </summary>
        Dictionary<ulong, string> m_NetworkIdToAuth = new Dictionary<ulong, string>();

        public MatchplayNetworkServer()
        {
            m_NetworkManager = NetworkManager.Singleton;

            // we add ApprovalCheck callback BEFORE OnNetworkSpawn to avoid spurious Netcode for GameObjects (Netcode)
            // warning: "No ConnectionApproval callback defined. Connection approval will timeout"
            m_NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            m_NetworkManager.OnServerStarted += OnNetworkReady;
        }

        public async Task<SynchedServerData> StartServer(string ip, int port, GameInfo startingGameInfo)
        {
            var unityTransport = m_NetworkManager.gameObject.GetComponent<UnityTransport>();
            m_NetworkManager.NetworkConfig.NetworkTransport = unityTransport;
            unityTransport.SetConnectionData(ip, (ushort)port);
            Debug.Log($"Starting server at {ip}:{port}\nWith: {startingGameInfo}");

            m_NetworkManager.StartServer();
            ChangeMap(startingGameInfo.map);
            try
            {
                var getServerDataTries = 3;
                while (getServerDataTries > 0 && m_SynchedServerData == null)
                {
                    m_SynchedServerData = Object.FindObjectOfType<SynchedServerData>();
                    getServerDataTries--;
                    await Task.Delay(50);
                }

                m_SynchedServerData.map.Value = startingGameInfo.map;
                m_SynchedServerData.gameMode.Value = startingGameInfo.gameMode;
                m_SynchedServerData.gameQueue.Value = startingGameInfo.gameQueue;
                Debug.Log($"Synched Server Values: {m_SynchedServerData.map.Value} - {m_SynchedServerData.gameMode.Value} - {m_SynchedServerData.gameQueue.Value}");
                return m_SynchedServerData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting synched values :\n{ex}");
            }

            return null;
        }

        void OnNetworkReady()
        {
            m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        /// <summary>
        /// This logic plugs into the "ConnectionApprovalCallback" exposed by Netcode.NetworkManager, and is run every time a client connects to us.
        /// See MatchplayNetworkClient.BootClient for the complementary logic that runs when the client starts its connection.
        /// </summary>
        /// <param name="connectionData">binary data passed into BootClient. In our case this is the client's GUID, which is a unique identifier for their install of the game that persists across app restarts. </param>
        /// <param name="networkId">This is the networkId that Netcode assigned us on login. It does not persist across multiple logins from the same client. </param>
        /// <param name="connectionApprovedCallback">The delegate we must invoke to signal that the connection was approved or not. </param>
        void ApprovalCheck(byte[] connectionData, ulong networkId, NetworkManager.ConnectionApprovedDelegate connectionApprovedCallback)
        {
            if (connectionData.Length > k_MaxConnectPayload)
            {
                connectionApprovedCallback(false, 0, false, null, null);
                Debug.LogError($"ConnectionData too big! : {connectionData.Length} / {k_MaxConnectPayload}");
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var userData = JsonUtility.FromJson<UserData>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
            userData.networkId = networkId;
            Debug.Log("Host ApprovalCheck: connecting client: " + userData);

            //Test for Duplicate Login.
            if (m_ClientData.ContainsKey(userData.userAuthId))
            {
                ulong oldClientId = m_ClientData[userData.userAuthId].networkId;
                Debug.Log($"Duplicate ID Found : {userData.userAuthId}, Disconnecting Old user");

                // kicking old client to leave only current
                SendClientDisconnected(networkId, ConnectStatus.LoggedInAgain);
                WaitToDisconnect(oldClientId);
            }

            SendClientConnected(networkId, ConnectStatus.Success);

            //Populate our dictionaries with the playerData
            m_NetworkIdToAuth[networkId] = userData.userAuthId;
            m_ClientData[userData.userAuthId] = userData;
            OnPlayerJoined?.Invoke(userData);
            connectionApprovedCallback(true, null, true, Vector3.zero, Quaternion.identity);

            // connection approval will create a player object for you
            SetupPlayerPrefab(networkId, userData.userName);
        }

        /// <summary>
        /// Handles the case where NetworkManager has told us a client has disconnected. This includes ourselves, if we're the host,
        /// and the server is stopped."
        /// </summary>
        private void OnClientDisconnect(ulong networkId)
        {
            SendClientDisconnected(networkId, ConnectStatus.GenericDisconnect);
            if (m_NetworkIdToAuth.TryGetValue(networkId, out var authId))
            {
                m_NetworkIdToAuth?.Remove(networkId);

                if (m_ClientData[authId].networkId == networkId)
                {
                    m_ClientData.Remove(authId);
                }

                OnPlayerLeft?.Invoke(m_ClientData[authId]);
            }

            var matchPlayerInstance = GetNetworkedMatchPlayer(networkId);
            OnServerPlayerDespawned?.Invoke(matchPlayerInstance);

            //matchPlayerInstance.NetworkObject.Despawn(true);
        }

        void SetupPlayerPrefab(ulong networkId, string playerName)
        {
            // get this client's player NetworkObject
            var networkedMatchPlayer = GetNetworkedMatchPlayer(networkId);
            networkedMatchPlayer.ServerSetName(playerName);
            OnServerPlayerSpawned?.Invoke(networkedMatchPlayer);
        }

        Matchplayer GetNetworkedMatchPlayer(ulong networkId)
        {
            var networkObject = m_NetworkManager.SpawnManager.GetPlayerNetworkObject(networkId);
            return networkObject.GetComponent<Matchplayer>();
        }

        async void WaitToDisconnect(ulong networkId)
        {
            await Task.Delay(500);
            m_NetworkManager.DisconnectClient(networkId);
        }

        /// <summary>
        /// Sends a message that a client has connected to the server
        /// </summary>
        /// <param name="networkId"> id of the client to send to </param>
        /// <param name="status"> the status to pass to the client</param>
        void SendClientConnected(ulong networkId, ConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            Debug.Log($"Send Network Client Connected to : {networkId}");
            MatchplayNetworkMessenger.SendMessageTo(NetworkMessage.LocalClientConnected, networkId, writer);
        }

        /// <summary>
        /// Sends a DisconnectReason to the indicated client. This should only be done on the server, prior to disconnecting the client.
        /// </summary>
        /// <param name="networkId"> id of the client to send to </param>
        /// <param name="status"> The reason for the upcoming disconnect.</param>
        void SendClientDisconnected(ulong networkId, ConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            Debug.Log($"Send networkClient Disconnected to : {networkId}");
            MatchplayNetworkMessenger.SendMessageTo(NetworkMessage.LocalClientDisconnected, networkId, writer);
        }

        void ChangeMap(Map newMap)
        {
            var sceneString = ToMap(newMap);
            if (string.IsNullOrEmpty(sceneString))
            {
                Debug.LogError($"Cant Change map, no valid map selection in {newMap}.");
                return;
            }

            m_NetworkManager.SceneManager.LoadScene(sceneString, LoadSceneMode.Single);
        }

        /// <summary>
        /// Convert the map flag enum to a scene name.
        /// </summary>
        string ToMap(Map map)
        {
            switch (map)
            {
                case Map.Lab:
                    return "game_lab";
                case Map.Space:
                    return "game_space";
                default:
                    Debug.LogWarning($"{map} - is not supported.");
                    return "";
            }
        }

        public void Dispose()
        {
            if (m_NetworkManager == null)
                return;
            m_NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
            m_NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            m_NetworkManager.OnServerStarted -= OnNetworkReady;
        }
    }
}