using System.Collections.Generic;
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

        private HashSet<ulong> m_EchoedClients = new HashSet<ulong>();

        public bool IsClientEchoed(ulong clientId)
        {
            return m_EchoedClients.Contains(clientId);
        }

        public void MarkClientEchoed(ulong clientId)
        {
            m_EchoedClients.Add(clientId);
        }

        /// <summary>
        /// NetworkedVariables have no built-in callback for the initial client-server synch.
        /// This lets non-networked classes know when we are ready to read the values.
        /// </summary>
        public Action OnNetworkSpawned;

        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
            }

            OnNetworkSpawned?.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}