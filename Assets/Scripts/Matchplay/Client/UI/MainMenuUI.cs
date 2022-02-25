using System;
using System.Collections.Generic;
using Matchplay.Client;
using Matchplay.Shared;
using Matchplay.Infrastructure;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Matchplay.Client.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuUI : MonoBehaviour
    {
        UIDocument m_Document;
        [SerializeField]
        ClientGameManager m_GameManager;
        [SerializeField]
        AuthenticationHandler m_AuthenticationHandler;
        bool m_LocalLaunchMode;
        string m_LocalIP;
        string m_LocalPort;
        Button m_MatchmakerButton;
        Button m_LocalButton;
        Button m_CompetetiveButton;
        Button m_PlayButton;

        DropdownField m_QueueDropDown;
        VisualElement m_GameSettings;
        VisualElement m_IPPortGroup;

        Toggle m_StaringMode;
        Toggle m_MeditationMode;
        Toggle m_SpaceMap;
        Toggle m_LabMap;

        TextField m_IPField;
        TextField m_PortField;

        async void Start()
        {
            m_GameManager = DIScope.RootScope.Resolve<ClientGameManager>();
            m_AuthenticationHandler = DIScope.RootScope.Resolve<AuthenticationHandler>();
            m_Document = GetComponent<UIDocument>();
            var root = m_Document.rootVisualElement;

            m_MatchmakerButton = root.Q<Button>("matchmaking_button");
            m_MatchmakerButton.clicked += MatchmakerPressed;

            m_LocalButton = root.Q<Button>("local_button");
            m_LocalButton.clicked += LocalbuttonPressed;

            m_QueueDropDown = root.Q<DropdownField>("queue_drop_down");
            m_QueueDropDown.choices = new List<string>(typeof(GameQueue).GetEnumNames());
            m_QueueDropDown.RegisterValueChangedCallback(QueueDropDownChanged);

            m_GameSettings = root.Q<VisualElement>("game_settings");
            m_IPPortGroup = root.Q<VisualElement>("ip_port_group");

            m_MeditationMode = root.Q<Toggle>("meditation_toggle");
            m_MeditationMode.RegisterValueChangedCallback(MeditationChanged);
            m_StaringMode = root.Q<Toggle>("staring_toggle");
            m_StaringMode.RegisterValueChangedCallback(StaringChanged);
            m_SpaceMap = root.Q<Toggle>("space_toggle");
            m_SpaceMap.RegisterValueChangedCallback(SpaceSelection);
            m_LabMap = root.Q<Toggle>("lab_toggle");
            m_LabMap.RegisterValueChangedCallback(LabSelection);

            m_PlayButton = root.Q<Button>("play_button");
            m_PlayButton.clicked += PlayButtonPressed;

            m_IPField = root.Q<TextField>("ipTextField");
            m_PortField = root.Q<TextField>("portTextField");
            m_LocalIP = m_IPField.value;
            m_LocalPort = m_PortField.value;
            m_IPField.RegisterValueChangedCallback(IPField);
            m_PortField.RegisterValueChangedCallback(PortField);

            //Set the game manager casual gameMode defaults to whatever the UI starts with
            m_GameManager.SetGameModes(GameMode.Meditating, m_MeditationMode.value);
            m_GameManager.SetGameModes(GameMode.Staring, m_StaringMode.value);
            m_GameManager.SetGameMaps(Map.Space, m_SpaceMap.value);
            m_GameManager.SetGameMaps(Map.Lab, m_LabMap.value);

            //We can't matchmake until the auth is set up.
            m_PlayButton.SetEnabled(false);
            await m_AuthenticationHandler.Authenticating();
            m_PlayButton.SetEnabled(true);
        }

        void OnDestroy()
        {
            m_QueueDropDown.UnregisterValueChangedCallback(QueueDropDownChanged);
            m_MeditationMode.UnregisterValueChangedCallback(MeditationChanged);
            m_StaringMode.UnregisterValueChangedCallback(StaringChanged);
            m_SpaceMap.UnregisterValueChangedCallback(SpaceSelection);
            m_LabMap.UnregisterValueChangedCallback(LabSelection);
        }

        #region buttonPresses

        void MatchmakerPressed()
        {
            m_LocalLaunchMode = false;
            m_QueueDropDown.SetEnabled(true);
            m_IPPortGroup.SetEnabled(false);
            if (m_GameManager.ClientGameQueue == GameQueue.Competetive)
                m_GameSettings.SetEnabled(false);
            else
                m_GameSettings.SetEnabled(true);
        }

        void LocalbuttonPressed()
        {
            m_LocalLaunchMode = true;
            m_QueueDropDown.SetEnabled(false);
            m_IPPortGroup.SetEnabled(true);
            m_GameSettings.SetEnabled(true);
        }

        void PlayButtonPressed()
        {
            if (m_LocalLaunchMode)
            {
                if (int.TryParse(m_LocalPort, out var localIntPort))
                    m_GameManager.BeginConnection(m_LocalIP, localIntPort);
                else
                    Debug.LogError("No valid port in Port Field");
            }
            else
                m_GameManager.Matchmake();
        }

        #endregion

        #region gameSelectorCallbacks

        void QueueDropDownChanged(ChangeEvent<string> evt)
        {
            if (!Enum.TryParse(evt.newValue, out GameQueue selectedQueue))
                return;
            m_GameManager.ClientGameQueue = selectedQueue;
            if (selectedQueue == GameQueue.Competetive)
            {
                m_GameSettings.SetEnabled(false);
            }
            else
            {
                m_GameSettings.SetEnabled(true);
            }
        }

        void MeditationChanged(ChangeEvent<bool> changedTo)
        {
            m_GameManager.SetGameModes(GameMode.Meditating, changedTo.newValue);
        }

        void StaringChanged(ChangeEvent<bool> changedTo)
        {
            m_GameManager.SetGameModes(GameMode.Staring, changedTo.newValue);
        }

        void SpaceSelection(ChangeEvent<bool> changedTo)
        {
            m_GameManager.SetGameMaps(Map.Space, changedTo.newValue);
        }

        void LabSelection(ChangeEvent<bool> changedTo)
        {
            m_GameManager.SetGameMaps(Map.Lab, changedTo.newValue);
        }

        void IPField(ChangeEvent<string> changedTo)
        {
            m_LocalIP = changedTo.newValue;
        }

        void PortField(ChangeEvent<string> changedTo)
        {
            m_LocalPort = changedTo.newValue;
        }

        #endregion
    }
}
