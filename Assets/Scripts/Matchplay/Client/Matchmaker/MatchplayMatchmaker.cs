using System;
using System.Collections.Generic;
using System.Linq;
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

    [Serializable]
    public class MatchmakingOption
    {
        public List<string> playerIds;
        public MatchplayGameInfo m_GameInfo;
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
        const string k_MapAttribute = "maps";
        const string k_ModeAttribute = "modes";
        CancellationTokenSource m_CancelToken = new CancellationTokenSource();

        public async Task<MatchmakingResult> Matchmake(MatchmakingOption option)
        {
            m_CancelToken = new CancellationTokenSource();
            var createTicketOptions = MatchmakingToTicketOptions(option);
            try
            {
                m_IsMatchmaking = true;
                var createResult = await Matchmaker.Instance.CreateTicketAsync(createTicketOptions);
                m_LastUsedTicket = createResult.Id;
                try
                {
                    //Polling Loop
                    while (!m_CancelToken.IsCancellationRequested)
                    {
                        Debug.Log($"Polling Ticket: {m_LastUsedTicket}");
                        var checkTicket = await Matchmaker.Instance.GetTicketAsync(m_LastUsedTicket);

                        if (checkTicket.Type == typeof(MultiplayAssignment))
                        {
                            var matchAssignment = (MultiplayAssignment)checkTicket.Value;
                            switch (matchAssignment.Status)
                            {
                                case "Found":
                                    return ReturnMatchResult(MatchResult.Success, "", matchAssignment);
                                case "Timeout":
                                    return ReturnMatchResult(MatchResult.MatchAssignmentError, $"Ticket: {m_LastUsedTicket} Timed out.");
                                case "Failed":
                                    return ReturnMatchResult(MatchResult.MatchAssignmentError, $"Failed: {matchAssignment.Message}");
                                default:
                                    Debug.Log($"Assignment Status: {matchAssignment.Status}");
                                    break;
                            }
                        }

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
            SetProdEnvironment();
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
            await Matchmaker.Instance.DeleteTicketAsync(m_LastUsedTicket);
        }

        //Make sure we exit the matchmaking cycle through this method every time.
        MatchmakingResult ReturnMatchResult(MatchResult resultErrorType, string message = "", MultiplayAssignment assignment = null)
        {
            m_IsMatchmaking = false;
            if (assignment != null)
            {
                var parsedIP = assignment.Ip;
                int parsedPort = -1;
                if (assignment.Port != null)
                    parsedPort = (int)assignment.Port;

                if (parsedPort < 1)
                {
                    return new MatchmakingResult
                    {
                        result = MatchResult.MatchAssignmentError,
                        resultMessage = $"Port could not be cast? - {assignment.Port}"
                    };
                }

                // Enum.TryParse(checkTicket.QueueName, out map selectedMap);
                // gameMode selectedMode = (gameMode)checkTicket.Attributes.GetAs<Dictionary<string, object>>()["selectedGameMode"];

                return new MatchmakingResult
                {
                    result = MatchResult.Success,
                    ip = parsedIP,
                    port = parsedPort

                    //  map = selectedMap,
                    // selectedGameMode = selectedMode
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
        async void SetProdEnvironment()
        {
            await AuthenticationHandler.Authenticating();
            var sdkConfiguration = (IMatchmakerSdkConfiguration)Matchmaker.Instance;
            sdkConfiguration.SetBasePath("https://matchmaker.services.api.unity.com");
        }

        /// <summary>
        /// From Game options to matchmaking options
        /// </summary>
        CreateTicketOptions MatchmakingToTicketOptions(MatchmakingOption mmOption)
        {
            var players = mmOption.playerIds.Select(s => new Player(s)).ToList();

            var qosResults = new List<RuleBasedQoSResult> { new RuleBasedQoSResult("c98f7689-5913-446b-bce5-a3cb9417e906", 0.3, 50) };

            var attributes = new Dictionary<string, object>();

            var customData = new Dictionary<string, object>
            {
                { "gameModePreference", mmOption.m_GameInfo.gameMode },
                { "mapPreference", mmOption.m_GameInfo.map }
            };

            var queueName = QueueModeToName(mmOption.m_GameInfo.gameQueue);

            return new CreateTicketOptions
            {
                QueueName = queueName,
                QosResult = qosResults,
                Players = players,
                Attributes = attributes,
                Data = customData
            };
        }

        /// <summary>
        /// Convert queue enums to ticket queue name
        /// (Same as your queue name in the matchmaker dashboard)
        /// </summary>
        string QueueModeToName(GameQueue queue)
        {
            return queue switch
            {
                GameQueue.Casual => "casual-queue",
                GameQueue.Competetive => "competetive-queue",
                _ => "casual-queue"
            };
        }

        /// <summary>
        ///String to Ip:Port
        /// </summary>
        (string, int) ConnectionParse(string connectionString)
        {
            string ipString = null;
            int portInt = -1;

            if (string.IsNullOrWhiteSpace(connectionString))
                return (ipString, portInt);

            var ipPortSplit = connectionString.Split(':');

            ipString = ipPortSplit[0];

            return (ipString, portInt);
        }

        public void Dispose()
        {
            CancelMatchmaking();
            m_CancelToken?.Dispose();
        }
    }
}
