using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Unity.Services.Wire.Internal
{
    interface ISubscriptionRepository
    {
        event Action<int> SubscriptionCountChanged;
        bool IsAlreadySubscribed(Subscription sub);
        bool IsRecovering(Subscription sub);

        void OnSubscriptionComplete(Subscription sub, Reply res);
        Subscription GetSub(Subscription sub);
        Subscription GetSub(string channel);

        IEnumerable<KeyValuePair<string, Subscription>> GetAll();
        void RemoveSub(Subscription sub);

        void OnSocketClosed();
        void RecoverSubscriptions(Reply reply);
        bool IsEmpty { get; }
        bool ServerHasSubscription(Subscription subscription);
        void PromoteSubscriptionHandle(Subscription subscription);
    }

    class ConcurrentDictSubscriptionRepository : ISubscriptionRepository
    {
        public ConcurrentDictionary<string, Subscription> Subscriptions;

        private ConcurrentDictionary<string, SubscriptionHandle> SubscriptionHandles;

        public bool IsEmpty => Subscriptions.IsEmpty;

        public event Action<int> SubscriptionCountChanged;

        public ConcurrentDictSubscriptionRepository()
        {
            Subscriptions = new ConcurrentDictionary<string, Subscription>();
            SubscriptionHandles = new ConcurrentDictionary<string, SubscriptionHandle>();
        }

        public bool IsAlreadySubscribed(string alias)
        {
            return GetSub(alias)?.IsConnected ?? false;
        }

        public bool IsAlreadySubscribed(Subscription sub)
        {
            return IsAlreadySubscribed(sub.Channel);
        }

        public bool IsRecovering(Subscription sub)
        {
            if (String.IsNullOrEmpty(sub.Channel))
            {
                return false;
            }
            return Subscriptions.ContainsKey(sub.Channel) && sub.IsConnected == false;
        }

        public void OnSubscriptionComplete(Subscription sub, Reply res)
        {
            if (sub == null)
            {
                Logger.LogError("Subscription is null!");
                return;
            }

            if (res.HasError())
            {
                Logger.LogError($"An error occured during subscription to {sub.Channel}: {res.error.message}");
                return;
            }

            if (res.result.offset != sub.Offset)
            {
                try
                {
                    sub.OnMessageReceived(res);
                    sub.Offset = res.result.offset;
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }

            var recovering = IsRecovering(sub);
            sub.OnConnectivityChangeReceived(true);

            if (!recovering)
            {
                AddSub(sub);
            }
        }

        public Subscription GetSub(string channel)
        {
            if (String.IsNullOrEmpty(channel))
            {
                return null;
            }

            if (Subscriptions.ContainsKey(channel))
            {
                Subscriptions.TryGetValue(channel, out var sub);
                return sub;
            }

            return null;
        }

        public Subscription GetSub(Subscription sub)
        {
            return GetSub(sub.Channel);
        }

        public void RemoveSub(Subscription sub)
        {
            if (String.IsNullOrEmpty(sub.Channel))
            {
                return;
            }
            if (Subscriptions.ContainsKey(sub.Channel))
            {
                Subscriptions.TryRemove(sub.Channel, out _);
                sub.OnUnsubscriptionComplete();
                SubscriptionCountChanged?.Invoke(Subscriptions.Count);
            }
        }

        public bool ServerHasSubscription(Subscription subscription)
        {
            return SubscriptionHandles.ContainsKey(subscription.Channel);
        }

        public void PromoteSubscriptionHandle(Subscription subscription)
        {
            if (Subscriptions.ContainsKey(subscription.Channel))
            {
                Logger.LogError($"Unable to promote {nameof(SubscriptionHandle)} to {nameof(Subscription)} because a Subscription[{subscription.Channel}] already exists!");
            }
            if (!SubscriptionHandles.TryRemove(subscription.Channel, out var _))
            {
                Logger.LogError($"Unable to promote {nameof(SubscriptionHandle)} to {nameof(Subscription)} because the {nameof(SubscriptionHandle)}[{subscription.Channel}] could not be removed!");
            }
            AddSub(subscription);
        }

        private void AddSub(Subscription sub)
        {
            if (Subscriptions.TryAdd(sub.Channel, sub))
            {
                SubscriptionCountChanged?.Invoke(Subscriptions.Count);
            }
        }

        private void AddSubscriptionHandle(SubscriptionHandle subscriptionHandle)
        {
            SubscriptionHandles.TryAdd(subscriptionHandle.ChannelName, subscriptionHandle);
        }

        public void OnSocketClosed()
        {
            foreach (var iterator in Subscriptions)
            {
                iterator.Value.OnConnectivityChangeReceived(false);
            }
        }

        public void RecoverSubscriptions(Reply reply)
        {
            var res = reply.result.ToConnectionResult();
            if (res.subs?.Count > 0)
            {
                foreach (var replySubscription in res.subs)
                {
                    var sub = GetSub(replySubscription.Key);
                    if (sub == null)
                    {
                        // The subscription exists on the server but not on our client, so we need to create it
                        var newSub = new SubscriptionHandle(replySubscription.Key);
                        AddSubscriptionHandle(newSub);
                    }
                    else
                    {
                        var subreply = new Reply(0, null, new Result()
                        {
                            channel = replySubscription.Key,
                            publications = replySubscription.Value.publications,
                            offset = replySubscription.Value.offset
                        });
                        OnSubscriptionComplete(sub, subreply);
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<string, Subscription>> GetAll()
        {
            return Subscriptions.ToArray();
        }
    }
}
