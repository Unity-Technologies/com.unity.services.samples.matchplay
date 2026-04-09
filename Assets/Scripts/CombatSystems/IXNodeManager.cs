using System.Collections.Generic;
using UnityEngine;

namespace MilehighWorld.CombatSystems
{
    /// <summary>
    /// Singleton manager that coordinates IX-Nodes across the scene.
    /// </summary>
    public class IXNodeManager : MonoBehaviour
    {
        public static IXNodeManager Instance { get; private set; }

        private readonly Dictionary<string, IXNodeController> _nodes = new Dictionary<string, IXNodeController>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional based on game flow
        }

        public void RegisterNode(IXNodeController node)
        {
            if (!_nodes.ContainsKey(node.nodeId))
            {
                _nodes.Add(node.nodeId, node);
            }
            else
            {
                Debug.LogWarning($"[IXNodeManager] Node with ID {node.nodeId} is already registered.");
            }
        }

        public void UnregisterNode(IXNodeController node)
        {
            if (_nodes.ContainsKey(node.nodeId))
            {
                _nodes.Remove(node.nodeId);
            }
        }

        /// <summary>
        /// Updates a specific node's state and variance based on telemetry.
        /// </summary>
        public void UpdateNode(string id, float variance, string state)
        {
            if (_nodes.TryGetValue(id, out var node))
            {
                node.UpdateNodeData(variance, state);
            }
            else
            {
                Debug.LogWarning($"[IXNodeManager] Received update for unknown Node ID: {id}");
            }
        }
    }
}
