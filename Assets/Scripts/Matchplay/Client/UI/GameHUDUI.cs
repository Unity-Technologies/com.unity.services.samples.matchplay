using System;
using System.Collections.Generic;
using Matchplay.Networking;
using Matchplay.Shared;
using UnityEngine;
using UnityEngine.UIElements;
using Label = UnityEngine.UIElements.Label;

namespace Matchplay.Client.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class GameHUDUI : MonoBehaviour
    {
        Dictionary<string, Label> m_PlayerLabels = new Dictionary<string, Label>();
        VisualElement m_PlayerListGroup;
        Label m_GameModeValue;
        Label m_QueueValue;
        Label m_MapValue;

        void OnGameInfoUpdated(MatchplayGameInfo oldValue, MatchplayGameInfo newValue)
        {
            m_GameModeValue.text = newValue.CurrentGameMode.ToString();
            m_QueueValue.text = newValue.CurrentGameQueue.ToString();
            m_MapValue.text = newValue.CurrentMap.ToString();
        }

        void AddPlayerLabel(PlayerData? playerId)
        {
            if (!playerId.HasValue)
                return;
            var clientId = playerId.Value.m_ClientID.ToString();
            if (!m_PlayerLabels.ContainsKey(clientId))
                return;
            var newLabel = new Label(playerId.Value.m_PlayerName);
            m_PlayerListGroup.Add(newLabel);
            m_PlayerLabels[clientId] = newLabel;
        }

        void RemovePlayerLabel(PlayerData? playerId)
        {
            if (!playerId.HasValue)
                return;
            var clientId = playerId.Value.m_ClientID.ToString();
            if (m_PlayerLabels.ContainsKey(clientId))
            {
                Debug.LogError($"No player by id: {playerId}");
                return;
            }

            var playerLabel = m_PlayerLabels[clientId];
            m_PlayerLabels.Remove(clientId);
            m_PlayerListGroup.Remove(playerLabel);
        }

        void Start()
        {
            var root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            m_PlayerListGroup = root.Q<VisualElement>();
            m_GameModeValue = root.Q<Label>("modeValue");
            m_QueueValue = root.Q<Label>("queueValue");
            m_MapValue = root.Q<Label>("mapValue");
            /*MatchplayServer.Singleton.OnPlayerConnected.AddListener(AddPlayerLabel);
            MatchplayServer.Singleton.OnPlayerDisconnected.AddListener(RemovePlayerLabel);
            MatchplayServer.Singleton.MatchInfo.OnValueChanged += OnGameInfoUpdated;*/
        }

        void OnDestroy()
        {
           /* MatchplayServer.Singleton.OnPlayerConnected.RemoveListener(AddPlayerLabel);
            MatchplayServer.Singleton.OnPlayerDisconnected.RemoveListener(RemovePlayerLabel);
            MatchplayServer.Singleton.MatchInfo.OnValueChanged -= OnGameInfoUpdated;*/
        }
    }
}
