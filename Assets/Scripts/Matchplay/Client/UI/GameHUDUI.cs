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

        void Start()
        {
            var root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            m_GameModeValue = root.Q<Label>("modeValue");
            m_QueueValue = root.Q<Label>("queueValue");
            m_MapValue = root.Q<Label>("mapValue");

            ClientGameManager.Singleton.Client.MapUpdated += UpdatedMap;
            ClientGameManager.Singleton.Client.GameModeUpdated += UpdatedMode;
            ClientGameManager.Singleton.Client.GameQueueUpdated += UpdatedQueue;
            ClientGameManager.Singleton.Client.MatchPlayerSpawned += AddPlayerLabel;
            ClientGameManager.Singleton.Client.MatchPlayerDespawned += RemovePlayerLabel;
            ClientGameManager.Singleton.Client.LocalConnectionFinished += OnLocalConnection;
            ClientGameManager.Singleton.Client.LocalDisconnectHappened += OnLocalDisconnection;
        }

        void OnLocalConnection(ConnectStatus status)
        {
            gameObject.SetActive(true);
        }

        void OnLocalDisconnection(ConnectStatus status)
        {
            gameObject.SetActive(false);
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
                Debug.LogError($"No player in list :  {player}");
                return;
            }

            var playerLabel = m_PlayerLabels[player];
            Destroy(playerLabel);
            m_PlayerLabels.Remove(player);
        }

        void UpdatedMap(Map map)
        {
            m_MapValue.text = map.ToString(); //TODO investigate ways to get the actual flags from the flag map
        }

        void UpdatedMode(GameMode gameMode)
        {
            m_GameModeValue.text = gameMode.ToString();
        }

        void UpdatedQueue(GameQueue mode)
        {
            m_QueueValue.text = mode.ToString();
        }

        void OnDestroy()
        {
            ClientGameManager.Singleton.Client.MapUpdated -= UpdatedMap;
            ClientGameManager.Singleton.Client.GameModeUpdatd -= UpdatedMode;
            ClientGameManager.Singleton.Client.GameQueueUpdated -= UpdatedQueue;
            ClientGameManager.Singleton.Client.MatchPlayerSpawned -= AddPlayerLabel;
            ClientGameManager.Singleton.Client.MatchPlayerDespawned -= RemovePlayerLabel;
        }
    }
}
