using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;

namespace Matchplay.Shared
{
    public class AnchorDesyncHandler : NetworkBehaviour
    {
        [SerializeField] private DesolateEchoAI _echoAI;
        [SerializeField] private Material _voidShearMaterial;

        // Triggered upon Multiplay allocation timeout or client disconnect
        public void HandleClientDesync(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId) return;

            TransitionToVoidShearedState();
        }

        private void TransitionToVoidShearedState()
        {
            // 1. Decouple input systems; transition IState
            var inputHandler = GetComponent<PlayerInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.DisableInput();
            }

            if (_echoAI != null)
            {
                _echoAI.EngageStaticDefenseMode();
            }

            // 2. Asynchronous visual state change (prevent frame drop)
            _ = ApplyVoidShearVisualsAsync();

            // 3. Broadcast stability drain to the Mesh
            if (SynchedServerData.Instance != null)
            {
                SynchedServerData.Instance.RegisterShearedAnchor(this.NetworkObjectId);
            }
        }

        private async Task ApplyVoidShearVisualsAsync()
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material = _voidShearMaterial;
            }

            // Additional FX logic mapped to the Void Render Pipeline
            await Task.Yield();
        }
    }
}
