using System;
using System.Collections.Generic;
using Matchplay.Client;
using Matchplay.Server;
using Matchplay.Infrastructure;
using UnityEngine;
using Unity.Netcode;

namespace Matchplay.Shared
{
    public class ApplicationController : MonoBehaviour
    {
        //Manager instances to be instantiated.
        [SerializeField]
        ServerGameManager m_serverPrefab;
        [SerializeField]
        ClientGameManager m_clientPrefab;
        [SerializeField]
        List<GameObject> m_sharedManagers = new List<GameObject>();
        [SerializeField]
        List<GameObject> m_serverManagers = new List<GameObject>();
        [SerializeField]
        List<GameObject> m_clientManagers = new List<GameObject>();

        void Start()
        {
            DontDestroyOnLoad(gameObject);

            //We use EditorApplicationController for Editor launching.
            if (Application.isEditor)
                return;
            LaunchInMode(ApplicationData.IsServerMode());
        }

        /// <summary>
        /// Main project launcher, launched in Awake() for builds, and via the EditorApplicationController in-editor
        /// </summary>
        public void LaunchInMode(bool isServer)
        {
            InstantiateManagers(m_sharedManagers);
            if (isServer)
            {
                InstantiateManagers(m_serverManagers);
                var serverInstance = Instantiate(m_serverPrefab);
                serverInstance.BeginServer();
            }
            else
            {
                InstantiateManagers(m_clientManagers);
                AuthenticationHandler.Singleton.BeginAuth();
                var clientInstance = Instantiate(m_clientPrefab);
                clientInstance.ToMainMenu();
            }
        }

        void InstantiateManagers(List<GameObject> managers)
        {
            foreach (var go in managers)
            {
                Instantiate(go);
            }
        }
    }
}
