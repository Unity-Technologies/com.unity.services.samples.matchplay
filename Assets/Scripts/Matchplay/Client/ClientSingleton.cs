using UnityEngine;

namespace Matchplay.Client
{
    public class ClientSingleton : MonoBehaviour
    {
        public static ClientSingleton Instance
        {
            get
            {
                if (s_ClientGameManager != null) return s_ClientGameManager;
                s_ClientGameManager = FindObjectOfType<ClientSingleton>();
                if (s_ClientGameManager == null)
                {
                    Debug.LogError("No ClientSingleton in scene, did you run this from the bootStrap scene?");
                    return null;
                }

                return s_ClientGameManager;
            }
        }

        static ClientSingleton s_ClientGameManager;

        public ClientGameManager Manager = new ClientGameManager();

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        // Update is called once per frame
        void OnDestroy()
        {
            Manager.Dispose();
        }
    }
}
