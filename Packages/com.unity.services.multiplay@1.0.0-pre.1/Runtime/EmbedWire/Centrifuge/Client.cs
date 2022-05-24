using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core.Telemetry.Internal;
using Unity.Services.Core.Threading.Internal;
using UnityEditor;

namespace Unity.Services.Wire.Internal
{
    class Client : IWire
    {
        enum ConnectionState
        {
            Disconnected,
            Connected,
            Connecting,
            Disconnecting
        }

        public readonly ISubscriptionRepository SubscriptionRepository;

        TaskCompletionSource<ConnectionState> m_ConnectionCompletionSource;
        TaskCompletionSource<ConnectionState> m_DisconnectionCompletionSource;
        ConnectionState m_ConnectionState = ConnectionState.Disconnected;
        CancellationTokenSource m_PingCancellationSource;
        Task<bool> m_PingTask;
        IWebSocket m_WebsocketClient;

        readonly IBackoffStrategy m_Backoff;
        readonly CommandManager m_CommandManager;
        readonly Configuration m_Config;
        readonly IMetrics m_Metrics;
        readonly IUnityThreadUtils m_ThreadUtils;

        private bool m_WebsocketInitialized = false;

        private bool m_DirectClient = false;
        private string m_DirectSubscriptionChannel = null;

        public Client(Configuration config, Core.Scheduler.Internal.IActionScheduler actionScheduler, IMetrics metrics, IUnityThreadUtils threadUtils)
        {
            m_ThreadUtils = threadUtils;
            m_Config = config;
            m_Metrics = metrics;
            SubscriptionRepository = new ConcurrentDictSubscriptionRepository();
            SubscriptionRepository.SubscriptionCountChanged += (int subscriptionCount) =>
            {
                if (m_Metrics != null)
                {
                    m_Metrics.SendGaugeMetric("subscription_count", subscriptionCount);
                }
                Logger.LogVerbose($"Subscription count changed: {subscriptionCount}");
                if (subscriptionCount == 0)
                {
                    Disconnect();
                }
            };
            m_Backoff = new ExponentialBackoffStrategy();
            m_CommandManager = new CommandManager(config, actionScheduler);
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
#endif
        }

        public void SetupDirectClient()
        {
            m_DirectClient = true;
        }

#if UNITY_EDITOR
        void PlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
            {
                return;
            }

            foreach (var sub in SubscriptionRepository.GetAll())
            {
                sub.Value.Dispose();
            }
        }
#endif

        async Task<Reply> SendCommandAsync(UInt32 id, Message command)
        {
            var time = DateTime.Now;
            var tags = new Dictionary<string, string> {{"method", command.GetMethod()}};
            m_CommandManager.RegisterCommand(id);

            m_WebsocketClient.Send(command.GetBytes());

            Logger.LogVerbose($"sending {command.GetMethod()} command: {command.Serialize()}");
            try
            {
                var reply = await m_CommandManager.WaitForCommandAsync(id);
                tags.Add("result", "success");
                if (m_Metrics != null)
                {
                    m_Metrics.SendHistogramMetric("command", (DateTime.Now - time).TotalMilliseconds, tags);
                }
                return reply;
            }
            catch (Exception)
            {
                tags.Add("result", "failure");
                if (m_Metrics != null)
                {
                    m_Metrics.SendHistogramMetric("command", (DateTime.Now - time).TotalMilliseconds, tags);
                }
                throw;
            }
        }

        /// <summary>
        /// Ping is a routine responsible for sending a Ping command to centrifuge at a regular interval.
        /// The main objective is to detect connectivity issues with the server.
        /// It could also be used to measure the command round trip latency.
        /// </summary>
        /// <typeparam name="TPayload"> The TPayload class representation of the payloads sent to your channel</typeparam>
        /// <returns> Return true if the routine exits because the system noticed the ws connection was closed by itself, false if an error happened during the Ping command</returns>
        async Task<bool> PingAsync()
        {
            if (m_PingCancellationSource != null)
            {
                throw new Exception("ping cancellation already exists");
            }

            m_PingCancellationSource = new CancellationTokenSource();
            while (true)
            {
                Command<PingRequest> command = new Command<PingRequest>(Message.Method.PING, new PingRequest());
                try
                {
                    var res = await SendCommandAsync(command.id, command);
                }
                catch (CommandInterruptedException)
                {
                    OnPingInterrupted(null);
                    return false;
                }
                catch (Exception e)
                {
                    OnPingInterrupted(e);
                    return false;
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(m_Config.PingIntervalInSeconds),
                        m_PingCancellationSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            m_PingCancellationSource = null;

            return true;
        }

        private void OnPingInterrupted(Exception exception)
        {
            if (exception != null)
            {
                Logger.LogError("Exception caught during Ping command: " + exception.Message);
            }

            m_WebsocketClient.Close();
            m_PingCancellationSource = null;
        }

        internal void Disconnect()
        {
            if (m_WebsocketClient != null)
            {
                ChangeConnectionState(ConnectionState.Disconnecting);
                m_WebsocketClient.Close();
            }
            else
            {
                ChangeConnectionState(ConnectionState.Disconnected);
            }
        }

        public async Task ConnectAsync()
        {
            Logger.LogVerbose("Connection initiated. Checking state prior to connection.");
            while (m_ConnectionState == ConnectionState.Disconnecting)
            {
                Logger.LogVerbose(
                    "Disconnection already in progress. Waiting for disconnection to complete before proceeding.");
                await m_DisconnectionCompletionSource.Task;
            }

            while (m_ConnectionState == ConnectionState.Connecting)
            {
                Logger.LogVerbose("Connection already in progress. Waiting for connection to complete.");
                await m_ConnectionCompletionSource.Task;
            }

            if (m_ConnectionState == ConnectionState.Connected)
            {
                Logger.LogVerbose("Already connected.");
                return;
            }

            Logger.LogVerbose("Proceeding to connection.");

            ChangeConnectionState(ConnectionState.Connecting);

            // initialize websocket object
            if (!m_WebsocketInitialized)
            {
                InitWebsocket();
            }

            // Connect to the websocket server
            Logger.Log($"Attempting connection on: {m_Config.address}");
            m_WebsocketClient.Connect();
            await m_ConnectionCompletionSource.Task;
        }

        private void InitWebsocket()
        {
            Logger.LogVerbose("Initializing Websocket.");
            m_WebsocketInitialized = true;
            // use the eventual websocket override instead of the default one
            if (m_Config.WebSocket != null)
            {
                m_WebsocketClient = m_Config.WebSocket;
            }
            else
            {
                m_WebsocketClient = WebSocketFactory.CreateInstance(m_Config.address);
            }

            //  Add OnOpen event listener
            m_WebsocketClient.OnOpen += async () =>
            {
                Logger.Log($"Websocket connected to : {m_Config.address}. Initiating Wire handshake.");
                var subscriptionRequests = await SubscribeRequest.getRequestFromRepo(SubscriptionRepository);
                var request = new ConnectRequest(m_Config?.token?.AccessToken ?? string.Empty, subscriptionRequests);
                var command = new Command<ConnectRequest>(Message.Method.CONNECT, request);
                Reply reply;
                try
                {
                    reply = await SendCommandAsync(command.id, command);
                }
                catch (CommandInterruptedException exception)
                {
                    // Wire handshake failed
                    m_ConnectionCompletionSource.SetException(
                        new ConnectionFailedException($"Socket closed during connection attempt: {exception.m_Code}"));
                    m_WebsocketClient.Close();
                    return;
                }
                catch (Exception exception)
                {
                    // Unknown exception caught during connection
                    m_ConnectionCompletionSource.SetException(exception);
                    m_WebsocketClient.Close();
                    return;
                }

                m_Backoff.Reset();
                try
                {
                    SubscriptionRepository.RecoverSubscriptions(reply);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
                ChangeConnectionState(ConnectionState.Connected);
            };

            // Add OnMessage event listener
            m_WebsocketClient.OnMessage += (byte[] payload) =>
            {
                try
                {
                    if (m_Metrics != null)
                    {
                        m_Metrics.SendSumMetric("message_received", 1);
                    }
                    Logger.LogVerbose("WS received message: " + Encoding.UTF8.GetString(payload));
                    var messages = BatchMessages
                        .SplitMessages(payload); // messages can be batched so we need to split them..
                    foreach (var message in messages)
                    {
                        var reply = Reply.FromJson(message);

                        if (reply.id > 0)
                        {
                            HandleCommandReply(reply);
                        }
                        else if (reply.result?.type > 0)
                        {
                            HandlePushMessage(reply);
                        }
                        else
                        {
                            HandlePublications(reply);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                    // TODO: try and find a way of reporting this exception
                }
            };

            // Add OnError event listener
            m_WebsocketClient.OnError += (string errMsg) =>
            {
                m_Metrics.SendSumMetric("websocket_error", 1);
                Logger.LogError("Websocket connection error: " + errMsg);
                // TODO: try and find a way of reporting this error
            };

            // Add OnClose event listener
            m_WebsocketClient.OnClose += async (WebSocketCloseCode originalcode) =>
            {
                var code = (CentrifugeCloseCode)originalcode;
                Logger.Log("Websocket closed with code: " + code);
                ChangeConnectionState(ConnectionState.Disconnected);
                m_CommandManager.OnDisconnect(new CommandInterruptedException($"websocket disconnected: {code}", code));
                if (m_DisconnectionCompletionSource != null)
                {
                    m_DisconnectionCompletionSource.SetResult(ConnectionState.Disconnected);
                    m_DisconnectionCompletionSource = null;
                }

                if (ShouldReconnect(code))
                {
                    var secondsUntilNextAttempt = m_Backoff.GetNext();
                    Logger.LogVerbose($"Retrying websocket connection in : {secondsUntilNextAttempt} s");
                    await Task.Delay(TimeSpan.FromSeconds(secondsUntilNextAttempt));
                    await m_ThreadUtils.PostAsync(async () => { await ConnectAsync(); });
                }
            };
        }

        private bool ShouldReconnect(CentrifugeCloseCode code)
        {
            switch (code)
            {
                case CentrifugeCloseCode.WebsocketNotSet:
                case CentrifugeCloseCode.WebsocketNormal:
                case CentrifugeCloseCode.WebsocketAway:
                case CentrifugeCloseCode.WebsocketUnsupportedData:
                case CentrifugeCloseCode.WebsocketMandatoryExtension:
                case CentrifugeCloseCode.Normal:
                case CentrifugeCloseCode.InvalidToken:
                case CentrifugeCloseCode.ForceNoReconnect:
                    return false;
                case CentrifugeCloseCode.WebsocketProtocolError:
                case CentrifugeCloseCode.WebsocketAbnormal:
                case CentrifugeCloseCode.WebsocketUndefined:
                case CentrifugeCloseCode.WebsocketNoStatus:
                case CentrifugeCloseCode.WebsocketInvalidData:
                case CentrifugeCloseCode.WebsocketPolicyViolation:
                case CentrifugeCloseCode.WebsocketTooBig:
                case CentrifugeCloseCode.WebsocketServerError:
                case CentrifugeCloseCode.WebsocketTlsHandshakeFailure:
                case CentrifugeCloseCode.Shutdown:
                case CentrifugeCloseCode.BadRequest:
                case CentrifugeCloseCode.InternalServerError:
                case CentrifugeCloseCode.Expired:
                case CentrifugeCloseCode.SubscriptionExpired:
                case CentrifugeCloseCode.Stale:
                case CentrifugeCloseCode.Slow:
                case CentrifugeCloseCode.WriteError:
                case CentrifugeCloseCode.InsufficientState:
                case CentrifugeCloseCode.ForceReconnect:
                case CentrifugeCloseCode.ConnectionLimit:
                case CentrifugeCloseCode.ChannelLimit:
                default:
                    return true;
            }
        }

        void ChangeConnectionState(ConnectionState state)
        {
            var tags = new Dictionary<string, string> {{"state", state.ToString()},};
            if (m_Metrics != null)
            {
                m_Metrics.SendSumMetric("connection_state_change", 1, tags);
            }
            m_ConnectionState = state;
            switch (state)
            {
                case ConnectionState.Disconnected:
                    Logger.LogVerbose("Wire disconnected.");
                    SubscriptionRepository.OnSocketClosed();
                    m_PingCancellationSource?.Cancel();
                    break;
                case ConnectionState.Connected:
                    Logger.LogVerbose("Wire connected.");
                    m_ConnectionCompletionSource.SetResult(ConnectionState.Connected);
                    m_ConnectionCompletionSource = null;
                    if (m_PingTask == null || m_PingTask.IsCompleted)
                    {
                        m_PingTask = PingAsync(); // start ping pong thread
                    }
                    else
                    {
                        // TODO: report something wrong
                    }

                    break;
                case ConnectionState.Connecting:
                    Logger.LogVerbose("Wire connecting...");
                    if (m_ConnectionCompletionSource == null)
                    {
                        m_ConnectionCompletionSource = new TaskCompletionSource<ConnectionState>();
                    }

                    break;
                case ConnectionState.Disconnecting:
                    Logger.LogVerbose("Wire is disconnecting");
                    if (m_DisconnectionCompletionSource == null)
                    {
                        m_DisconnectionCompletionSource = new TaskCompletionSource<ConnectionState>();
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        // Handle publications from a channel
        void HandlePublications(Reply reply)
        {
            if (string.IsNullOrEmpty(reply.result.channel))
            {
                throw new NoChannelPublicationException(reply.originalString);
            }

            var subscription = SubscriptionRepository.GetSub(reply.result.channel);
            if (subscription == null)
            {
                Logger.LogError(
                    $"The Wire server is sending publications related to an unknown channel: {reply.result.channel}.");
                return;
            }

            subscription.OnMessageReceived(reply);
        }

        // Handle push actions emitted from the server
        void HandlePushMessage(Reply reply)
        {
            var tags = new Dictionary<string, string> { { "push_type", reply.result.type.ToString() } };
            if (m_Metrics != null)
            {
                m_Metrics.SendSumMetric("push_received", 1, tags);
            }
            var subscription = GetSubscriptionForReply(reply);
            if (subscription == null)
            {
                Logger.LogError($"The Wire server is sending push messages of type[{reply.result.type}] related to an unknown channel: {reply.result.channel}.");
                return;
            }
            switch (reply.result.type)
            {
                case PushType.UNSUB: // force unsubscribe from server
                {
                    subscription.OnKickReceived();
                    SubscriptionRepository.RemoveSub(subscription);
                    break;
                }
                case PushType.MESSAGE:
                {
                    Logger.LogVerbose($"PushMessage[{reply.originalString}]");
                    subscription.OnMessageReceived(reply);
                    break;
                }
                default:
                    Logger.LogError("Not implemented type: " + reply.result.type);
                    // TODO: find a way of reporting this
                    break;
            }
        }

        private Subscription GetSubscriptionForReply(Reply reply)
        {
            if (m_DirectSubscriptionChannel != null)
            {
                return SubscriptionRepository.GetSub(m_DirectSubscriptionChannel);
            }
            return SubscriptionRepository.GetSub(reply.result.channel);
        }

        // Handle replies from commands issued by the client
        void HandleCommandReply(Reply reply)
        {
            m_CommandManager.OnCommandReplyReceived(reply);
        }

        async Task SubscribeAsync(Subscription subscription)
        {
            await ConnectAsync();
            try
            {
                var token = await subscription.RetrieveTokenAsync();

                if (m_DirectClient)
                {
                    if (SubscriptionRepository.ServerHasSubscription(subscription))
                    {
                        Logger.LogVerbose($"Promoting Subscription[{subscription.Channel}]");
                        SubscriptionRepository.PromoteSubscriptionHandle(subscription);
                        m_DirectSubscriptionChannel = subscription.Channel;
                        return;
                    }
                }

                if (SubscriptionRepository.IsAlreadySubscribed(subscription))
                {
                    throw new AlreadySubscribedException(subscription.Channel);
                }

                var recover = SubscriptionRepository.IsRecovering(subscription);
                var request = new SubscribeRequest
                {
                    channel = subscription.Channel, token = token, recover = recover, offset = subscription.Offset
                };
                var command = new Command<SubscribeRequest>(Message.Method.SUBSCRIBE, request);
                var reply = await SendCommandAsync(command.id, command);

                subscription.Epoch = reply.result.epoch;
                SubscriptionRepository.OnSubscriptionComplete(subscription, reply);
            }
            catch (Exception exception)
            {
                subscription.OnError($"Subscription failed: {exception.Message}");
                // we caught an error while subscribing but connected for that one subscription
                // in this specific case, we need to disconnect

                if (SubscriptionRepository.IsEmpty)
                {
                    Disconnect();
                }

                throw;
            }
        }

        public IChannel CreateChannel(IChannelTokenProvider tokenProvider)
        {
            var subscription = new Subscription(tokenProvider, m_ThreadUtils);
            subscription.UnsubscribeReceived += async (TaskCompletionSource<bool> completionSource) =>
            {
                try
                {
                    if (SubscriptionRepository.IsAlreadySubscribed(subscription))
                    {
                        await UnsubscribeAsync(subscription);
                    }
                    else
                    {
                        SubscriptionRepository.RemoveSub(subscription);
                    }

                    completionSource.SetResult(true);
                }
                catch (Exception e)
                {
                    // TODO: find a way of reporting this
                    Logger.LogException(e);
                    completionSource.SetException(e);
                }
            };
            subscription.SubscribeReceived += async (TaskCompletionSource<bool> completionSource) =>
            {
                try
                {
                    await SubscribeAsync(subscription);
                    completionSource.SetResult(true);
                }
                catch (Exception e)
                {
                    completionSource.SetException(e);
                }
            };
            subscription.KickReceived += () =>
            {
                SubscriptionRepository.RemoveSub(subscription);
            };
            subscription.DisposeReceived += () =>
            {
                SubscriptionRepository.RemoveSub(subscription);
            };
            return subscription;
        }

        async Task UnsubscribeAsync(Subscription subscription)
        {
            if (!SubscriptionRepository.IsAlreadySubscribed(subscription))
            {
                throw new AlreadyUnsubscribedException(subscription.Channel);
            }

            var request = new UnsubscribeRequest {channel = subscription.Channel,};

            var command = new Command<UnsubscribeRequest>(Message.Method.UNSUBSCRIBE, request);
            await SendCommandAsync(command.id, command);
            SubscriptionRepository.RemoveSub(subscription);
        }
    }
}
