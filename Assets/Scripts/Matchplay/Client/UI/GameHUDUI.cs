using System.Collections.Generic;
using Matchplay.Networking;
using Matchplay.Server;
using Matchplay.Shared;
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
        PlayerNameUI m_PlayerLabelUI;

        Dictionary<Matchplayer, PlayerNameUI> m_PlayerLabels = new Dictionary<Matchplayer, PlayerNameUI>();
        Label m_GameModeValue;
        Label m_QueueValue;
        Label m_MapValue;
        VisualElement m_HudElement;
        ClientGameManager m_ClientGameManager;
        SynchedServerData m_SynchedServerData;

        void Start()
        {

            //UIDocument setup
            var root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            m_GameModeValue = root.Q<Label>("modeValue");
            m_QueueValue = root.Q<Label>("queueValue");
            m_MapValue = root.Q<Label>("mapValue");
            m_HudElement = root.Q<VisualElement>("mainMenuVisual");

            //GameManagerCallbacks
            m_ClientGameManager = ClientGameManager.Singleton;
            m_ClientGameManager.networkClient.OnLocalConnection += OnLocalConnection;
            m_ClientGameManager.networkClient.OnLocalDisconnection += OnLocalDisconnection;
            m_ClientGameManager.MatchPlayerSpawned += AddPlayerLabel;
            m_ClientGameManager.MatchPlayerDespawned += RemovePlayerLabel;
            //Synched Variables
            m_SynchedServerData = SynchedServerData.Singleton;
            m_SynchedServerData.OnInitialSynch += OnSynched;
            m_SynchedServerData.map.OnValueChanged += OnMapChanged;
            m_SynchedServerData.gameMode.OnValueChanged += OnModeChanged;
            m_SynchedServerData.gameQueue.OnValueChanged += OnQueueChanged;

        }

        void OnSynched()
        {
            OnMapChanged( Map.None,  m_SynchedServerData.map.Value);
            OnModeChanged( GameMode.None,  m_SynchedServerData.gameMode.Value);
            OnQueueChanged( GameQueue.Missing,  m_SynchedServerData.gameQueue.Value);
        }

        void OnLocalConnection(ConnectStatus status)
        {
            if (status != ConnectStatus.Success)
                return;

            m_HudElement.contentContainer.visible = true;
        }

        void OnLocalDisconnection(ConnectStatus status)
        {
            m_HudElement.contentContainer.visible = false;
        }

        void AddPlayerLabel(Matchplayer player)
        {
            var newLabel = Instantiate(m_PlayerLabelUI, transform);
            m_PlayerLabels[player] = newLabel;
            newLabel.SetLabel(player.PlayerName.Value.ToString(), player.transform);
        }

        void RemovePlayerLabel(Matchplayer player)
        {
            if (m_PlayerLabels.ContainsKey(player))
            {
                Debug.LogWarning($"No player in list : {player}");
                return;
            }

            var playerLabel = m_PlayerLabels[player];
            Destroy(playerLabel);
            m_PlayerLabels.Remove(player);
        }

        void OnMapChanged(Map oldMap, Map newMap)
        {
            Debug.Log($"setting UI MAP: {newMap}");
            m_MapValue.text = newMap.ToString();
        }

        void OnModeChanged(GameMode oldGameMode, GameMode newGameMode)
        {
            Debug.Log($"Setting UI GameMode: {newGameMode}");
            m_GameModeValue.text = newGameMode.ToString();
        }

        void OnQueueChanged(GameQueue oldQueue, GameQueue newQueue)
        {
            Debug.Log($"Setting UI GameQueue: {newQueue}");
            m_QueueValue.text = newQueue.ToString();
        }

        void OnDestroy()
        {
            m_ClientGameManager.networkClient.OnLocalConnection -= OnLocalConnection;
            m_ClientGameManager.networkClient.OnLocalDisconnection -= OnLocalDisconnection;
            m_ClientGameManager.MatchPlayerSpawned -= AddPlayerLabel;
            m_ClientGameManager.MatchPlayerDespawned -= RemovePlayerLabel;
            m_SynchedServerData.OnInitialSynch -= OnSynched;
            m_SynchedServerData.map.OnValueChanged -= OnMapChanged;
            m_SynchedServerData.gameMode.OnValueChanged -= OnModeChanged;
            m_SynchedServerData.gameQueue.OnValueChanged -= OnQueueChanged;

        }
    }
}
