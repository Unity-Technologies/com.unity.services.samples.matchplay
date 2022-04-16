using System;
using System.Threading.Tasks;
using Matchplay.Server;
using Matchplay.Shared;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Matchplay.Client
{
    public class ClientGameManager : IDisposable
    {
        public event Action<Matchplayer> MatchPlayerSpawned;
        public event Action<Matchplayer> MatchPlayerDespawned;

        public ObservableUser observableUser { get; private set; }

        public MatchplayNetworkClient networkClient { get; set; }

        MatchplayMatchmaker m_Matchmaker;

        public ClientGameManager()
        {
            //Starts an async task reliably.
#pragma warning disable 4014
            Init();
#pragma warning restore 4014
        }

        /// <summary>
        /// We do service initialization in parrallel to starting the main menu scene
        /// </summary>
        async Task Init()
        {
            observableUser = new ObservableUser();

            await UnityServices.InitializeAsync();
            AuthenticationWrapper.BeginAuth();

            networkClient = new MatchplayNetworkClient();
            m_Matchmaker = new MatchplayMatchmaker();
            observableUser.AuthId = await AuthenticationWrapper.GetClientId();
        }

        public void BeginConnection(string ip, int port)
        {
            Debug.Log($"Starting networkClient @ {ip}:{port}\nWith : {observableUser}");
            networkClient.StartClient(ip, port);
        }

        public void Disconnect()
        {
            networkClient.DisconnectClient();
        }

        public async void Matchmake(Action<MatchmakerPollingResult> onMatchmakerResponse = null)
        {
            if (m_Matchmaker.IsMatchmaking)
            {
                Debug.LogWarning("Already matchmaking, please wait or cancel.");
                return;
            }

            var matchResult = await MatchmakeAsync();
            onMatchmakerResponse?.Invoke(matchResult);
        }

        public async Task CancelMatchmaking()
        {
            await m_Matchmaker.CancelMatchmaking();
        }

        public void ToMainMenu()
        {
            SceneManager.LoadScene("mainMenu", LoadSceneMode.Single);
        }

        public void AddMatchPlayer(Matchplayer player)
        {
            MatchPlayerSpawned?.Invoke(player);
        }

        public void RemoveMatchPlayer(Matchplayer player)
        {
            MatchPlayerDespawned?.Invoke(player);
        }

        public void SetGameModePreferencesFlag(GameMode gameMode, bool added)
        {
            if (added) //Add Flag if True, remove if not.
                observableUser.GameModePreferences |= gameMode;
            else
            {
                observableUser.GameModePreferences &= ~gameMode;
            }
        }

        public void SetMapPreferencesFlag(Map map, bool added)
        {
            if (added) //Add Flag if True ,remove if not.
                observableUser.MapPreferences |= map;
            else
            {
                observableUser.MapPreferences &= ~map;
            }
        }

        public void SetGameQueue(GameQueue queue)
        {
            observableUser.QueuePreference = queue;
        }

        async Task<MatchmakerPollingResult> MatchmakeAsync()
        {
            Debug.Log($"Beginning Matchmaking with {observableUser}");
            var matchmakingResult = await m_Matchmaker.Matchmake(observableUser.Data);

            if (matchmakingResult.result == MatchmakerPollingResult.Success)
            {
                BeginConnection(matchmakingResult.ip, matchmakingResult.port);
            }
            else
            {
                Debug.LogWarning($"{matchmakingResult.result} : {matchmakingResult.resultMessage}");
            }

            return matchmakingResult.result;
        }

        public void Dispose()
        {
            networkClient?.Dispose();
            m_Matchmaker?.Dispose();
        }
    }
}
