using System;
using Matchplay.Client;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Matchplay.Server
{
    /// <summary>
    /// Currently there is no control for moving the player around, only the server does.
    /// The NetworkManager spawns this in automatically, as it is on the designated player object.
    /// </summary>
    public class Matchplayer : NetworkBehaviour
    {
        public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(NetworkVariableReadPermission.Everyone);

        public override void OnNetworkSpawn()
        {
            if (IsServer && !IsHost)
                return;
            ClientGameManager.Singleton.AddMatchPlayer(this);
        }

        public void ServerSetName(string name)
        {
            PlayerName.Value = name;
        }

        public override void OnDestroy()
        {
            if (IsServer && !IsHost)
                return;

            ClientGameManager.Singleton.RemoveMatchPlayer(this);
        }
    }
}
