using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    /// <summary>
    /// Multiplay Server Subscription State
    /// </summary>
    public enum MultiplayServerSubscriptionState
    {
        /// <summary>
        /// The Multiplay Server Subscription is unsubscribed.
        /// </summary>
        Unsubscribed,

        /// <summary>
        /// The Multiplay Server Subscription is synced.
        /// </summary>
        Synced,

        /// <summary>
        /// The Multiplay Server Subscription is unsynced.
        /// </summary>
        Unsynced,

        /// <summary>
        /// The Multiplay Server Subscription has reached an error state.
        /// </summary>
        Error,

        /// <summary>
        /// The Multiplay Server Subscription is subscribing.
        /// </summary>
        Subscribing,
    }
}
