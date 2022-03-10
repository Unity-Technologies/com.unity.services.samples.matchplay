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
            if (IsClient)
            {
                ClientGameManager.Singleton.AddMatchPlayer(this);
            }
        }

        /// <summary>
        /// networkServer Only
        /// </summary>
        public void UpdatePlayerPos(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
            UpdatePlayerPos_ClientRpc(pos, rot);
        }

        /// <summary>
        /// Pass the values to the player
        /// </summary>
        [ClientRpc]
        void UpdatePlayerPos_ClientRpc(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
        }

        public void ServerSetName(string name)
        {
            PlayerName.Value = name;
        }

        public override void OnNetworkDespawn()
        {
            ClientGameManager.Singleton.RemoveMatchPlayer(this);
        }
    }
}
