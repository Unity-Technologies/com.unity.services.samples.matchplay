using UnityEngine;

namespace MilehighWorld.AI
{
    public class EchoPassiveState : IState
    {
        private DesolateEchoAI _ai;

        public EchoPassiveState(DesolateEchoAI ai)
        {
            _ai = ai;
        }

        public void Enter()
        {
            Debug.Log($"[{_ai.gameObject.name}] Entering Passive State");
        }

        public void Update()
        {
            // Passive behavior: just stay still or wander
        }

        public void Exit()
        {
            Debug.Log($"[{_ai.gameObject.name}] Exiting Passive State");
        }
    }
}
