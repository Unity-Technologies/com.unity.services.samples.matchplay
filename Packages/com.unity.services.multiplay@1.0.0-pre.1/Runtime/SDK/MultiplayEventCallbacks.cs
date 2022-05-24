using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    /// <summary>
    /// Class for providing your callbacks, which are used by the Multiplay SDK when a Multiplay Event occurs.
    /// </summary>
    public class MultiplayEventCallbacks
    {
        /// <summary>
        /// Callback which will be invoked when the Server receives an Allocation.
        /// </summary>
        public event Action<MultiplayAllocation> Allocate;

        /// <summary>
        /// Callback which will be invoked when the Server receives a Deallocation.
        /// </summary>
        public event Action<MultiplayDeallocation> Deallocate;

        /// <summary>
        /// Callback which will be invoked when the Server receives an error.
        /// </summary>
        public event Action<MultiplayError> Error;

        /// <summary>
        /// Callback for if the subcription state changes.
        /// </summary>
        public event Action<MultiplayServerSubscriptionState> SubscriptionStateChanged;

        internal void InvokeAllocate(MultiplayAllocation allocation)
        {
            MultiplayServiceLogging.Verbose($"{nameof(InvokeAllocate)}(): {nameof(allocation.EventId)}[{allocation.EventId}] {nameof(allocation.ServerId)}[{allocation.ServerId}] {nameof(allocation.AllocationId)}[{allocation.AllocationId}]");
            Allocate?.Invoke(allocation);
        }

        internal void InvokeDeallocate(MultiplayDeallocation deallocation)
        {
            MultiplayServiceLogging.Verbose($"{nameof(InvokeDeallocate)}(): {nameof(deallocation.EventId)}[{deallocation.EventId}] {nameof(deallocation.ServerId)}[{deallocation.ServerId}] {nameof(deallocation.AllocationId)}[{deallocation.AllocationId}]");
            Deallocate?.Invoke(deallocation);
        }

        internal void InvokeMultiplayError(MultiplayError error)
        {
            MultiplayServiceLogging.Verbose($"{nameof(InvokeMultiplayError)}(): {nameof(error.Reason)}[{error.Reason}] {nameof(error.Detail)}[{error.Detail}]");
            Error?.Invoke(error);
        }

        internal void InvokeSubscriptionStateChanged(MultiplayServerSubscriptionState state)
        {
            MultiplayServiceLogging.Verbose($"{nameof(InvokeSubscriptionStateChanged)}(): {nameof(state)}[{state}]");
            SubscriptionStateChanged?.Invoke(state);
        }
    }
}
