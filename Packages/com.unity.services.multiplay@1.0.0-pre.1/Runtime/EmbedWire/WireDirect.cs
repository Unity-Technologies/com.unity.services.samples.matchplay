using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication.Internal;
using Unity.Services.Core.Scheduler.Internal;
using Unity.Services.Core.Telemetry.Internal;
using Unity.Services.Core.Threading.Internal;
using Unity.Services.Wire.Internal;
using UnityEngine;

namespace Unity.Services.Wire.Internal
{
    internal class WireDirect : IWireDirect
    {
        private readonly Dictionary<string, Client> m_ClientsByAddress = new Dictionary<string, Client>();

        private readonly IActionScheduler m_ActionScheduler;
        private readonly IMetrics m_Metrics;
        private readonly IUnityThreadUtils  m_ThreadUtils;
        private readonly IAccessToken m_AccessToken;

        public WireDirect(IActionScheduler actionScheduler, IMetrics metrics, IUnityThreadUtils threadUtils, IAccessToken accessToken)
        {
            m_ActionScheduler = actionScheduler;
            m_Metrics = metrics;
            m_ThreadUtils = threadUtils;
            m_AccessToken = accessToken;
        }

        public IChannel CreateChannel(string address, IChannelTokenProvider provider)
        {
            if (m_ClientsByAddress.TryGetValue(address, out var client))
            {
                return client.CreateChannel(provider);
            }

            var configuration = new Configuration
            {
                token = m_AccessToken,
                address = address,
            };
            var newClient = new Client(configuration, m_ActionScheduler, m_Metrics, m_ThreadUtils);
            newClient.SetupDirectClient();
            m_ClientsByAddress.Add(address, newClient);
            return newClient.CreateChannel(provider);
        }
    }
}
