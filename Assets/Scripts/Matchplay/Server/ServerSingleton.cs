using System;
using UnityEngine;

namespace Matchplay.Server
{
    /// <summary>
    /// Monobehaviour Singleton pattern for easy access to the Server Game Manager
    /// We seperated the logic away from the Monobehaviour, so we could more easily write tests for it.
    /// </summary>
    public class ServerSingleton : MonoBehaviour
    {
        public static ServerSingleton Instance
        {
            get
            {
                if (s_ServerSingleton != null) return s_ServerSingleton;
                s_ServerSingleton = FindObjectOfType<ServerSingleton>();
                if (s_ServerSingleton == null)
                {
                    Debug.LogError("No ClientSingleton in scene, did you run this from the bootStrap scene?");
                    return null;
                }

                return s_ServerSingleton;
            }
        }

        static ServerSingleton s_ServerSingleton;

        public ServerGameManager Manager = new ServerGameManager();

        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            Manager.Dispose();
        }
    }
}
