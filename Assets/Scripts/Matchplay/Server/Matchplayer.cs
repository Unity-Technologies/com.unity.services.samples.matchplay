using System;
using Matchplay.Networking;
using Unity.Netcode;
using UnityEngine;

namespace Matchplay.Server
{
    /// <summary>
    /// Currently there is no control for moving the player around, only the server does.
    /// </summary>
    public class Matchplayer : NetworkBehaviour
    {
        public string PlayerName { get; private set; }

        /// <summary>
        /// Server Only
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

        [ServerRpc]
        public void SetName_ServerRpc(string name)
        {
            PlayerName = name;
        }
    }
}
