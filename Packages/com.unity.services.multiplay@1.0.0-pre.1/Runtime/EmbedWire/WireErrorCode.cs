using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Wire.Internal
{
    /// <summary>
    /// WireErrorCode lists the error codes to expect from <c>RequestFailedException</c>.
    /// The error code range is: 23000 to 23999.
    /// </summary>
    public enum WireErrorCode
    {
        /// <summary>
        /// Unknown error
        /// </summary>
        Unknown = 23000,

        /// <summary>
        /// The command failed for unknown reason.
        /// </summary>
        CommandFailed = 23002,

        /// <summary>
        /// The connection to the Wire service failed.
        /// </summary>
        ConnectionFailed = 23003,

        /// <summary>
        /// The token provided by the IChannelTokenProvider is invalid.
        /// </summary>
        InvalidToken = 23004,

        /// <summary>
        /// The channel name provided by the IChannelTokenProvider is not valid.
        /// </summary>
        InvalidChannelName = 23005,

        /// <summary>
        /// The GetTokenAsync() method of IChannelTokenProvider threw an exception.
        /// It is provided as inner exception.
        /// </summary>
        TokenRetrieverFailed = 23006,

        /// <summary>
        /// The Wire service refused the subscription.
        /// </summary>
        Unauthorized = 23007,

        /// <summary>
        /// Thrown when trying to subscribe to an IChannel that is already in a subscribed state.
        /// </summary>
        AlreadySubscribed = 23008,

        /// <summary>
        /// Thrown when trying to unsubscribe from an IChannel that is already in an unsubscribed state.
        /// </summary>
        AlreadyUnsubscribed = 23009,
    }
}
