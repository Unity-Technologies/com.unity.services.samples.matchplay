using System;
using Matchplay.Client;
using Matchplay.Networking;
using Matchplay.Server;
using Matchplay.Shared.Infrastructure;
using Samples.UI;
using UnityEngine;

namespace Matchplay.Shared
{
    public class ApplicationController : MonoBehaviour
    {
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="isServer"></param>
        public void LaunchInMode(bool isServer)
        {
            var scope = DIScope.RootScope;
            scope.BindAsSingle<ApplicationData>();

            if (isServer)
            {
                scope.BindAsSingle<UpdateRunner>();
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
                scope.BindAsSingle<MainMenuUI>();
            }

            scope.FinalizeScopeConstruction();

            if (isServer)
            {
                scope.Resolve<ServerGameManager>().BeginServer();
            }
            else
            {
                scope.Resolve<ClientGameManager>().ToMainMenu();
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            //We use EditorApplicationController for Editor launching.
            if (Application.isEditor)
                return;
            LaunchInMode(ApplicationData.IsServerMode());
        }

        void OnDestroy()
        {
            //Will Call Dispose on all IDisposable in the rootscope.
            DIScope.RootScope.Dispose();
        }
    }
}
