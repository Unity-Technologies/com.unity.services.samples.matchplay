using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Wire.Internal;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    internal class MultiplayServerEvents : IServerEvents
    {
        public MultiplayEventCallbacks Callbacks { get; }

        private IChannel m_Channel { get; }

        public MultiplayServerEvents(IChannel channel, MultiplayEventCallbacks callbacks)
        {
            Callbacks = callbacks;
            m_Channel = channel;
            m_Channel.MessageReceived += OnMessageReceived;
            m_Channel.KickReceived += OnKickReceived;
            m_Channel.NewStateReceived += OnNewStateReceived;
            m_Channel.ErrorReceived += OnErrorReceived;
        }

        public async Task SubscribeAsync()
        {
            await m_Channel.SubscribeAsync();
        }

        public async Task UnsubscribeAsync()
        {
            await m_Channel.UnsubscribeAsync();
        }

        private void OnMessageReceived(string message)
        {
        }

        private void OnKickReceived()
        {
            Callbacks.InvokeSubscriptionStateChanged(MultiplayServerSubscriptionState.Unsubscribed);
        }

        private void OnNewStateReceived(SubscriptionState state)
        {
            switch (state)
            {
                case SubscriptionState.Unsubscribed: Callbacks.InvokeSubscriptionStateChanged(MultiplayServerSubscriptionState.Unsubscribed); break;
                case SubscriptionState.Synced: Callbacks.InvokeSubscriptionStateChanged(MultiplayServerSubscriptionState.Synced); break;
                case SubscriptionState.Unsynced: Callbacks.InvokeSubscriptionStateChanged(MultiplayServerSubscriptionState.Unsynced); break;
                case SubscriptionState.Error: Callbacks.InvokeSubscriptionStateChanged(MultiplayServerSubscriptionState.Error); break;
                case SubscriptionState.Subscribing: Callbacks.InvokeSubscriptionStateChanged(MultiplayServerSubscriptionState.Subscribing); break;
            }
        }

        private void OnErrorReceived(string error)
        {
            Callbacks.InvokeMultiplayError(new MultiplayError(MultiplayExceptionReason.Unknown, error));
        }
    }
}
