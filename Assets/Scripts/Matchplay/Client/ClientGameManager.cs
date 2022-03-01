using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Matchplay.Shared;
using Matchplay.Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;

namespace Matchplay.Client
{
    public class ClientGameManager : IDisposable
    {
        MatchplayGameInfo m_GameOptions = new MatchplayGameInfo()
        {
            gameMode = GameMode.Staring,
            map = Map.Lab,
            gameQueue = GameQueue.Casual,
            maxPlayers = 10
        };
        CancellationTokenSource m_CancelMatchmaker;
        MatchplayClient m_MatchplayClientNetPortal;
        MatchplayMatchmaker m_Matchmaker;

        [Inject]
        void InitClientScope(MatchplayClient matchplayClient, MatchplayMatchmaker matchmaker)
        {
            m_Matchmaker = matchmaker;
            m_MatchplayClientNetPortal = matchplayClient;
        }

        public void BeginConnection(string ip, int port)
        {
            Debug.Log($"Starting Client @ {ip}:{port}");
            m_MatchplayClientNetPortal.StartClient(ip, port);
        }

        public void Matchmake()
        {
            if (m_Matchmaker.IsMatchmaking)
            {
                Debug.LogWarning("Already matchmaking, please wait or cancel.");
                return;
            }

            m_CancelMatchmaker = new CancellationTokenSource();
            MatchmakeAsync(m_CancelMatchmaker.Token);
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

        public void Dispose()
        {
            m_CancelMatchmaker.Cancel();
            m_CancelMatchmaker.Dispose();
        }

        async Task MatchmakeAsync(CancellationToken cancellationToken)
        {
            var matchOptions = new MatchmakingOption
            {
                m_GameInfo = m_GameOptions,
                playerIds = new List<string> { AuthenticationService.Instance.PlayerId }
            };
            Debug.Log($"Beginning Matchmaking with {m_GameOptions}");
            var matchmakingResult = await m_Matchmaker.Matchmake(matchOptions, cancellationToken);

            if (matchmakingResult.result == MatchResult.Success)
            {
                BeginConnection(matchmakingResult.ip, matchmakingResult.port);
            }
            else
            {
                Debug.LogWarning($"Matchmaking Failed {matchmakingResult.result} : {matchmakingResult.resultMessage}");
            }
        }
    }
}
