using Unity.Services.Core;
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

        public ClientGameManager Manager
        {
            get
            {
                if (m_GameManager != null) return m_GameManager;
                Debug.LogError($"ClientGameManager is missing, did you run StartClient()?", gameObject);
                return null;
            }
        }

        ClientGameManager m_GameManager;

        public void CreateClient(string profileName = "default")
        {
            m_GameManager = new ClientGameManager(profileName);
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        // Update is called once per frame
        void OnDestroy()
        {
            Manager?.Dispose();
        }
    }
}
