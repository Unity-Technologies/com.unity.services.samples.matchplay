using System;
using Matchplay.Client;
using Matchplay.Networking;
using Matchplay.Server;
using Matchplay.Infrastructure;
using UnityEngine;

namespace Matchplay.Shared
{
    public class ApplicationController : MonoBehaviour
    {

        [SerializeField]
        UpdateRunner m_UpdateRunnerPrefab;

        void Awake()
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
            var scope = DIScope.RootScope;
            scope.BindAsSingle<ApplicationData>();

            if (isServer)
            {
                var updateInstance = Instantiate(m_UpdateRunnerPrefab);
                DontDestroyOnLoad(updateInstance.gameObject);
                scope.BindInstanceAsSingle(updateInstance);
                scope.BindAsSingle<MatchplayServer>();
                scope.BindAsSingle<UnitySqp>();
                scope.BindAsSingle<ServerGameManager>();
            }
            else
            {
                scope.BindAsSingle<AuthenticationHandler>();
                scope.BindAsSingle<MatchplayMatchmaker>();
                scope.BindAsSingle<MatchplayClient>();
                scope.BindAsSingle<ClientGameManager>();
            }

            scope.FinalizeScopeConstruction();

            if (isServer)
            {
                scope.Resolve<ServerGameManager>().BeginServer();
            }
            else
            {
                scope.Resolve<AuthenticationHandler>().BeginAuth();
                scope.Resolve<ClientGameManager>().ToMainMenu();
            }
        }

        void OnDestroy()
        {
            //Will Call Dispose on all IDisposable in the rootscope.
            DIScope.RootScope.Dispose();
        }
    }
}
