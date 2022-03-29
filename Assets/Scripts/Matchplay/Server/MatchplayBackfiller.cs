using System.Collections.Generic;
using Matchplay.Client;
using Matchplay.Shared;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;

namespace Matchplay.Server
{
    public class MatchplayBackfiller
    {
        BackfillTicketProperties m_ServerProperties;

        public MatchplayBackfiller(string connection, UserData data, MatchProperties properties, List<Player> players)
        {
            var backfillProperties = new BackfillTicketProperties(properties);

            DoBackfill(connection, data, backfillProperties);
        }

        public async void DoBackfill(string connection, UserData data, BackfillTicketProperties properties)
        {
            await UnityServices.InitializeAsync();
            var attributes = new Dictionary<string, object>()
            {
                { MatchplayMatchmaker.k_ModeAttribute, (double)data.gameInfo.gameMode }
            };

            var queueName = data.gameInfo.MultiplayQueue();

            var backfillOptions = new CreateBackfillTicketOptions
            {
                Connection = connection,
                Attributes = attributes,
                QueueName = queueName,
                Properties = properties
            };
            await MatchmakerService.Instance.CreateBackfillTicketAsync(backfillOptions);
        }
    }
}
