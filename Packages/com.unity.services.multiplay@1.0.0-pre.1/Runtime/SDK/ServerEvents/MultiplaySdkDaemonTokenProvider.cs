using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Wire.Internal;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    internal class MultiplaySdkDaemonTokenProvider : IChannelTokenProvider
    {
        private readonly long serverId;

        public Task<ChannelToken> GetTokenAsync()
        {
            var token = new ChannelToken
            {
                ChannelName = $"server#{serverId}",
                Token = "TestToken"
            };
            return Task.FromResult(token);
        }

        public MultiplaySdkDaemonTokenProvider(long serverId)
        {
            this.serverId = serverId;
        }
    }
}
