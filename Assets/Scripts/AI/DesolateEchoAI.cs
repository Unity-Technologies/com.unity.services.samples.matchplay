using Unity.Netcode;
using UnityEngine;

namespace MilehighWorld.AI
{
    public class DesolateEchoAI : NetworkBehaviour
    {
        private IState _currentState;
        public EchoPassiveState PassiveState { get; private set; }
        public EchoHostileState HostileState { get; private set; }

        public GameObject target;

        private void Awake()
        {
            PassiveState = new EchoPassiveState(this);
            HostileState = new EchoHostileState(this);
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                enabled = false;
                return;
            }
            ChangeState(PassiveState);
        }

        private void Update()
        {
            if (!IsServer) return;
            _currentState?.Update();

            // Simple logic for state transition
            if (target != null && _currentState == PassiveState)
            {
                ChangeState(HostileState);
            }
            else if (target == null && _currentState == HostileState)
            {
                ChangeState(PassiveState);
            }
        }

        public void ChangeState(IState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }
    }
}
