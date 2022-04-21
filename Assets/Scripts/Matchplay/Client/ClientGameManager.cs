using System;
using System.Threading.Tasks;
using Matchplay.Server;
using Matchplay.Shared;
using Matchplay.Shared.Tools;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Matchplay.Client
{
    public class ClientGameManager : IDisposable
    {
        public event Action<Matchplayer> MatchPlayerSpawned;
        public event Action<Matchplayer> MatchPlayerDespawned;

        public MatchplayUser matchplayUser { get; private set; }

        public MatchplayNetworkClient networkClient { get; set; }

        MatchplayMatchmaker m_Matchmaker;

        public ClientGameManager()
        {
            //We can load the mainMenu while the client initializes
#pragma warning disable 4014
            //Disabled warning because we want to fire and forget.
            InitAsync();
#pragma warning restore 4014
        }

        /// <summary>
        /// We do service initialization in parrallel to starting the main menu scene
        /// </summary>
        async Task InitAsync()
        {
            matchplayUser = new MatchplayUser();
            var unityAuthenticationInitOptions = new InitializationOptions();
            var profile = ProfileManager.Profile;
            if (profile.Length > 0)
            {
                unityAuthenticationInitOptions.SetOption("com.unity.services.authentication.profile", profile);
            }

            await UnityServices.InitializeAsync(unityAuthenticationInitOptions);
            AuthenticationWrapper.BeginAuth();

            networkClient = new MatchplayNetworkClient();
            m_Matchmaker = new MatchplayMatchmaker();
            matchplayUser.AuthId = await AuthenticationWrapper.GetClientId();
        }

        public void BeginConnection(string ip, int port)
        {
            Debug.Log($"Starting networkClient @ {ip}:{port}\nWith : {matchplayUser}");
            networkClient.StartClient(ip, port);
        }

        public void Disconnect()
        {
            networkClient.DisconnectClient();
        }

        public async Task MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakerResponse = null)
        {
            if (m_Matchmaker.IsMatchmaking)
            {
                Debug.LogWarning("Already matchmaking, please wait or cancel.");
                return;
            }

            var matchResult = await GetMatchAsync();
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
                matchplayUser.GameModePreferences |= gameMode;
            else
            {
                matchplayUser.GameModePreferences &= ~gameMode;
            }
        }

        public void SetMapPreferencesFlag(Map map, bool added)
        {
            if (added) //Add Flag if True ,remove if not.
                matchplayUser.MapPreferences |= map;
            else
            {
                matchplayUser.MapPreferences &= ~map;
            }
        }

        public void SetGameQueue(GameQueue queue)
        {
            matchplayUser.QueuePreference = queue;
        }

        async Task<MatchmakerPollingResult> GetMatchAsync()
        {
            Debug.Log($"Beginning Matchmaking with {matchplayUser}");
            var matchmakingResult = await m_Matchmaker.Matchmake(matchplayUser.Data);

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
