using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Matchplay.Server
{
    public class MultiplayAllocationService : IDisposable
    {
        IMultiplayService m_MultiplayService;
        MultiplayEventCallbacks m_Servercallbacks;
        IServerCheckManager m_ServerCheckManager;
        IServerEvents m_ServerEvents;
        string m_AllocationId;
        bool m_LocalServerValuesChanged = false;
        CancellationTokenSource m_ServerCheckCancel;

        const string k_PayloadProxyUrl = "http://localhost:8086";

        public MultiplayAllocationService()
        {
            try
            {
                m_MultiplayService = MultiplayService.Instance;
                m_ServerCheckCancel = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error creating Multiplay allocation service.\n{ex}");
            }
        }

        /// <summary>
        /// Should be wrapped in a timeout function
        /// </summary>
        public async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
        {
            if (m_MultiplayService == null)
                return null;
            m_AllocationId = null;
            m_Servercallbacks = new MultiplayEventCallbacks();
            m_Servercallbacks.Allocate += OnMultiplayAllocation;
            m_ServerEvents = await m_MultiplayService.SubscribeToServerEventsAsync(m_Servercallbacks);

            var allocationID = await AwaitAllocationID();
            var mmPayload = await GetMatchmakerAllocationPayloadAsync();

            return mmPayload;
        }

        //The networked server is our source of truth for what is going on, so we update our multiplay check server with values from there.
        public async Task BeginServerCheck()
        {
            if (m_MultiplayService == null)
                return;
            m_ServerCheckManager = await m_MultiplayService.StartServerQueryHandlerAsync((ushort)10,
                "", "", "0", "");

#pragma warning disable 4014
            ServerCheckLoop(m_ServerCheckCancel.Token);
#pragma warning restore 4014
        }

        public void SetServerName(string name)
        {
            m_ServerCheckManager.ServerName = name;
            m_LocalServerValuesChanged = true;
        }

        public void SetBuildID(string id)
        {
            m_ServerCheckManager.BuildId = id;
            m_LocalServerValuesChanged = true;
        }

        public void SetMaxPlayers(ushort players)
        {
            m_ServerCheckManager.MaxPlayers = players;
        }

        public void SetPlayerCount(ushort count)
        {
            m_ServerCheckManager.CurrentPlayers = count;
            m_LocalServerValuesChanged = true;
        }

        public void AddPlayer()
        {
            m_ServerCheckManager.CurrentPlayers += 1;
            m_LocalServerValuesChanged = true;
        }

        public void RemovePlayer()
        {
            m_ServerCheckManager.CurrentPlayers -= 1;
            m_LocalServerValuesChanged = true;
        }

        public void SetMap(string newMap)
        {

            m_ServerCheckManager.Map = newMap;
            m_LocalServerValuesChanged = true;
        }

        public void SetMode(string mode)
        {

            m_ServerCheckManager.GameType = mode;
            m_LocalServerValuesChanged = true;
        }

        public void UpdateServerIfChanged()
        {
            if (m_LocalServerValuesChanged)
            {
                m_ServerCheckManager.UpdateServerCheck();
                m_LocalServerValuesChanged = false;
            }
        }

        async Task ServerCheckLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UpdateServerIfChanged();
                await Task.Delay(1000);
            }
        }

        async Task<string> AwaitAllocationID()
        {
            var config = m_MultiplayService.ServerConfig;
            Debug.Log($"Awaiting Allocation. Server Config is:\n" +
                $"-ServerID: {config.ServerId}\n" +
                $"-AllocationID: {config.AllocatedUuid}\n" +
                $"-Port: {config.Port}\n" +
                $"-QPort: {config.QueryPort}\n" +
                $"-logs: {config.ServerLogDirectory}");

            //Waiting on OnMultiplayAllocation() event (Probably wont ever happen in a matchmaker scenario)
            while (string.IsNullOrEmpty(m_AllocationId))
            {
                var configID = config.AllocatedUuid;

                if (!string.IsNullOrEmpty(configID) && string.IsNullOrEmpty(m_AllocationId))
                {
                    Debug.Log($"Config had AllocationID: {configID}");
                    m_AllocationId = configID;
                }

                await Task.Delay(100);
            }

            return m_AllocationId;
        }

        /// <summary>
        /// Get the Multiplay Allocation Payload for Matchmaker (using Multiplay SDK)
        /// </summary>
        /// <returns></returns>
        async Task<MatchmakingResults> GetMatchmakerAllocationPayloadAsync()
        {
            var payloadAllocation = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
            var modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
            Debug.Log(nameof(GetMatchmakerAllocationPayloadAsync) + ":" + Environment.NewLine + modelAsJson);
            return payloadAllocation;
        }

        void OnMultiplayAllocation(MultiplayAllocation allocation)
        {
            Debug.Log($"OnAllocation: {allocation.AllocationId}");
            if (string.IsNullOrEmpty(allocation.AllocationId))
                return;
            m_AllocationId = allocation.AllocationId;
        }

        void OnMultiplayDeAllocation(MultiplayDeallocation deallocation)
        {
            Debug.Log(
                $"Multiplay Deallocated : ID: {deallocation.AllocationId}\nEvent: {deallocation.EventId}\nServer{deallocation.ServerId}");
        }

        void OnMultiplayError(MultiplayError error)
        {
            Debug.Log($"MultiplayError : {error.Reason}\n{error.Detail}");
        }

        public void Dispose()
        {
            if (m_Servercallbacks != null)
            {
                m_Servercallbacks.Allocate -= OnMultiplayAllocation;
                m_Servercallbacks.Deallocate -= OnMultiplayDeAllocation;
                m_Servercallbacks.Error -= OnMultiplayError;
            }

            if (m_ServerCheckCancel != null)
                m_ServerCheckCancel.Cancel();

            m_ServerEvents?.UnsubscribeAsync();
        }
    }

    public static class AllocationPayloadExtensions
    {
        public static string ToString(this MatchmakingResults payload)
        {
            StringBuilder payloadDescription = new StringBuilder();
            payloadDescription.AppendLine("Matchmaker Allocation Payload:");
            payloadDescription.AppendFormat("-QueueName: {0}\n", payload.QueueName);
            payloadDescription.AppendFormat("-PoolName: {0}\n", payload.PoolName);
            payloadDescription.AppendFormat("-ID: {0}\n", payload.BackfillTicketId);
            payloadDescription.AppendFormat("-Teams: {0}\n", payload.MatchProperties.Teams.Count);
            payloadDescription.AppendFormat("-Players: {0}\n", payload.MatchProperties.Players.Count);
            payloadDescription.AppendFormat("-Region: {0}\n", payload.MatchProperties.Region);
            return payloadDescription.ToString();
        }
    }
}