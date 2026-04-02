using UnityEngine;

namespace MilehighWorld.AI
{
    public class EchoPassiveState : IState
    {
        public void Enter(DesolateEchoAI ai)
        {
            Debug.Log("[EchoPassiveState] Entered.");
        }

        public void Tick(DesolateEchoAI ai)
        {
            // Passive behavior: wandering or idling
        }

        public void Exit(DesolateEchoAI ai)
        {
            Debug.Log("[EchoPassiveState] Exited.");
        }
    }
}
