using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Matchplay.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;

namespace Matchplay.Client
{
    public class ClientGameManager : MonoBehaviour
    {
        MatchplayGameInfo m_GameOptions = new MatchplayGameInfo
        {
            gameMode = GameMode.Staring,
            map = Map.Lab,
            gameQueue = GameQueue.Casual,
            maxPlayers = 10
        };
        MatchplayClient m_MatchplayClientNetPortal;
        MatchplayMatchmaker m_Matchmaker;

        public static ClientGameManager Singleton
        {
            get
            {
                if (m_ClientGameManager != null) return m_ClientGameManager;
                m_ClientGameManager = FindObjectOfType<ClientGameManager>();
                if (m_ClientGameManager == null)
                {
                    Debug.LogError("No ClientGameManager in scene, did you run this from the bootStrap scene?");
                    return null;
                }

                return m_ClientGameManager;
            }
        }

        static ClientGameManager m_ClientGameManager;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            m_Matchmaker = new MatchplayMatchmaker();
            m_MatchplayClientNetPortal = new MatchplayClient();
        }

        public void BeginConnection(string ip, int port)
        {
            Debug.Log($"Starting Client @ {ip}:{port}");
            m_MatchplayClientNetPortal.StartClient(ip, port);
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

        public void SetGameModes(GameMode gameMode, bool added)
        {
            if (added) //Add Flag if True
                m_GameOptions.gameMode |= gameMode;
            else
            {
                m_GameOptions.gameMode &= ~gameMode;
            }
        }

        public void SetGameMaps(Map map, bool added)
        {
            if (added) //Add Flag if True
                m_GameOptions.map |= map;
            else
            {
                m_GameOptions.map &= ~map;
            }
        }

        public GameQueue ClientGameQueue
        {
            get => m_GameOptions.gameQueue;
            set => m_GameOptions.gameQueue = value;
        }

        public void ToMainMenu()
        {
            SceneManager.LoadScene("mainMenu", LoadSceneMode.Single);
        }

        public void OnDestroy()
        {
            m_MatchplayClientNetPortal.Dispose();
            m_Matchmaker.Dispose();
        }

        async Task<MatchResult> MatchmakeAsync()
        {
            var matchOptions = new MatchmakingOption
            {
                m_GameInfo = m_GameOptions,
                playerIds = new List<string> { AuthenticationService.Instance.PlayerId }
            };
            Debug.Log($"Beginning Matchmaking with {m_GameOptions}");
            var matchmakingResult = await m_Matchmaker.Matchmake(matchOptions);

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
    }
}
