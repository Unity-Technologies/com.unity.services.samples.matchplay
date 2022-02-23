using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Matchplay.Shared;
using Matchplay.Shared.Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;

namespace Matchplay.Client
{
    public class ClientGameManager : IDisposable
    {
        MatchmakingOption m_CasualMatchmakingOptions = new MatchmakingOption
        {
            gameModePreference = GameMode.Staring,
            mapSelection = Map.Lab,
            matchmakingQueue = GameQueue.Casual
        };
        CancellationTokenSource m_CancelMatchmaker = new CancellationTokenSource();
        MatchplayClient m_MatchplayClientNetPortal;
        MatchplayMatchmaker m_Matchmaker;

        [Inject]
        void InitClientScope(MatchplayClient matchplayClient, MatchplayMatchmaker matchmaker)
        {
            m_Matchmaker = matchmaker;
            m_MatchplayClientNetPortal = matchplayClient;
        }

        public void Matchmake()
        {
            MatchmakeAsync(m_CancelMatchmaker.Token);
        }

        public void SetGameModes(GameMode gameMode, bool added)
        {
            if (added) //Add Flag if True
                m_CasualMatchmakingOptions.gameModePreference |= gameMode;
            else
            {
                m_CasualMatchmakingOptions.gameModePreference &= ~gameMode;
            }
        }

        public void SetGameMaps(Map map, bool added)
        {
            if (added) //Add Flag if True
                m_CasualMatchmakingOptions.mapSelection |= map;
            else
            {
                m_CasualMatchmakingOptions.mapSelection &= ~map;
            }
        }

        public GameQueue ClientGameQueue
        {
            get => m_CasualMatchmakingOptions.matchmakingQueue;
            set => m_CasualMatchmakingOptions.matchmakingQueue = value;
        }

        public void ToMainMenu()
        {
            SceneManager.LoadScene("mainMenu", LoadSceneMode.Single);
        }

        /// <summary>
        /// TODO call this to UX
        /// </summary>
        async void MatchmakeAsync(CancellationToken cancellationToken)
        {
            var matchOptions = new MatchmakingOption
            {
                mapSelection = Map.Lab,
                gameModePreference = GameMode.Staring,
                playerIds = new List<string> { AuthenticationService.Instance.PlayerId }
            };

            var matchmakingResult = await m_Matchmaker.Matchmake(matchOptions, cancellationToken);

            if (matchmakingResult.result == MatchResult.Success)
            {
                Debug.Log($"Starting Client @ {matchmakingResult.ip}:{matchmakingResult.port}");
                m_MatchplayClientNetPortal.StartClient(matchmakingResult.ip, matchmakingResult.port);
            }
            else
            {
                Debug.LogWarning($"Matchmaking Failed {matchmakingResult.result} : {matchmakingResult.resultMessage}");
            }
        }

        public void Dispose()
        {
            m_CancelMatchmaker.Cancel();
            m_CancelMatchmaker.Dispose();
        }
    }
}
