using System;
using System.Collections.Generic;
using Matchplay.Networking;
using Matchplay.Server;
using Matchplay.Shared;
using PlasticGui.Help;
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
        PlayerNameUI m_playerLabelUI;

        Dictionary<Matchplayer, PlayerNameUI> m_PlayerLabels = new Dictionary<Matchplayer, PlayerNameUI>();
        Label m_GameModeValue;
        Label m_QueueValue;
        Label m_MapValue;
        VisualElement m_HudElement;
        ClientGameManager m_ClientGameManager;

        void Start()
        {
            m_ClientGameManager = ClientGameManager.Singleton;
            var root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            m_GameModeValue = root.Q<Label>("modeValue");
            m_QueueValue = root.Q<Label>("queueValue");
            m_MapValue = root.Q<Label>("mapValue");
            m_HudElement = root.Q<VisualElement>("mainMenuVisual");

            m_ClientGameManager.observableUser.onMapChanged += OnChangedMap;
            m_ClientGameManager.observableUser.onModeChanged += OnModeChanged;
            m_ClientGameManager.observableUser.onQueueChanged += OnQueueChanged;
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
            var newLabel = Instantiate(m_playerLabelUI, transform);
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

        void OnChangedMap(Map map)
        {
            m_MapValue.text = map.ToString(); //TODO investigate ways to get the actual flags from the flag map
        }

        void OnModeChanged(GameMode gameMode)
        {
            m_GameModeValue.text = gameMode.ToString();
        }

        void OnQueueChanged(GameQueue mode)
        {
            m_QueueValue.text = mode.ToString();
        }

        void OnDestroy()
        {
            m_ClientGameManager.observableUser.onMapChanged -= OnChangedMap;
            m_ClientGameManager.observableUser.onModeChanged -= OnModeChanged;
            m_ClientGameManager.observableUser.onQueueChanged -= OnQueueChanged;
            m_ClientGameManager.networkClient.OnLocalConnection -= OnLocalConnection;
            m_ClientGameManager.networkClient.OnLocalDisconnection -= OnLocalDisconnection;
            m_ClientGameManager.MatchPlayerSpawned -= AddPlayerLabel;
            m_ClientGameManager.MatchPlayerDespawned -= RemovePlayerLabel;
        }
    }
}
