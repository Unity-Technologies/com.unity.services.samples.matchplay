using Matchplay.Client;
using Matchplay.Networking;
using Matchplay.Shared;
using Matchplay.Shared.Tools;
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
        [HideInInspector]
        public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>();
        [HideInInspector]
        public NetworkVariable<NetworkString> PlayerName = new NetworkVariable<NetworkString>();
        [SerializeField]
        RendererColorer m_ColorSwitcher;

        public override void OnNetworkSpawn()
        {
            if (IsServer && !IsHost)
                return;

            SetColor(Color.black, PlayerColor.Value);
            PlayerColor.OnValueChanged += SetColor;
            ClientSingleton.Instance.Manager.AddMatchPlayer(this);
        }

        void SetColor(Color oldColor, Color newColor)
        {
            if (oldColor == newColor)
                return;

            m_ColorSwitcher.SetColor(newColor);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && !IsHost)
                return;
            if (ApplicationData.IsServerUnitTest)
                return;

            ClientSingleton.Instance.Manager.RemoveMatchPlayer(this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestDesyncStateServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            var senderId = rpcParams.Receive.SenderClientId;
            if (senderId != clientId)
            {
                Debug.LogWarning($"DoS attempt detected! Client {senderId} tried to desync client {clientId}. Severing attacker's tether.");
                SeverTetherCommand(senderId);
                return;
            }

            if (SynchedServerData.Instance != null && SynchedServerData.Instance.IsClientEchoed(clientId))
            {
                Debug.LogWarning($"Client {clientId} is already echoed. Severing tether.");
                SeverTetherCommand(clientId);
                return;
            }

            ExecuteVoidShear(clientId);
        }

        void SeverTetherCommand(ulong clientId)
        {
            Debug.Log($"Severing tether for client {clientId} due to suspected exploit.");
            NetworkManager.DisconnectClient(clientId);
        }

        void ExecuteVoidShear(ulong clientId)
        {
            Debug.Log($"Executing Void Shear for client {clientId}.");
            if (SynchedServerData.Instance != null)
            {
                SynchedServerData.Instance.MarkClientEchoed(clientId);
            }
            // Additional state transition logic would go here
        }
    }
}