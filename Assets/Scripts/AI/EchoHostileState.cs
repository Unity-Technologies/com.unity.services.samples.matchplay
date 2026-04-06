using UnityEngine;

namespace MilehighWorld.AI
{
    public class EchoHostileState : IState
    {
        private DesolateEchoAI _ai;

        public EchoHostileState(DesolateEchoAI ai)
        {
            _ai = ai;
        }

        public void Enter()
        {
            Debug.Log($"[{_ai.gameObject.name}] Entering Hostile State");
        }

        public void Update()
        {
            // Hostile behavior: move toward target and attack
            if (_ai.target != null)
            {
                Debug.Log($"[{_ai.gameObject.name}] Attacking target: {_ai.target.name}");
            }
        }

        public void Exit()
        {
            Debug.Log($"[{_ai.gameObject.name}] Exiting Hostile State");
        }
    }
}
