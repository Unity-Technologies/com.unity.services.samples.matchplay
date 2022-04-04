using System;
using System.Threading.Tasks;
using Matchplay.Server;
using Matchplay.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Matchplay.Client
{
    public class ClientGameManager : MonoBehaviour
    {
        public event Action<Matchplayer> MatchPlayerSpawned;
        public event Action<Matchplayer> MatchPlayerDespawned;

        public ObservableUser observableUser { get; private set; }

        public MatchplayNetworkClient networkClient { get; set; }

        MatchplayMatchmaker m_Matchmaker;

        public static ClientGameManager Singleton
        {
            get
            {
                if (s_ClientGameManager != null) return s_ClientGameManager;
                s_ClientGameManager = FindObjectOfType<ClientGameManager>();
                if (s_ClientGameManager == null)
                {
                    Debug.LogError("No ClientGameManager in scene, did you run this from the bootStrap scene?");
                    return null;
                }

                return s_ClientGameManager;
            }
        }

        static ClientGameManager s_ClientGameManager;

        public async void Init()
        {
            observableUser = new ObservableUser();
            m_Matchmaker = new MatchplayMatchmaker();
            networkClient = new MatchplayNetworkClient();
            observableUser.AuthId = await AuthenticationWrapper.GetClientId();
        }

        public void BeginConnection(string ip, int port)
        {
            Debug.Log($"Starting networkClient @ {ip}:{port}\nWith : {observableUser}");
            networkClient.StartClient(ip, port);
        }

        public void EndConnection()
        {
            networkClient.StopClient();
        }

        public async void Matchmake(Action<MatchResult> onMatchmakerResponse = null)
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
            if (added) //Add Flag if True
                observableUser.GameModePreferences |= gameMode;
            else
            {
                observableUser.GameModePreferences &= ~gameMode;
            }

            Debug.Log($"Set Game GameModePreferences {observableUser.GameModePreferences} - {added}");
        }

        public void SetMapPreferencesFlag(Map map, bool added)
        {
            if (added) //Add Flag if True
                observableUser.MapPreferences |= map;
            else
            {
                observableUser.MapPreferences &= ~map;
            }

            Debug.Log($"Set Game MapPreferences {observableUser.MapPreferences} - {added}");
        }

        public void SetGameQueue(GameQueue queue)
        {
            observableUser.QueuePreference = queue;
        }

        async Task<MatchResult> MatchmakeAsync()
        {
            Debug.Log($"Beginning Matchmaking with {observableUser}");
            var matchmakingResult = await m_Matchmaker.Matchmake(observableUser.Data);

            if (matchmakingResult.result == MatchResult.Success)
            {
                BeginConnection(matchmakingResult.ip, matchmakingResult.port);
            }
            else
            {
                Debug.LogWarning($"Matchmaking Failed {matchmakingResult.result} : {matchmakingResult.resultMessage}");
            }

            return matchmakingResult.result;
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void OnDestroy()
        {
            networkClient.Dispose();
            m_Matchmaker.Dispose();
        }
    }
}
