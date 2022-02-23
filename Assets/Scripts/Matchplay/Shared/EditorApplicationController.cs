#if UNITY_EDITOR
using ParrelSync;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Matchplay.Editor
{
    ///Helps launch ParrelSynced Projects for easy testing
    public class EditorApplicationController : MonoBehaviour
    {
        public UnityEvent onServerStart;
        public UnityEvent onClientStart;
        public void Start()
        {
#if UNITY_EDITOR

            if (ClonesManager.IsClone())
            {
                var argument = ClonesManager.GetArgument();
                if (argument == "server")
                    onServerStart?.Invoke();
                else if (argument == "client")
                {
                    onClientStart?.Invoke();
                }
            }
            else
                onClientStart?.Invoke();
#endif
        }
    }
}
