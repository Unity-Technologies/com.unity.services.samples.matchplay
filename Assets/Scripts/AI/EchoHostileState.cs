using UnityEngine;

namespace MilehighWorld.AI
{
    public class EchoHostileState : IState
    {
        public void Enter(DesolateEchoAI ai)
        {
            Debug.Log("[EchoHostileState] Entered. Target locked.");
        }

        public void Tick(DesolateEchoAI ai)
        {
            // Hostile behavior: pursuing and attacking
        }

        public void Exit(DesolateEchoAI ai)
        {
            Debug.Log("[EchoHostileState] Exited.");
        }
    }
}
