using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.Multiplay;
using Unity.Ucg.Usqp;
using UnityEngine;

namespace Unity.Services.Multiplay.Internal
{
    internal class WrappedMultiplayService : IMultiplayService
    {
        private readonly IMultiplayServiceSdk m_MultiplayServiceSdk;

        public ServerConfig ServerConfig { get; private set; }

        public WrappedMultiplayService(IMultiplayServiceSdk serviceSdk)
        {
            m_MultiplayServiceSdk = serviceSdk;

            // Note that if there is no server.json file to be read, ServerConfigReader will throw an exception.
            // This is intended! MultiplayService is not currently usable without a server.json,
            // so a call to MultiplayService.Instance without a server.json will throw!
            ServerConfig = serviceSdk.ServerConfigReader.LoadServerConfig();
        }

        public async Task ServerReadyForPlayersAsync()
        {
            if (string.IsNullOrWhiteSpace(ServerConfig.AllocatedUuid))
            {
                throw new InvalidOperationException("Attempting to be ready for players, but the server has not been allocated yet. You must wait for an allocation.");
            }
            if (!Guid.TryParse(ServerConfig.AllocatedUuid, out var allocationGuid))
            {
                throw new InvalidOperationException($"Unable to parse AllocatedUUID[{ServerConfig.AllocatedUuid}] from {nameof(ServerConfig)}!");
            }
            var request = new GameServer.ServerReadyRequest(ServerConfig.ServerId, allocationGuid);
            await m_MultiplayServiceSdk.GameServerApi.ServerReadyAsync(request);
        }

        public async Task ServerUnreadyAsync()
        {
            var request = new GameServer.ServerUnreadyRequest(ServerConfig.ServerId);
            await m_MultiplayServiceSdk.GameServerApi.ServerUnreadyAsync(request);
        }

        public async Task<IServerEvents> SubscribeToServerEventsAsync(MultiplayEventCallbacks callbacks)
        {
            var serverId = ServerConfig.ServerId;
            var channel = m_MultiplayServiceSdk.WireDirect.CreateChannel($"ws://127.0.0.1:8086/v1/connection/websocket", new MultiplaySdkDaemonTokenProvider(serverId));
            channel.MessageReceived += (message) => OnMessageReceived(callbacks, message);
            await channel.SubscribeAsync();
            return new MultiplayServerEvents(channel, callbacks);
        }

        public async Task<IServerCheckManager> ConnectToServerCheckAsync(ushort maxPlayers, string serverName, string gameType, string buildId, string map)
        {
            return await ConnectToServerCheckAsync(maxPlayers, serverName, gameType, buildId, map, ServerConfig.QueryPort);
        }

        public Task<IServerCheckManager> ConnectToServerCheckAsync(ushort maxPlayers, string serverName, string gameType, string buildId, string map, ushort port)
        {
            var serverCheckManager = new ServerCheckManager(maxPlayers, serverName, gameType, buildId, map);
            serverCheckManager.Connect(port);
            return Task.FromResult((IServerCheckManager)serverCheckManager);
        }

        private void OnMessageReceived(MultiplayEventCallbacks callbacks, string message)
        {
            MultiplayServiceLogging.Verbose($"Received Message[{message}]");
            var jObject = JObject.Parse(message);
            var eventTypeJObject = jObject.SelectToken("EventType");
            if (eventTypeJObject == null)
            {
                // Due to a typo in the early versions of PayloadProxy, "EventType" might not exist. Instead, we check for EventTyp.
                // We can probably remove this at a later date.
                MultiplayServiceLogging.Verbose("EventTypeJObject[EventType] not found. Trying for EventTyp!");
                eventTypeJObject = jObject.SelectToken("EventTyp");
            }
            var eventTypeString = eventTypeJObject.ToObject<string>();
            if (Enum.TryParse<MultiplayEventType>(eventTypeString, out var eventType))
            {
                MultiplayServiceLogging.Verbose($"Handling {nameof(MultiplayEventType)}[{eventType}]");
                switch (eventType)
                {
                    case MultiplayEventType.AllocateEventType: callbacks.InvokeAllocate(CreateMultiplayAllocationFromJson(jObject)); break;
                    case MultiplayEventType.DeallocateEventType: callbacks.InvokeDeallocate(CreateMultiplayDeallocationFromJson(jObject)); break;
                    default: Debug.LogError($"Unhandled {nameof(MultiplayEventType)}[{eventType}]"); break;
                }
            }
            else
            {
                Debug.LogError($"Unrecognised {nameof(MultiplayEventType)}[{eventTypeString}]");
            }
        }

        private MultiplayAllocation CreateMultiplayAllocationFromJson(JObject jObject)
        {
            var eventId = jObject.SelectToken("EventID").ToObject<string>();
            var serverId = jObject.SelectToken("ServerID").ToObject<long>();
            var allocationId = jObject.SelectToken("AllocationID").ToObject<string>();
            MultiplayServiceLogging.Verbose($"Allocation Event: eventId[{eventId}] serverId[{serverId}] allocationId[{allocationId}]");
            ServerConfig = m_MultiplayServiceSdk.ServerConfigReader.LoadServerConfig();
            var allocation = new MultiplayAllocation(eventId, serverId, allocationId);
            return allocation;
        }

        private MultiplayDeallocation CreateMultiplayDeallocationFromJson(JObject jObject)
        {
            var eventId = jObject.SelectToken("EventID").ToObject<string>();
            var serverId = jObject.SelectToken("ServerID").ToObject<long>();
            var allocationId = jObject.SelectToken("AllocationID").ToObject<string>();
            MultiplayServiceLogging.Verbose($"Deallocation Event: eventId[{eventId}] serverId[{serverId}] allocationId[{allocationId}]");
            ServerConfig = m_MultiplayServiceSdk.ServerConfigReader.LoadServerConfig();
            var deallocation = new MultiplayDeallocation(eventId, serverId, allocationId);
            return deallocation;
        }
    }
}
