using System;
using Matchplay.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Matchplay.Shared
{
    /// <summary>
    /// The Shared Network Server State
    /// </summary>
    public class SynchedServerData : NetworkBehaviour
    {
        public static SynchedServerData Instance { get; private set; }

        [HideInInspector]
        public NetworkVariable<NetworkString> serverID = new NetworkVariable<NetworkString>();
        public NetworkVariable<Map> map = new NetworkVariable<Map>();
        public NetworkVariable<GameMode> gameMode = new NetworkVariable<GameMode>();
        public NetworkVariable<GameQueue> gameQueue = new NetworkVariable<GameQueue>();

        private readonly System.Collections.Generic.HashSet<ulong> _shearedAnchors = new System.Collections.Generic.HashSet<ulong>();

        /// <summary>
        /// NetworkedVariables have no built-in callback for the initial client-server synch.
        /// This lets non-networked classes know when we are ready to read the values.
        /// </summary>
        public Action OnNetworkSpawned;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            OnNetworkSpawned?.Invoke();
        }

        public void RegisterShearedAnchor(ulong networkObjectId)
        {
            if (!_shearedAnchors.Contains(networkObjectId))
            {
                _shearedAnchors.Add(networkObjectId);
                Debug.Log($"[SynchedServerData]: Anchor {networkObjectId} registered as sheared (stability drain broadcasted).");
            }
        }
    }
}
