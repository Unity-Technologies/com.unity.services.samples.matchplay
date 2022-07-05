using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    /// <summary>
    /// The multiplay allocation for the server.
    /// </summary>
    public class MultiplayAllocation : IMultiplayAllocationInfo
    {
        /// <inheritdoc />
        public string EventId { get; }

        /// <inheritdoc />
        public long ServerId { get; }

        /// <inheritdoc />
        public string AllocationId { get; }

        /// <summary>
        /// Constructs a multiplay allocation.
        /// </summary>
        /// <param name="eventId">The event ID for the allocation.</param>
        /// <param name="serverId">The server ID for the allocation.</param>
        /// <param name="allocationId">The ID for the allocation.</param>
        public MultiplayAllocation(string eventId, long serverId, string allocationId)
        {
            EventId = eventId;
            ServerId = serverId;
            AllocationId = allocationId;
        }
    }
}
