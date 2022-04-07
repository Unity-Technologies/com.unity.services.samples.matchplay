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

        public MatchplayBackfiller(string connection, string queueName, MatchProperties properties)
        {
            var backfillProperties = new BackfillTicketProperties(properties);

            DoBackfill(connection, queueName, backfillProperties);
        }

        public async void DoBackfill(string connection, string queueName, BackfillTicketProperties properties)
        {
            await UnityServices.InitializeAsync();

            var backfillOptions = new CreateBackfillTicketOptions
            {
                Connection = connection,
                QueueName = queueName,
                Properties = properties
            };

            await MatchmakerService.Instance.CreateBackfillTicketAsync(backfillOptions);
        }
    }
}
