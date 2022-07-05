using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    /// <summary>
    /// Interface representing your subscription to the Multiplay Server Events messages.
    /// </summary>
    public interface IServerEvents
    {
        /// <summary>
        /// The callbacks that the Multiplay Server Events will invoke when messages are received.
        /// </summary>
        MultiplayEventCallbacks Callbacks { get; }

        /// <summary>
        /// Subscribes to the messages. You do not need to call this if you are already subscribed.
        /// </summary>
        /// <returns>An awaitable task.</returns>
        Task SubscribeAsync();

        /// <summary>
        /// Unsubscribes from the messages. You do not need to call this if you are already unsubscribed.
        /// </summary>
        /// <returns>An awaitable task.</returns>
        Task UnsubscribeAsync();
    }
}
