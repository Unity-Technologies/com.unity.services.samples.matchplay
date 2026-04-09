using UnityEngine;

namespace MilehighWorld.CombatSystems
{
    /// <summary>
    /// Controls the visual and audio state of an individual IX-Node.
    /// Handles transitions between NOW and VOID realities.
    /// </summary>
    public class IXNodeController : MonoBehaviour
    {
        [Header("Node Settings")]
        public string nodeId;
        public float currentVariance;
        public string currentState = "NOW"; // "NOW" | "VOID"

        [Header("Visual References")]
        [SerializeField] private GameObject nowVisuals;
        [SerializeField] private GameObject voidVisuals;
        [SerializeField] private MeshRenderer nodeRenderer;

        [Header("Audio References")]
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioClip nowClip;
        [SerializeField] private AudioClip voidClip;

        private void Awake()
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                nodeId = name;
            }
        }

        private void Start()
        {
            // Register this node with the manager
            if (IXNodeManager.Instance != null)
            {
                IXNodeManager.Instance.RegisterNode(this);
            }

            ApplyState();
        }

        private void OnDestroy()
        {
            if (IXNodeManager.Instance != null)
            {
                IXNodeManager.Instance.UnregisterNode(this);
            }
        }

        /// <summary>
        /// Updates the node with new telemetry data.
        /// </summary>
        public void UpdateNodeData(float variance, string state)
        {
            currentVariance = variance;

            if (currentState != state)
            {
                currentState = state;
                ApplyState();
            }
        }

        private void Update()
        {
            // Apply variance effects every frame for smooth animation
            UpdateVarianceEffects();
        }

        private void ApplyState()
        {
            bool isVoid = currentState == "VOID";

            if (nowVisuals != null) nowVisuals.SetActive(!isVoid);
            if (voidVisuals != null) voidVisuals.SetActive(isVoid);

            if (ambientSource != null)
            {
                AudioClip targetClip = isVoid ? voidClip : nowClip;
                if (ambientSource.clip != targetClip)
                {
                    ambientSource.clip = targetClip;
                    ambientSource.Play();
                }
            }

            Debug.Log($"[IXNode] {nodeId} shifted to {currentState}");
        }

        private void UpdateVarianceEffects()
        {
            // Example: Adjust emission or scale based on variance
            if (nodeRenderer != null)
            {
                float intensity = Mathf.PingPong(Time.time * currentVariance, 1.0f);
                nodeRenderer.material.SetColor("_EmissiveColor", Color.cyan * intensity);
            }
        }
    }
}
