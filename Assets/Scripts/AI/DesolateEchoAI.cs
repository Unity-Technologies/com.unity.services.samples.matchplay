using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using MilehighWorld.CombatSystems;

namespace MilehighWorld.AI
{
    public class DesolateEchoAI : NetworkBehaviour
    {
        private IState _currentState;

        // State References
        public readonly EchoPassiveState PassiveState = new EchoPassiveState();
        public readonly EchoHostileState HostileState = new EchoHostileState();

        [SerializeField] private CombatEventSO _echoAggroEvent; // Referencing existing CombatSystems
        [SerializeField] private NavMeshAgent _navAgent;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            TransitionToState(PassiveState);
        }

        public void TransitionToState(IState newState)
        {
            _currentState?.Exit(this);
            _currentState = newState;
            _currentState.Enter(this);
        }

        // Triggered by the Server when remaining Anchors vote to Sever
        public void EngageHostileDefenseMode()
        {
            if (!IsServer) return;

            TransitionToState(HostileState);
            BroadcastAggroTriggerClientRpc();
        }

        [ClientRpc]
        private void BroadcastAggroTriggerClientRpc()
        {
            // Execute visual/audio permutations locally
            if (_echoAggroEvent != null)
            {
                _echoAggroEvent.RaiseEvent(this.transform.position);
            }
        }

        private void Update()
        {
            if (!IsServer) return;
            _currentState?.Tick(this);
        }
    }
}
