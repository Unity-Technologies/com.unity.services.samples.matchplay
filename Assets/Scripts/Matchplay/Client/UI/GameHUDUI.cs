using System;
using System.Collections.Generic;
using Matchplay.Networking;
using Matchplay.Server;
using Matchplay.Shared;
using UnityEngine;
using UnityEngine.Serialization;
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
            m_ClientGameManager = ClientGameManager.Singleton;
            var root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            m_GameModeValue = root.Q<Label>("modeValue");
            m_QueueValue = root.Q<Label>("queueValue");
            m_MapValue = root.Q<Label>("mapValue");
            m_HudElement = root.Q<VisualElement>("mainMenuVisual");
            m_SynchedServerData = SynchedServerData.Singleton;

            m_SynchedServerData.map.OnValueChanged = OnChangedMap;
            m_SynchedServerData.gameMode.OnValueChanged += OnModeChanged;
            m_SynchedServerData.gameQueue.OnValueChanged += OnQueueChanged;
            m_ClientGameManager.networkClient.OnLocalConnection += OnLocalConnection;
            m_ClientGameManager.networkClient.OnLocalDisconnection += OnLocalDisconnection;
            m_ClientGameManager.MatchPlayerSpawned += AddPlayerLabel;
            m_ClientGameManager.MatchPlayerDespawned += RemovePlayerLabel;
        }

        void OnLocalConnection(ConnectStatus status)
        {
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
                Debug.LogError($"No player in list : {player}");
                return;
            }

            var playerLabel = m_PlayerLabels[player];
            Destroy(playerLabel);
            m_PlayerLabels.Remove(player);
        }

        void OnChangedMap(Map oldMap, Map newMap)
        {
            m_MapValue.text = newMap.ToString(); //TODO investigate ways to get the actual flags from the flag map
        }

        void OnModeChanged(GameMode oldGameMode, GameMode newGameMode)
        {
            m_GameModeValue.text = newGameMode.ToString();
        }

        void OnQueueChanged(GameQueue oldQueue, GameQueue newQueue)
        {
            m_QueueValue.text = newQueue.ToString();
        }

        void OnDestroy()
        {
            m_ClientGameManager.networkClient.OnLocalConnection -= OnLocalConnection;
            m_ClientGameManager.networkClient.OnLocalDisconnection -= OnLocalDisconnection;
            m_ClientGameManager.MatchPlayerSpawned -= AddPlayerLabel;
            m_ClientGameManager.MatchPlayerDespawned -= RemovePlayerLabel;
        }
    }
}
