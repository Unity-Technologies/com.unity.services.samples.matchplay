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
        Task ServerReadyForPlayersAsync();

        /// <summary>
        /// Unreadies this server.
        /// </summary>
        /// <returns>A task that should be awaited.</returns>
        Task ServerUnreadyAsync();

        /// <summary>
        /// Subscribes to the SDK Daemon and provides updates via callbacks.
        /// </summary>
        /// <param name="callbacks"></param>
        /// <returns>A task returning a handle for server event management.</returns>
        Task<IServerEvents> SubscribeToServerEventsAsync(MultiplayEventCallbacks callbacks);

        /// <summary>
        /// Reads configuration from serverconfig file, if present.
        /// Connects to servercheck daemon.
        /// </summary>
        /// <param name="maxPlayers">The max players for this server.</param>
        /// <param name="serverName">The name of this server.</param>
        /// <param name="gameType">The game type of this server.</param>
        /// <param name="buildId">The build ID of this server.</param>
        /// <param name="map">The map of this server.</param>
        /// <returns>A task returning a manager for changing configuration at runtime.</returns>
        Task<IServerCheckManager> ConnectToServerCheckAsync(ushort maxPlayers, string serverName, string gameType, string buildId, string map);
    }
}
