using System;
using System.Collections.Generic;
using Matchplay.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace Matchplay.Client.UI
{
    enum MainMenuPlayState
    {
        Ready,
        Playing,
        Cancelling
    }

    [RequireComponent(typeof(UIDocument))]
    public class MainMenuUI : MonoBehaviour
    {
        UIDocument m_Document;
        ClientGameManager gameManager;
        AuthState m_AuthState;
        bool m_LocalLaunchMode;
        string m_LocalIP;
        string m_LocalPort;
        string m_LocalName;

        Button m_ExitButton;
        Button m_RenameButton;
        Button m_MatchmakerButton;
        Button m_CancelButton;
        Button m_LocalButton;
        Button m_CompetetiveButton;
        Button m_PlayButton;

        DropdownField m_QueueDropDown;
        VisualElement m_ButtonGroup;
        VisualElement m_GameSettings;
        VisualElement m_IPPortGroup;
        VisualElement m_QueueGroup;
        Label m_NameLabel;

        Toggle m_StaringMode;
        Toggle m_MeditationMode;
        Toggle m_SpaceMap;
        Toggle m_LabMap;

        TextField m_IPField;
        TextField m_PortField;
        TextField m_RenameField;

        async void Start()
        {
            m_Document = GetComponent<UIDocument>();
            var root = m_Document.rootVisualElement;

            #region visual_groups

            m_ButtonGroup = root.Q<VisualElement>("play_button_group");
            m_QueueGroup = root.Q<VisualElement>("queue_group");
            m_GameSettings = root.Q<VisualElement>("game_settings");
            m_IPPortGroup = root.Q<VisualElement>("ip_port_group");

            #endregion

            #region interactables

            m_ExitButton = root.Q<Button>("exit_button");
            m_ExitButton.clicked += ExitApplication;

            m_RenameButton = root.Q<Button>("rename_button");
            m_RenameButton.clicked += ToggleRenameField;

            m_MatchmakerButton = root.Q<Button>("matchmaking_button");
            m_MatchmakerButton.clicked += SetMatchmakerMode;


            m_LocalButton = root.Q<Button>("local_button");
            m_LocalButton.clicked += SetLocalGameMode;

            m_QueueDropDown = root.Q<DropdownField>("queue_drop_down");
            m_QueueDropDown.choices = new List<string>(typeof(GameQueue).GetEnumNames());
            m_QueueDropDown.RegisterValueChangedCallback(QueueDropDownChanged);

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

            m_CancelButton = root.Q<Button>("cancel_button");
            m_CancelButton.clicked += CancelButtonPressed;

            m_IPField = root.Q<TextField>("ip_text_field");
            m_LocalIP = m_IPField.value;
            m_IPField.RegisterValueChangedCallback(IPField);

            m_PortField = root.Q<TextField>("port_text_field");
            m_LocalPort = m_PortField.value;
            m_PortField.RegisterValueChangedCallback(PortField);

            m_RenameField = root.Q<TextField>("rename_field");
            m_RenameField.isDelayed = true;
            m_RenameField.RegisterValueChangedCallback(OnNameFieldChanged);

            m_NameLabel = root.Q<Label>("name_label");

            #endregion

            #region initial_state_setup

            gameManager = ClientSingleton.Instance.Manager;
            SetName(gameManager.User.Name);
            gameManager.User.onNameChanged += SetName;

            //Set the game manager casual gameMode defaults to whatever the UI starts with
            gameManager.SetGameModePreferencesFlag(GameMode.Meditating, m_MeditationMode.value);
            gameManager.SetGameModePreferencesFlag(GameMode.Staring, m_StaringMode.value);
            gameManager.SetMapPreferencesFlag(Map.Space, m_SpaceMap.value);
            gameManager.SetMapPreferencesFlag(Map.Lab, m_LabMap.value);
            gameManager.SetGameQueue(Enum.Parse<GameQueue>(m_QueueDropDown.value));
            SetMatchmakerMode();

            //We can't click play until the auth is set up.
            m_ButtonGroup.SetEnabled(false);
            m_AuthState = await AuthenticationWrapper.Authenticating();
            if (m_AuthState == AuthState.Authenticated)
                SetMenuState(MainMenuPlayState.Ready);
            Debug.Log($"Initializing UI {m_AuthState}");

            #endregion
        }

        void SetName(string newName)
        {
            m_NameLabel.text = newName;
        }

        void OnNameFieldChanged(ChangeEvent<string> evt)
        {
            gameManager.User.Name = evt.newValue;
            m_RenameField.contentContainer.style.display = DisplayStyle.None;
        }

        void ExitApplication()
        {
            gameManager.ExitGame();
        }

        void ToggleRenameField()
        {
            m_RenameField.contentContainer.style.display = m_RenameField.contentContainer.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
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

        void SetMatchmakerMode()
        {
            m_LocalLaunchMode = false;
            if (m_AuthState == AuthState.Authenticated)
                m_ButtonGroup.contentContainer.SetEnabled(true);
            else
                m_ButtonGroup.contentContainer.SetEnabled(false);
            m_PlayButton.text = "Matchmake";
            m_QueueGroup.contentContainer.style.display = DisplayStyle.Flex;
            m_IPPortGroup.contentContainer.style.display = DisplayStyle.None;
            if (gameManager.User.QueuePreference == GameQueue.Competetive)
                m_GameSettings.contentContainer.style.display = DisplayStyle.None;
            else
                m_GameSettings.contentContainer.style.display = DisplayStyle.Flex;
        }

        void SetLocalGameMode()
        {
            m_LocalLaunchMode = true;
            m_ButtonGroup.contentContainer.SetEnabled(true);
            m_QueueGroup.contentContainer.style.display = DisplayStyle.None;
            m_IPPortGroup.contentContainer.style.display = DisplayStyle.Flex;
            m_GameSettings.contentContainer.style.display = DisplayStyle.None;
            m_PlayButton.text = "Play";
        }

        void PlayButtonPressed()
        {
            if (m_LocalLaunchMode)
            {
                if (int.TryParse(m_LocalPort, out var localIntPort))
                    gameManager.BeginConnection(m_LocalIP, localIntPort);
                else
                    Debug.LogError("No valid port in Port Field");
            }
            else
            {
#pragma warning disable 4014
                gameManager.MatchmakeAsync(OnMatchmade);
#pragma warning restore 4014
            }

            SetMenuState(MainMenuPlayState.Playing);
        }

        async void CancelButtonPressed()
        {
            if (m_LocalLaunchMode)
            {
                gameManager.Disconnect();
                SetMenuState(MainMenuPlayState.Ready);
            }
            else
            {
                SetMenuState(MainMenuPlayState.Cancelling);
                await gameManager.CancelMatchmaking();
                SetMenuState(MainMenuPlayState.Ready);
            }
        }

        void OnMatchmade(MatchmakerPollingResult result)
        {
            SetMenuState(MainMenuPlayState.Ready);
        }

        void SetMenuState(MainMenuPlayState state)
        {
            switch (state)
            {
                case MainMenuPlayState.Ready:
                    m_PlayButton.contentContainer.style.display = DisplayStyle.Flex;
                    m_ButtonGroup.contentContainer.SetEnabled(true);
                    m_CancelButton.contentContainer.style.display = DisplayStyle.None;
                    break;
                case MainMenuPlayState.Playing:
                    m_PlayButton.contentContainer.style.display = DisplayStyle.None;
                    m_CancelButton.contentContainer.style.display = DisplayStyle.Flex;
                    break;
                case MainMenuPlayState.Cancelling:
                    m_ButtonGroup.contentContainer.SetEnabled(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        #endregion

        #region gameSelectorCallbacks

        void QueueDropDownChanged(ChangeEvent<string> queueEvent)
        {
            if (!Enum.TryParse(queueEvent.newValue, out GameQueue selectedQueue))
                return;
            gameManager.SetGameQueue(selectedQueue);
            if (selectedQueue == GameQueue.Competetive)
            {
                m_GameSettings.contentContainer.SetEnabled(false);
            }
            else
            {
                m_GameSettings.contentContainer.SetEnabled(true);
            }
        }

        void MeditationChanged(ChangeEvent<bool> meditationEvent)
        {
            gameManager.SetGameModePreferencesFlag(GameMode.Meditating, meditationEvent.newValue);
        }

        void StaringChanged(ChangeEvent<bool> staringEvent)
        {
            gameManager.SetGameModePreferencesFlag(GameMode.Staring, staringEvent.newValue);
        }

        void SpaceSelection(ChangeEvent<bool> spaceEvent)
        {
            gameManager.SetMapPreferencesFlag(Map.Space, spaceEvent.newValue);
        }

        void LabSelection(ChangeEvent<bool> labEvent)
        {
            gameManager.SetMapPreferencesFlag(Map.Lab, labEvent.newValue);
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
