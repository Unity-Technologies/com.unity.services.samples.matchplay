using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Matchplay.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Matchplay.Shared;
using UnityEngine.SceneManagement;

namespace Matchplay.Server
{
    public class MatchplayNetworkServer : IDisposable
    {
        public Action<Matchplayer> OnServerPlayerSpawned;
        public Action<Matchplayer> OnServerPlayerDespawned;

        SynchedServerData m_SynchedServerData;
        bool m_InitializedServer;
        NetworkManager m_NetworkManager;

        // used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
        private const int k_MaxConnectPayload = 1024;

        /// <summary>
        /// map a given client guid to the data for a given client player.
        /// </summary>
        private Dictionary<string, UserData> m_ClientData = new Dictionary<string, UserData>();

        /// <summary>
        /// map to allow us to cheaply map from guid to player data.
        /// </summary>
        private Dictionary<ulong, string> m_NetworkIdToAuth = new Dictionary<ulong, string>();

        public MatchplayNetworkServer()
        {
            m_NetworkManager = NetworkManager.Singleton;

            // we add ApprovalCheck callback BEFORE OnNetworkSpawn to avoid spurious Netcode for GameObjects (Netcode)
            // warning: "No ConnectionApproval callback defined. Connection approval will timeout"
            m_NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            m_NetworkManager.OnServerStarted += OnNetworkReady;
            m_SynchedServerData = SynchedServerData.Singleton;
        }

        public void StartServer(string ip, int port, GameInfo startingGameInfo)
        {
            var unityTransport = m_NetworkManager.gameObject.GetComponent<UnityTransport>();
            m_NetworkManager.NetworkConfig.NetworkTransport = unityTransport;
            unityTransport.SetConnectionData(ip, (ushort)port);
            Debug.Log($"Starting server at {ip}:{port}\nWith: {startingGameInfo}");

            m_NetworkManager.StartServer();
            ChangeMap(startingGameInfo.map);
            ChangeGameMode(startingGameInfo.gameMode);
            ChangeQueueMode(startingGameInfo.gameQueue);
        }

        /// <summary>
        /// TEMP: Since we can't receive the info from the server allocation before the client joins, we need to make sure we are in an empty scene to avoid duplicating our bootstrap objects.
        /// </summary>
        public void ToWaitingScene()
        {
            m_NetworkManager.SceneManager.LoadScene("server_waitScene", LoadSceneMode.Single);
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
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var userData = JsonUtility.FromJson<UserData>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html

            Debug.Log("Host ApprovalCheck: connecting client: " + userData);

            //Test for Duplicate Login.
            if (m_ClientData.ContainsKey(userData.userAuthId))
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"networkClient GUID {userData.userAuthId} already exists. Because this is a debug build, we will still accept the connection");
                    while (m_ClientData.ContainsKey(userData.userAuthId)) { userData.userAuthId += "_Secondary"; }
                }
                else
                {
                    ulong oldClientId = m_ClientData[userData.userAuthId].networkId;
                    Debug.Log($"Duplicate ID Found : {userData.userAuthId}, Disconnecting user.");

                    // kicking old client to leave only current
                    SendClientDisconnected(oldClientId, ConnectStatus.LoggedInAgain);
                    WaitToDisconnect(networkId);
                    return;
                }
            }

            SendClientConnected(networkId, ConnectStatus.Success);

            //Populate our dictionaries with the playerData
            m_NetworkIdToAuth[networkId] = userData.userAuthId;
            m_ClientData[userData.userAuthId] = userData;
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
                OnServerPlayerDespawned.Invoke(GetNetworkedMatchPlayer(networkId));

            }
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
            Debug.Log($"Send networkClient Connected to : {networkId}");
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

        void SendServerChangedGameMode(GameMode gameMode)
        {
            m_SynchedServerData.gameMode.Value = gameMode;
        }

        void SendServerChangedMap(Map gameMap)
        {
            m_SynchedServerData.map.Value = gameMap;
        }

        void SendServerChangedQueueMode(GameQueue queueMode)
        {
            m_SynchedServerData.gameQueue.Value = queueMode;
        }

        public void ChangeMap(Map newMap)
        {
            var sceneString = ToMap(newMap);
            if (string.IsNullOrEmpty(sceneString))
            {
                Debug.LogError($"Cant Change map, no valid map selection in {newMap}.");
                return;
            }

            m_NetworkManager.SceneManager.LoadScene(sceneString, LoadSceneMode.Single);
            SendServerChangedMap(newMap);
        }

        public void ChangeGameMode(GameMode mode)
        {
            SendServerChangedGameMode(mode);
        }

        public void ChangeQueueMode(GameQueue queueMode)
        {
            SendServerChangedQueueMode(queueMode);
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

//
//        /// <summary>
//        ///
//        /// </summary>
//        /// <param name="networkId"> guid of the client whose data is requested</param>
//        /// <returns>Player data struct matching the given ID</returns>
//        UserData GetPlayerData(ulong networkId)
//        {
//            //First see if we have a guid matching the clientID given.
//            Debug.Log($"Attempting to get player data for: {networkId}");
//            if (m_NetworkIdToAuth.TryGetValue(networkId, out var clientAuth))
//            {
//                if (m_ClientData.TryGetValue(clientAuth, out var playerData))
//                    return playerData;
//
//                Debug.LogError($"No UserData of matching GUID found: {clientAuth}");
//            }
//            else
//            {
//                Debug.LogError($"No client GUID found mapped to the given client Network ID: {networkId}");
//            }
//
//            return null;
//        }

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
