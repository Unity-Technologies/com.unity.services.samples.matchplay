using System;
using System.Collections.Generic;
using Matchplay.Networking;
using Matchplay.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace Matchplay.Client.UI
{
    enum MainMenuPlayState
    {
        Authenticating,
        Error,
        Ready,
        MatchMaking,
        Cancelling,
        Connecting,
        Connected
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
        DropdownField m_ModeDropDown;
        DropdownField m_MapDropDown;

        VisualElement m_ButtonGroup;
        VisualElement m_IPPortGroup;
        VisualElement m_QueueGroup;
        VisualElement m_MapGroup;
        VisualElement m_ModeGroup;
        Label m_NameLabel;
        Label m_MessageLabel;

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
            m_MapGroup = root.Q<VisualElement>("map_group");
            m_ModeGroup = root.Q<VisualElement>("mode_group");
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

            m_MapDropDown = root.Q<DropdownField>("map_drop_down");
            m_MapDropDown.choices = new List<string>(typeof(Map).GetEnumNames());
            m_MapDropDown.RegisterValueChangedCallback(MapDropDownChanged);

            m_ModeDropDown = root.Q<DropdownField>("mode_drop_down");
            m_ModeDropDown.choices = new List<string>(typeof(GameMode).GetEnumNames());
            m_ModeDropDown.RegisterValueChangedCallback(GameModeDropDownChanged);

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
            m_MessageLabel = root.Q<Label>("message_label");

            #endregion

            #region initial_state_setup

            gameManager = ClientSingleton.Instance.Manager;

            SetName(gameManager.User.Name);
            gameManager.User.onNameChanged += SetName;
            gameManager.NetworkClient.OnLocalConnection += OnConnectionChanged;
            gameManager.NetworkClient.OnLocalDisconnection += OnConnectionChanged;

            //Set the game manager casual gameMode defaults to whatever the UI starts with
            gameManager.SetGameMode(Enum.Parse<GameMode>(m_ModeDropDown.value));
            gameManager.SetGameMap(Enum.Parse<Map>(m_MapDropDown.value));
            gameManager.SetGameQueue(Enum.Parse<GameQueue>(m_QueueDropDown.value));

            //Default mode is Matchmaker
            SetMatchmakerMode();

            m_AuthState = await AuthenticationWrapper.Authenticating();

            if (m_AuthState == AuthState.Authenticated)
                SetMenuState(MainMenuPlayState.Ready, "Authenticated!");
            else
            {
                SetMenuState(MainMenuPlayState.Error, "Error Authenticating: Check the Console for more details.\n" +
                    "(Did you remember to link the editor with the Unity cloud Project?)");
            }

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
            m_RenameField.contentContainer.style.display =
                m_RenameField.contentContainer.style.display == DisplayStyle.Flex
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
        }

        void OnDestroy()
        {
            m_QueueDropDown.UnregisterValueChangedCallback(QueueDropDownChanged);
            m_MapDropDown.UnregisterValueChangedCallback(MapDropDownChanged);
            m_ModeDropDown.UnregisterValueChangedCallback(GameModeDropDownChanged);
            gameManager.User.onNameChanged -= SetName;
            gameManager.NetworkClient.OnLocalConnection -= OnConnectionChanged;
            gameManager.NetworkClient.OnLocalDisconnection -= OnConnectionChanged;
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
            m_ModeGroup.contentContainer.style.display = DisplayStyle.Flex;
            m_MapGroup.contentContainer.style.display = DisplayStyle.Flex;
            m_QueueGroup.contentContainer.style.display = DisplayStyle.Flex;
            m_IPPortGroup.contentContainer.style.display = DisplayStyle.None;
        }

        void SetLocalGameMode()
        {
            m_LocalLaunchMode = true;
            m_ButtonGroup.contentContainer.SetEnabled(true);
            m_ModeGroup.contentContainer.style.display = DisplayStyle.None;
            m_MapGroup.contentContainer.style.display = DisplayStyle.None;
            m_QueueGroup.contentContainer.style.display = DisplayStyle.None;
            m_IPPortGroup.contentContainer.style.display = DisplayStyle.Flex;
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

            SetMenuState(MainMenuPlayState.MatchMaking);
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
            switch (result)
            {
                case MatchmakerPollingResult.Success:
                    SetMenuState(MainMenuPlayState.Connecting);
                    break;
                case MatchmakerPollingResult.TicketCreationError:
                    SetMenuState(MainMenuPlayState.Error,
                        "Matchmaking Error while Creating a ticket.\n Check Console for more details.");
                    break;
                case MatchmakerPollingResult.TicketCancellationError:
                    SetMenuState(MainMenuPlayState.Error,
                        "Matchmaking Error while Cancelling a ticket.\n Check Console for more details.");
                    break;
                case MatchmakerPollingResult.TicketRetrievalError:
                    SetMenuState(MainMenuPlayState.Error,
                        "Matchmaking Error while Retrieving a ticket.\n Check Console for more details.");
                    break;
                case MatchmakerPollingResult.MatchAssignmentError:
                    SetMenuState(MainMenuPlayState.Error,
                        "Matchmaking Error while Assigning a ticket.\n Check Console for more details.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        void OnConnectionChanged(ConnectStatus status)
        {
            if (status == ConnectStatus.Success)
                SetMenuState(MainMenuPlayState.Connected);
            else if (status == ConnectStatus.UserRequestedDisconnect)
                SetMenuState(MainMenuPlayState.Ready, $"Succsefully Disconnected!");
            else
                SetMenuState(MainMenuPlayState.Error, $"Connection Error: {status}");
        }

        void SetLabelMessage(string message, Color messageColor)
        {
            m_MessageLabel.text = message;
            m_MessageLabel.style.color = messageColor;
        }

        void SetMenuState(MainMenuPlayState state, string message = "")
        {
            switch (state)
            {
                case MainMenuPlayState.Authenticating:
                    //We can't click play until the auth is set up.
                    m_ButtonGroup.SetEnabled(false);
                    SetLabelMessage("Authenticating...", Color.white);
                    break;
                case MainMenuPlayState.Error:
                    SetLabelMessage(message, new Color(1, .2f, .2f, 1));
                    m_PlayButton.contentContainer.style.display = DisplayStyle.Flex;
                    m_ButtonGroup.contentContainer.SetEnabled(true);
                    m_CancelButton.contentContainer.style.display = DisplayStyle.None;
                    break;
                case MainMenuPlayState.Ready:
                    m_PlayButton.contentContainer.style.display = DisplayStyle.Flex;
                    m_ButtonGroup.contentContainer.SetEnabled(true);
                    m_CancelButton.contentContainer.style.display = DisplayStyle.None;
                    SetLabelMessage(message, new Color(.2f, 1, .2f, 1));
                    break;
                case MainMenuPlayState.MatchMaking:
                    m_PlayButton.contentContainer.style.display = DisplayStyle.None;
                    m_CancelButton.contentContainer.style.display = DisplayStyle.Flex;
                    SetLabelMessage("Matchmaking...", Color.white);
                    break;
                case MainMenuPlayState.Connecting:
                    m_PlayButton.contentContainer.style.display = DisplayStyle.None;
                    m_CancelButton.contentContainer.style.display = DisplayStyle.Flex;
                    SetLabelMessage("Connecting...", Color.white);
                    break;
                case MainMenuPlayState.Connected:
                    m_PlayButton.contentContainer.style.display = DisplayStyle.None;
                    m_CancelButton.contentContainer.style.display = DisplayStyle.None;
                    SetLabelMessage("Connected!", Color.white);
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
        }

        void GameModeDropDownChanged(ChangeEvent<string> modeEvent)
        {
            if (!Enum.TryParse(modeEvent.newValue, out GameMode selectedMode))
                return;
            gameManager.SetGameMode(selectedMode);
        }

        void MapDropDownChanged(ChangeEvent<string> mapEvent)
        {
            if (!Enum.TryParse(mapEvent.newValue, out Map selectedMap))
                return;
            gameManager.SetGameMap((Map)selectedMap);
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