using System;
using Unity.Collections;
using Unity.Netcode;

namespace Matchplay.Shared
{
    /// <summary>
    /// The Shared Network Server State
    /// </summary>
    public class SynchedServerData : NetworkBehaviour
    {
        public NetworkVariable<FixedString64Bytes> serverID = new NetworkVariable<FixedString64Bytes>();
        public NetworkVariable<Map> map = new NetworkVariable<Map>();
        public NetworkVariable<GameMode> gameMode = new NetworkVariable<GameMode>();
        public NetworkVariable<GameQueue> gameQueue = new NetworkVariable<GameQueue>();
        /// <summary>
        /// NetworkedVariables have no built-in callback for the initial client-server synch.
        /// This lets non-networked classes know when we are ready to read the values.
        /// </summary>
        public Action OnNetworkSpawned;

        public override void OnNetworkSpawn()
        {
            OnNetworkSpawned?.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            map.Value = Map.None;
            gameMode.Value = GameMode.None;
            gameQueue.Value = GameQueue.None;
        }
    }
}
