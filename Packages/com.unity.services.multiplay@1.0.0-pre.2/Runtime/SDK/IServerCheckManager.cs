using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    /// <summary>
    /// The interface for interacting with the servercheck system for this server.
    /// </summary>
    public interface IServerCheckManager : IDisposable
    {
        /// <summary>
        /// The maximum number of players on the server.
        /// </summary>
        ushort MaxPlayers { get; set; }

        /// <summary>
        /// The name for the server.
        /// </summary>
        string ServerName { get; set; }

        /// <summary>
        /// The name or identifier of the game type the server is running.
        /// </summary>
        string GameType { get; set; }

        /// <summary>
        /// The version of the game.
        /// </summary>
        string BuildId { get; set; }

        /// <summary>
        /// The map or world the server is running for the game.
        /// </summary>
        string Map { get; set; }

        /// <summary>
        /// The game port that game clients connect to.
        /// </summary>
        ushort Port { get; set; }

        /// <summary>
        /// The number of players currently on the server.
        /// </summary>
        ushort CurrentPlayers { get; set; }

        /// <summary>
        /// Updates the servercheck values for the server.
        /// This is expected to be called in an update loop.
        /// </summary>
        void UpdateServerCheck();
    }
}
