using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    /// <summary>
    /// Interface for MultiplayAllocation information
    /// MultiplayAllocation and MultiplayDeallocation can both be handled via this interface.
    /// </summary>
    public interface IMultiplayAllocationInfo
    {
        /// <summary>
        /// The event ID for the allocation.
        /// </summary>
        string EventId { get; }

        /// <summary>
        /// The server ID for the allocation.
        /// </summary>
        long ServerId { get; }

        /// <summary>
        /// The ID for the allocation.
        /// </summary>
        string AllocationId { get; }
    }
}
