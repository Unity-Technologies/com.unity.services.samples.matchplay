using System;
using System.Collections.Generic;
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

        private HashSet<ulong> _validatedClients = new HashSet<ulong>();

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
            }
            else
            {
                Instance = this;
            }
        }

        public override void OnNetworkSpawn()
        {
            OnNetworkSpawned?.Invoke();
        }

        public bool IsClientValidated(ulong clientId)
        {
            return _validatedClients.Contains(clientId);
        }

        public void ValidateClient(ulong clientId)
        {
            if (IsServer)
            {
                _validatedClients.Add(clientId);
            }
        }
    }
}
