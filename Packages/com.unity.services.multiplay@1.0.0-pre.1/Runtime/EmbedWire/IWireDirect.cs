using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core.Internal;
using UnityEngine;

namespace Unity.Services.Wire.Internal
{
    /// <summary>
    /// The Wire connection for the Multiplay SDK MVP!
    /// </summary>
    public interface IWireDirect : IServiceComponent
    {
        /// <summary>
        /// Creates a channel. This is a hack for Multiplay SDK MVP!
        /// </summary>
        /// <param name="address">The address to connect to.</param>
        /// <param name="tokenProvider">The token provider.</param>
        /// <returns>The channel you connected to.</returns>
        IChannel CreateChannel(string address, IChannelTokenProvider tokenProvider);
    }
}
