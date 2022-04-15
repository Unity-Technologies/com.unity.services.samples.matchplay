using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Matchplay.Shared;

namespace Matchplay.Client
{
    public enum MatchResult
    {
        Success,
        TicketCreationError,
        TicketCancellationError,
        TicketRetrievalError,
        MatchAssignmentError
    }

    public class MatchmakingResult
    {
        public string ip;
        public int port;
        public Map map;
        public GameMode gameMode;
        public MatchResult result;
        public string resultMessage;
    }

    public class MatchplayMatchmaker : IDisposable
    {
        string m_LastUsedTicket;
        bool m_IsMatchmaking = false;
        const string k_ModeAttribute = "game_mode";
        CancellationTokenSource m_CancelToken = new CancellationTokenSource();

        public async Task<MatchmakingResult> Matchmake(UserData data)
        {
            m_CancelToken = new CancellationTokenSource();
            var createTicketOptions = MatchmakingToTicketOptions(data);
            var players = new List<Player> { new Player(data.userAuthId, data.userGamePreferences) };
            try
            {
                m_IsMatchmaking = true;
                var createResult = await MatchmakerService.Instance.CreateTicketAsync(players, createTicketOptions);
                m_LastUsedTicket = createResult.Id;
                try
                {
                    //Polling Loop
                    while (!m_CancelToken.IsCancellationRequested)
                    {
                        Debug.Log($"Polling Ticket: {m_LastUsedTicket}");
                        var checkTicket = await MatchmakerService.Instance.GetTicketAsync(m_LastUsedTicket);

                        if (checkTicket.Type == typeof(MultiplayAssignment))
                        {
                            var matchAssignment = (MultiplayAssignment)checkTicket.Value;
                            switch (matchAssignment.Status)
                            {
                                case MultiplayAssignment.StatusOptions.Found:
                                    return ReturnMatchResult(MatchResult.Success, "", matchAssignment);
                                case MultiplayAssignment.StatusOptions.Timeout:
                                    return ReturnMatchResult(MatchResult.MatchAssignmentError, $"Ticket: {m_LastUsedTicket} Timed out.");
                                case MultiplayAssignment.StatusOptions.Failed:
                                    return ReturnMatchResult(MatchResult.MatchAssignmentError, $"Failed: {matchAssignment.Message}");
                                default:
                                    Debug.Log($"Assignment Status: {matchAssignment.Status}");
                                    break;
                            }
                        }

                        //GetTicketAsync has a 1 second cooldown, or we get a 429
                        await Task.Delay(1000);
                    }
                }
                catch (MatchmakerServiceException e)
                {
                    return ReturnMatchResult(MatchResult.TicketRetrievalError, e.ToString());
                }
            }
            catch (MatchmakerServiceException e)
            {
                return ReturnMatchResult(MatchResult.TicketCreationError, e.ToString());
            }

            return ReturnMatchResult(MatchResult.TicketCancellationError, "Cancelled Matchmaking");
        }

        public bool IsMatchmaking => m_IsMatchmaking;

        public MatchplayMatchmaker()
        {
            SetStagingEnvironment();
        }

        public async Task CancelMatchmaking()
        {
            if (!m_IsMatchmaking)
                return;
            m_IsMatchmaking = false;
            if (m_CancelToken.Token.CanBeCanceled)
                m_CancelToken.Cancel();

            if (string.IsNullOrEmpty(m_LastUsedTicket))
                return;

            Debug.Log($"Cancelling {m_LastUsedTicket}");
            await MatchmakerService.Instance.DeleteTicketAsync(m_LastUsedTicket);
        }

        //Make sure we exit the matchmaking cycle through this method every time.
        MatchmakingResult ReturnMatchResult(MatchResult resultErrorType, string message = "", MultiplayAssignment assignment = null)
        {
            m_IsMatchmaking = false;
            if (assignment != null)
            {
                var parsedIP = assignment.Ip;
                var parsedPort = assignment.Port;
                if (parsedPort == null)
                    return new MatchmakingResult
                    {
                        result = MatchResult.MatchAssignmentError,
                        resultMessage = $"Port missing? - {assignment.Port}\n-{assignment.Message}"
                    };

                return new MatchmakingResult
                {
                    result = MatchResult.Success,
                    ip = parsedIP,
                    port = (int)parsedPort,
                    resultMessage = assignment.Message
                };
            }

            return new MatchmakingResult
            {
                result = resultErrorType,
                resultMessage = message
            };
        }

        /// <summary>
        /// Testing environment
        /// </summary>
        async void SetStagingEnvironment()
        {
            await AuthenticationWrapper.Authenticating();
            var sdkConfiguration = (IMatchmakerSdkConfiguration)MatchmakerService.Instance;
            sdkConfiguration.SetBasePath("https://matchmaker-stg.services.api.unity.com");
        }

        /// <summary>
        /// From Game player to matchmaking player
        /// </summary>
        CreateTicketOptions MatchmakingToTicketOptions(UserData data)
        {
            var attributes = new Dictionary<string, object>
            {
                { k_ModeAttribute, (double)data.userGamePreferences.gameMode }
            };

            var queueName = data.userGamePreferences.ToMultiplayQueue();

            return new CreateTicketOptions(queueName, attributes);
        }

        public void Dispose()
        {
#pragma warning disable 4014
            CancelMatchmaking();
#pragma warning restore 4014
            m_CancelToken?.Dispose();
        }
    }
}
