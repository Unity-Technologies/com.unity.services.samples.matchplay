using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MilehighWorld.CombatSystems
{
    // ==============================================================================
    // MILEHIGH-WORLD LLC | INTO THE VOID
    // ARCHITECTURE: Unity C# WebSocket Reality Sync Client
    // ==============================================================================

    [Serializable]
    public class IXNodeData
    {
        public string id;
        public float variance;
        public string state; // "NOW" | "VOID"
    }

    [Serializable]
    public class SyncPayload
    {
        public string type;
        public IXNodeData[] nodes;
    }

    [Serializable]
    public class PulseMessage
    {
        public string nodeId;
        public float deltaVariance;
    }

    public class RealitySyncClient : MonoBehaviour
    {
        [Header("Network Configuration")]
        [SerializeField] private string wssUrl = "ws://localhost:8080";
        [SerializeField] private int maxRetryAttempts = 5;
        [SerializeField] private float baseRetryDelay = 2f;

        private ClientWebSocket _ws;
        private CancellationTokenSource _cts;

        // Thread-safe queue to pass background WebSocket data to Unity's main thread
        private readonly ConcurrentQueue<SyncPayload> _syncQueue = new ConcurrentQueue<SyncPayload>();

        private async void Start()
        {
            await ConnectToGridWithRetryAsync();
        }

        private async Task ConnectToGridWithRetryAsync()
        {
            int attempt = 0;
            while (attempt < maxRetryAttempts)
            {
                if (_cts != null && _cts.IsCancellationRequested) return;

                _ws = new ClientWebSocket();
                _cts = new CancellationTokenSource();

                try
                {
                    Debug.Log($"[RealitySync] Attempting to connect to {wssUrl} (Attempt {attempt + 1})...");
                    await _ws.ConnectAsync(new Uri(wssUrl), _cts.Token);
                    Debug.Log("[RealitySync] Connected to the Void grid. IX-Node telemetry active.");

                    // Fire and forget the listening loop on a background thread
                    _ = ReceiveLoopAsync();
                    return; // Successfully connected
                }
                catch (Exception e)
                {
                    attempt++;
                    float delay = baseRetryDelay * Mathf.Pow(2, attempt - 1);
                    Debug.LogError($"[RealitySync] Grid connection failed: {e.Message}. Retrying in {delay}s...");

                    _ws.Dispose();
                    if (attempt < maxRetryAttempts)
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(delay), _cts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            return;
                        }
                    }
                }
            }

            Debug.LogError("[RealitySync] Maximum connection attempts reached. Grid sync offline.");
        }

        /// <summary>
        /// Background thread listener. Continually reads incoming STATE_SYNC broadcasts.
        /// </summary>
        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[4096];

            while (_ws.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        SyncPayload payload = JsonUtility.FromJson<SyncPayload>(json);

                        if (payload != null && payload.type == "STATE_SYNC")
                        {
                            // Enqueue for main thread processing
                            _syncQueue.Enqueue(payload);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Grid disconnect", _cts.Token);
                    }
                }
                catch (Exception e)
                {
                    if (!_cts.Token.IsCancellationRequested)
                    {
                        Debug.LogWarning($"[RealitySync] Telemetry stream interrupted: {e.Message}");
                        // Trigger reconnection logic
                        _ = ReconnectAsync();
                    }
                    break;
                }
            }
        }

        private async Task ReconnectAsync()
        {
            _cts?.Cancel();
            _ws?.Dispose();
            await Task.Delay(1000); // Short pause before retrying
            await ConnectToGridWithRetryAsync();
        }

        private void Update()
        {
            // Unity Main Thread execution: Process queued state changes
            while (_syncQueue.TryDequeue(out SyncPayload payload))
            {
                ProcessRealityState(payload);
            }
        }

        private void ProcessRealityState(SyncPayload payload)
        {
            if (IXNodeManager.Instance == null) return;

            foreach (var node in payload.nodes)
            {
                // Dispatch events to local IX-Node controllers to trigger
                // visual/audio shifts between NOW and VOID realities.
                IXNodeManager.Instance.UpdateNode(node.id, node.variance, node.state);
            }
        }

        /// <summary>
        /// Transmits local player stabilization efforts to the Node.js orchestrator.
        /// </summary>
        public async void PulseVariance(string nodeId, float delta)
        {
            if (_ws == null || _ws.State != WebSocketState.Open) return;

            try
            {
                var pulse = new PulseMessage { nodeId = nodeId, deltaVariance = delta };
                string json = JsonUtility.ToJson(pulse);
                var bytes = Encoding.UTF8.GetBytes(json);

                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RealitySync] Failed to pulse variance: {e.Message}");
            }
        }

        private async void OnDestroy()
        {
            _cts?.Cancel();
            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                try
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client shutting down", CancellationToken.None);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[RealitySync] Error closing WebSocket: {e.Message}");
                }
            }
            _ws?.Dispose();
        }
    }
}
