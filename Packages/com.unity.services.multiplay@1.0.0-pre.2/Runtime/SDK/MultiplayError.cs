using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    /// <summary>
    /// An error given by the multiplay service.
    /// </summary>
    public class MultiplayError
    {
        /// <summary>
        /// The reason for the error.
        /// </summary>
        public MultiplayExceptionReason Reason { get; }

        /// <summary>
        /// The detail of the error.
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// Constructs a multiplay error.
        /// </summary>
        /// <param name="reason">The reason for the error.</param>
        /// <param name="detail">The detail of the error.</param>
        public MultiplayError(MultiplayExceptionReason reason, string detail)
        {
            Reason = reason;
            Detail = detail;
        }
    }
}
