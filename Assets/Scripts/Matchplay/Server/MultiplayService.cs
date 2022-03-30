using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Matchplay.Shared;
using Matchplay.Infrastructure;
using Matchplay.Networking;
using Newtonsoft.Json;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine.Networking;

namespace Matchplay.Server
{
    public class MultiplayService : IDisposable
    {
        UpdateRunner m_UpdateRunner;
        IMultiplayService m_MultiplayService;
        MultiplayEventCallbacks m_Servercallbacks = new MultiplayEventCallbacks();
        IServerCheckManager m_ServerCheckManager;
        IServerEvents m_ServerEvents;
        MultiplayAllocation m_Allocation;
        const string k_PayloadProxyUrl = "http://localhost:8086";

        public async Task<MatchmakerAllocationPayload> BeginServerAndAwaitMatchmakerAllocation()
        {
            m_Allocation = null;
            m_MultiplayService = Unity.Services.Multiplay.MultiplayService.Instance;

            m_Servercallbacks = new MultiplayEventCallbacks();
            m_Servercallbacks.Allocate += OnMultiplayAllocation;
            m_Servercallbacks.Deallocate += OnMultiplayDeAllocation;

            m_ServerEvents = await m_MultiplayService.SubscribeToServerEventsAsync(m_Servercallbacks);
            var mmPayload = await AwaitMatchmakerPayload();
            await m_MultiplayService.ServerReadyForPlayersAsync();
            return mmPayload;
        }

        public async Task BeginServerCheck(GameInfo info)
        {
            m_ServerCheckManager = await m_MultiplayService.ConnectToServerCheckAsync(8, "Matchplay Server", info.gameMode.ToString(), "0", info.map.ToString());
            RegisterNetworkListenrers();
        }

        //The networked server is our source of truth for what is going on, so we update our multiplay check server with values from there.
        void RegisterNetworkListenrers()
        {
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.LocalClientConnected, OnPlayerAdded);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.LocalClientDisconnected, OnPlayerRemoved);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.ServerChangedMap, OnMapChanged);
            MatchplayNetworkMessenger.RegisterListener(NetworkMessage.ServerChangedGameMode, OnModeChanged);
        }

        //Wait for the allocation to be called back before continuing
        async Task<MatchmakerAllocationPayload> AwaitMatchmakerPayload()
        {
            while (m_Allocation == null)
            {
                await Task.Delay(500);
            }

            return await GetMatchmakerAllocationPayloadAsync(m_Allocation.AllocationId);
        }

        async void OnMultiplayAllocation(MultiplayAllocation allocation)
        {
            m_Allocation = allocation;
        }

        void OnMultiplayDeAllocation(MultiplayDeallocation deallocation) { }

        void OnMultiplayError(MultiplayError error) { }

        /// <summary>
        /// This should be in the SDK but we can use web-requests to get access to the MatchmakerAllocationPayload
        /// </summary>
        /// <param name="allocationID"></param>
        /// <returns></returns>
        async Task<MatchmakerAllocationPayload> GetMatchmakerAllocationPayloadAsync(string allocationID)
        {
            var payloadUrl = k_PayloadProxyUrl + $"/payload/{allocationID}";

            using (var webRequest = UnityWebRequest.Get(payloadUrl))
            {
                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(nameof(GetMatchmakerAllocationPayloadAsync) + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(nameof(GetMatchmakerAllocationPayloadAsync) + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(nameof(GetMatchmakerAllocationPayloadAsync) + ":\nReceived: " + webRequest.downloadHandler.text);
                        break;
                }

                return JsonConvert.DeserializeObject<MatchmakerAllocationPayload>(webRequest.downloadHandler.text);
            }
        }

        void OnPlayerAdded(ulong clientID, FastBufferReader reader)
        {
            if (m_ServerCheckManager == null)
                return;
            reader.ReadValueSafe(out ConnectStatus status);
            if (status == ConnectStatus.Success)
            {
                m_ServerCheckManager.CurrentPlayers += 1;
            }
        }

        void OnPlayerRemoved(ulong clientID, FastBufferReader reader)
        {
            if (m_ServerCheckManager == null)
                return;
            reader.ReadValueSafe(out ConnectStatus status);
            if (status == ConnectStatus.GenericDisconnect || status == ConnectStatus.UserRequestedDisconnect)
            {
                m_ServerCheckManager.CurrentPlayers -= 1;
            }
        }

        void OnMapChanged(ulong unused, FastBufferReader reader)
        {
            if (m_ServerCheckManager == null)
                return;
            reader.ReadValueSafe(out Map map);
            m_ServerCheckManager.Map = map.ToString();
        }

        void OnModeChanged(ulong unused, FastBufferReader reader)
        {
            if (m_ServerCheckManager == null)
                return;
            reader.ReadValueSafe(out GameMode mode);

            m_ServerCheckManager.GameType = mode.ToString();
        }

        public void Dispose()
        {
            MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.LocalClientConnected);
            MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.LocalClientDisconnected);
            MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.ServerChangedMap);
            MatchplayNetworkMessenger.UnRegisterListener(NetworkMessage.ServerChangedGameMode);
            m_Servercallbacks.Allocate -= OnMultiplayAllocation;
            m_Servercallbacks.Deallocate -= OnMultiplayDeAllocation;
            m_ServerEvents.UnsubscribeAsync();
        }
    }

    [Serializable]
    public class MatchmakerAllocationPayload
    {
        public MatchProperties MatchProperties;
        public string QueueName;

        // new stuff that needs fixin
        public string Expansion;
        public string GeneratorName;
        public string FunctionName;
    }
}
