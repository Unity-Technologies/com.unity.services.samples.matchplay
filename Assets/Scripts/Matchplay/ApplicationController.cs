using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Matchplay.Client;
using Matchplay.Server;
using UnityEngine;

namespace Matchplay.Shared
{
    public class ApplicationController : MonoBehaviour
    {
        //Manager instances to be instantiated.
        [SerializeField]
        ServerGameManager m_ServerPrefab;
        [SerializeField]
        ClientGameManager m_ClientPrefab;

        [SerializeField]
        List<GameObject> m_ServerManagers = new List<GameObject>();

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
            if (isServer)
            {
                InstantiateManagers(m_ServerManagers);

                var serverInstance = Instantiate(m_ServerPrefab);
                serverInstance.Init();
#pragma warning disable 4014
                serverInstance.BeginServerAsync();
#pragma warning restore 4014
            }
            else
            {
                AuthenticationWrapper.BeginAuth();
                var clientInstance = Instantiate(m_ClientPrefab);
                clientInstance.Init();
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
