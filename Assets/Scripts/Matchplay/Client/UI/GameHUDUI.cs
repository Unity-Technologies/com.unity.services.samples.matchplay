using System.Collections.Generic;
using Matchplay.Networking;
using Matchplay.Server;
using Matchplay.Shared;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using Label = UnityEngine.UIElements.Label;

namespace Matchplay.Client.UI
{
    /// <summary>
    /// "In-Game" HUD for clients
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GameHUDUI : MonoBehaviour
    {
        [SerializeField]
        PlayerNameUI playerLabelUI;
        [SerializeField]
        SynchedServerData m_SynchedServerData;

        Dictionary<int, PlayerNameUI> m_PlayerLabels = new Dictionary<int, PlayerNameUI>();
        Label m_ServerLabel;
        Label m_GameModeValue;
        Label m_QueueValue;
        Label m_MapValue;
        VisualElement m_ClientUIGroup;
        ClientGameManager m_ClientGameManager;
        Button m_DisconnectButton;

        void Awake()
        {

            //Otherwise look to the bootstrap values
            if (ApplicationData.IsServerUnitTest||ApplicationData.IsServerUnitTest)
            {
                Destroy(gameObject);
                return;
            }

            //UIDocument setup
            var root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            m_ServerLabel = root.Q<Label>("serverValue");
            m_GameModeValue = root.Q<Label>("modeValue");
            m_QueueValue = root.Q<Label>("queueValue");
            m_MapValue = root.Q<Label>("mapValue");
            m_ClientUIGroup = root.Q<VisualElement>("clientUIGroup");
            m_DisconnectButton = root.Q<Button>("button_disconnect");
            m_DisconnectButton.clicked += DisconnectPressed;

            //GameManagerCallbacks
            m_ClientGameManager = ClientSingleton.Instance.Manager;
            m_ClientGameManager.NetworkClient.OnLocalConnection += OnLocalConnection;
            m_ClientGameManager.NetworkClient.OnLocalDisconnection += OnLocalDisconnection;
            m_ClientGameManager.MatchPlayerSpawned += AddPlayerLabel;
            m_ClientGameManager.MatchPlayerDespawned += RemovePlayerLabel;

            //Synched Variables, since the Game HUD is not networked
            m_SynchedServerData.OnNetworkSpawned += OnSynchSpawned;
            m_SynchedServerData.serverID.OnValueChanged += OnServerChanged;
            m_SynchedServerData.map.OnValueChanged += OnMapChanged;
            m_SynchedServerData.gameMode.OnValueChanged += OnModeChanged;
            m_SynchedServerData.gameQueue.OnValueChanged += OnQueueChanged;
            OnSynchSpawned();
        }

        void OnLocalConnection(ConnectStatus status)
        {
            if (status != ConnectStatus.Success)
                return;

            m_ClientUIGroup.contentContainer.visible = true;
        }

        void OnLocalDisconnection(ConnectStatus status)
        {
            m_ClientUIGroup.contentContainer.visible = false;
        }

        void AddPlayerLabel(Matchplayer player)
        {
            var newLabel = Instantiate(playerLabelUI, transform);
            m_PlayerLabels[player.GetInstanceID()] = newLabel;

            newLabel.SetPlayerLabel(player);
        }

        void RemovePlayerLabel(Matchplayer player)
        {
            var instanceId = player.GetInstanceID();
            if (!m_PlayerLabels.ContainsKey(instanceId))
            {
                Debug.LogWarning($"{instanceId} not in label dictionary.");
                return;
            }

            var playerLabel = m_PlayerLabels[instanceId];
            Destroy(playerLabel.gameObject);
            m_PlayerLabels.Remove(instanceId);
        }

        void OnSynchSpawned()
        {
            OnServerChanged("",m_SynchedServerData.serverID.Value.ToString());
            OnMapChanged(Map.None, m_SynchedServerData.map.Value);
            OnModeChanged(GameMode.None, m_SynchedServerData.gameMode.Value);
            OnQueueChanged(GameQueue.None, m_SynchedServerData.gameQueue.Value);
        }

        void OnServerChanged(NetworkString oldServerID, NetworkString newServerID)
        {
            if (oldServerID == newServerID)
                return;
            m_ServerLabel.text = newServerID.ToString();
        }

        void OnMapChanged(Map oldMap, Map newMap)
        {
            if (oldMap == newMap)
                return;
            m_MapValue.text = newMap.ToString();
        }

        void OnModeChanged(GameMode oldGameMode, GameMode newGameMode)
        {
            if (oldGameMode == newGameMode)
                return;
            m_GameModeValue.text = newGameMode.ToString();
        }

        void OnQueueChanged(GameQueue oldQueue, GameQueue newQueue)
        {
            if (oldQueue == newQueue)
                return;
            m_QueueValue.text = newQueue.ToString();
        }

        void DisconnectPressed()
        {
            m_ClientGameManager.Disconnect();
        }

        void OnDestroy()
        {
            if (ApplicationData.IsServerUnitTest||ApplicationData.IsServerUnitTest)
                return;
            m_ClientGameManager.NetworkClient.OnLocalConnection -= OnLocalConnection;
            m_ClientGameManager.NetworkClient.OnLocalDisconnection -= OnLocalDisconnection;
            m_ClientGameManager.MatchPlayerSpawned -= AddPlayerLabel;
            m_ClientGameManager.MatchPlayerDespawned -= RemovePlayerLabel;
            m_SynchedServerData.map.OnValueChanged -= OnMapChanged;
            m_SynchedServerData.gameMode.OnValueChanged -= OnModeChanged;
            m_SynchedServerData.gameQueue.OnValueChanged -= OnQueueChanged;
        }
    }
}
