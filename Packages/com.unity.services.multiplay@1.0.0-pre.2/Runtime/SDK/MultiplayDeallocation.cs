using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    /// <summary>
    /// The multiplay deallocation for the server.
    /// </summary>
    public class MultiplayDeallocation : IMultiplayAllocationInfo
    {
        /// <inheritdoc />
        public string EventId { get; }

        /// <inheritdoc />
        public long ServerId { get; }

        /// <inheritdoc />
        public string AllocationId { get; }

        /// <summary>
        /// Constructs a multiplay deallocation.
        /// </summary>
        /// <param name="eventId">The event ID for the deallocation.</param>
        /// <param name="serverId">The server ID for the deallocation.</param>
        /// <param name="allocationId">The ID for the deallocation.</param>
        public MultiplayDeallocation(string eventId, long serverId, string allocationId)
        {
            EventId = eventId;
            ServerId = serverId;
            AllocationId = allocationId;
        }
    }
}
