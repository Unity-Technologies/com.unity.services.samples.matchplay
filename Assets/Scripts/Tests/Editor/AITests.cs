using NUnit.Framework;
using UnityEngine;
using MilehighWorld.AI;
using Unity.Netcode;

namespace MilehighWorld.Tests
{
    public class AITests
    {
        [Test]
        public void DesolateEchoAI_CanTransitionState()
        {
            var go = new GameObject("EchoAI");
            var ai = go.AddComponent<DesolateEchoAI>();

            // Initial state (simulating OnNetworkSpawn)
            ai.TransitionToState(ai.PassiveState);

            // Transition to Hostile
            ai.TransitionToState(ai.HostileState);

            // Just verifying it doesn't crash and transitions work
            Assert.Pass();

            Object.DestroyImmediate(go);
        }
    }
}
