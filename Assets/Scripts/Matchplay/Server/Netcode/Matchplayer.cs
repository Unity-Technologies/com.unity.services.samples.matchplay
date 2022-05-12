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
        public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>();
        public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>();

        [SerializeField] Renderer playerRenderer;
        public override void OnNetworkSpawn()
        {
            if (IsServer && !IsHost)
                return;
            SetColor(Color.black,PlayerColor.Value);
            PlayerColor.OnValueChanged += SetColor;
            ClientSingleton.Instance.Manager.AddMatchPlayer(this);
        }

        void SetColor(Color oldColor, Color newColor)
        {
            if (oldColor == newColor)
                return;

            playerRenderer.material.color = newColor;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && !IsHost)
                return;

            ClientSingleton.Instance.Manager.RemoveMatchPlayer(this);
        }
    }
}
