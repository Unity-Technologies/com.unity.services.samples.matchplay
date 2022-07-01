using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    /// <summary>
    /// Interface of the Multiplay SDK for using the Multiplay Service.
    /// </summary>
    public interface IMultiplayService
    {
        /// <summary>
        /// Gets the server config for the current session.
        /// </summary>
        ServerConfig ServerConfig { get; }

        /// <summary>
        /// Readies this server.
        /// </summary>
        /// <returns>A task that should be awaited.</returns>
        Task ReadyServerForPlayersAsync();

        /// <summary>
        /// Unreadies this server.
        /// </summary>
        /// <returns>A task that should be awaited.</returns>
        Task UnreadyServerAsync();

        /// <summary>
        /// Gets the payload allocation as plain text.
        /// </summary>
        /// <returns>The payload allocation as plain text.</returns>
        Task<string> GetPayloadAllocationAsPlainText();

        /// <summary>
        /// Gets the payload allocation, in JSON, and deserializes it as the given object.
        /// </summary>
        /// <typeparam name="TPayload">The object to be deserialized as.</typeparam>
        /// <param name="throwOnMissingMembers">Throws an exception if the given class is missing a member.</param>
        /// <returns>An object representing the payload allocation.</returns>
        Task<TPayload> GetPayloadAllocationFromJsonAs<TPayload>(bool throwOnMissingMembers = false);

        /// <summary>
        /// Subscribes to the SDK Daemon and provides updates via callbacks.
        /// </summary>
        /// <param name="callbacks"></param>
        /// <returns>A task returning a handle for server event management.</returns>
        Task<IServerEvents> SubscribeToServerEventsAsync(MultiplayEventCallbacks callbacks);

        /// <summary>
        /// Starts the server query handler.
        /// The handler provides the Multiplay Service with information about this server.
        /// </summary>
        /// <param name="maxPlayers">The max players for this server.</param>
        /// <param name="serverName">The name of this server.</param>
        /// <param name="gameType">The game type of this server.</param>
        /// <param name="buildId">The build ID of this server.</param>
        /// <param name="map">The map of this server.</param>
        /// <returns>A task returning a manager for changing the current response.</returns>
        Task<IServerCheckManager> StartServerQueryHandlerAsync(ushort maxPlayers, string serverName, string gameType, string buildId, string map);
    }
}
