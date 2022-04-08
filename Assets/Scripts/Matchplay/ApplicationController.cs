using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Matchplay.Client;
using Matchplay.Server;
using Unity.Services.Core;
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

        CommandParser m_Parser;

        async void Start()
        {
            DontDestroyOnLoad(gameObject);

            //We use EditorApplicationController for Editor launching.
            if (Application.isEditor)
                return;
            await LaunchInMode(CommandParser.IsServerMode());
        }

        public void OnParrelSyncStarted(bool isServer)
        {
#pragma warning disable 4014
            LaunchInMode(isServer);
#pragma warning restore 4014
        }

        /// <summary>
        /// Main project launcher, launched in Awake() for builds, and via the EditorApplicationController in-editor
        /// </summary>
        async Task LaunchInMode(bool isServer)
        {
            //init the command parser, get launch args
            m_Parser = new CommandParser();

            await UnityServices.InitializeAsync();

            if (isServer)
            {
                var serverInstance = Instantiate(m_ServerPrefab);

                await serverInstance.BeginServerAsync();
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
