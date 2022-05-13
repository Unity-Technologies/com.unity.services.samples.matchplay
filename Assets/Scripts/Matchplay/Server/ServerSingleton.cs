using System;
using System.Threading.Tasks;
using Matchplay.Shared;
using Unity.Netcode;
using Unity.Services.Core;
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
                    Debug.LogError("No ServerSingleton in scene, did you run this from the bootStrap scene?");
                    return null;
                }
                return s_ServerSingleton;
            }
        }

        static ServerSingleton s_ServerSingleton;

        public ServerGameManager Manager
        {
            get
            {
                if (m_GameManager != null)
                {
                    return m_GameManager;
                }

                Debug.LogError($"Server Manager is missing, did you run StartServer?");
                return null;
            }
        }

        ServerGameManager m_GameManager;

        /// <summary>
        /// Server Should start itself as soon as the game starts.
        /// </summary>
        public async Task CreateServer()
        {
            await UnityServices.InitializeAsync();

            m_GameManager = new ServerGameManager(
                ApplicationData.IP(),
                ApplicationData.Port(),
                ApplicationData.QPort(),
                NetworkManager.Singleton);
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            m_GameManager?.Dispose();
        }
    }
}
