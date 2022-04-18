using System;
using Unity.Netcode;
using UnityEngine;

namespace Matchplay.Shared
{
    /// <summary>
    /// The Shared Network Server State
    /// </summary>
    public class SynchedServerData : NetworkBehaviour
    {
        public NetworkVariable<Map> map = new NetworkVariable<Map>(NetworkVariableReadPermission.Everyone);
        public NetworkVariable<GameMode> gameMode = new NetworkVariable<GameMode>(NetworkVariableReadPermission.Everyone);
        public NetworkVariable<GameQueue> gameQueue = new NetworkVariable<GameQueue>(NetworkVariableReadPermission.Everyone);
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
