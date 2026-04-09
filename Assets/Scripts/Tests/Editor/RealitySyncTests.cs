using NUnit.Framework;
using UnityEngine;
using MilehighWorld.CombatSystems;
using System.Collections.Generic;

namespace MilehighWorld.Tests
{
    public class RealitySyncTests
    {
        [Test]
        public void SyncPayload_DeserializesCorrectly()
        {
            string json = "{\"type\":\"STATE_SYNC\",\"nodes\":[{\"id\":\"node1\",\"variance\":0.5,\"state\":\"NOW\"},{\"id\":\"node2\",\"variance\":0.8,\"state\":\"VOID\"}]}";
            SyncPayload payload = JsonUtility.FromJson<SyncPayload>(json);

            Assert.IsNotNull(payload);
            Assert.AreEqual("STATE_SYNC", payload.type);
            Assert.AreEqual(2, payload.nodes.Length);
            Assert.AreEqual("node1", payload.nodes[0].id);
            Assert.AreEqual(0.5f, payload.nodes[0].variance);
            Assert.AreEqual("NOW", payload.nodes[0].state);
            Assert.AreEqual("VOID", payload.nodes[1].state);
        }

        [Test]
        public void IXNodeManager_CanUpdateRegisteredNode()
        {
            var managerGo = new GameObject("IXNodeManager");
            var manager = managerGo.AddComponent<IXNodeManager>();

            var nodeGo = new GameObject("Node_Alpha");
            var node = nodeGo.AddComponent<IXNodeController>();
            node.nodeId = "Node_Alpha";

            // Manual registration since we're in editor and Start() might not have run
            manager.RegisterNode(node);

            manager.UpdateNode("Node_Alpha", 0.75f, "VOID");

            Assert.AreEqual(0.75f, node.currentVariance);
            Assert.AreEqual("VOID", node.currentState);

            Object.DestroyImmediate(managerGo);
            Object.DestroyImmediate(nodeGo);
        }

        [Test]
        public void IXNodeManager_HandlesUnknownNodeGracefully()
        {
            var managerGo = new GameObject("IXNodeManager");
            var manager = managerGo.AddComponent<IXNodeManager>();

            // Should not throw exception
            LogAssert.Expect(LogType.Warning, "[IXNodeManager] Received update for unknown Node ID: UnknownNode");
            manager.UpdateNode("UnknownNode", 0.5f, "NOW");

            Object.DestroyImmediate(managerGo);
        }
    }
}
