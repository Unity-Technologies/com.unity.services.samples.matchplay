using System;
using System.Collections;
using System.Threading.Tasks;
using Matchplay.Client;
using Matchplay.Infrastructure;
using Matchplay.Shared;
using NUnit.Framework;
using Matchplay.Tests.Helpers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;

namespace Matchplay.Tests
{
    public class ClientRuntimeTests
    {
        DIScope m_RootScope;
        NetworkManager m_NetworkManager;
        AuthenticationHandler m_AuthenticationHandler;

        /// <summary>
        /// Setting up the DIScope for the classes needed to run a client.
        /// This behaviour is encapsulated in the ApplicationController.cs for regular runs.
        /// </summary>
        [OneTimeSetUp]
        public async void ClientSetup()
        {
            m_NetworkManager = GameObject.Instantiate(Resources.Load<NetworkManager>("NetworkManager"));
            m_RootScope = DIScope.RootScope;
            m_RootScope.BindInstanceAsSingle(m_NetworkManager);
            m_RootScope.BindAsSingle<ApplicationData>();
            m_RootScope.BindAsSingle<AuthenticationHandler>();
            m_RootScope.BindAsSingle<MatchplayMatchmaker>();
            m_RootScope.BindAsSingle<MatchplayClient>();
            m_RootScope.BindAsSingle<ClientGameManager>();
            m_RootScope.FinalizeScopeConstruction();

            m_AuthenticationHandler = m_RootScope.Resolve<AuthenticationHandler>();
            await m_AuthenticationHandler.BeginAuth();
        }

        [OneTimeTearDown]
        public void ClientTearDown()
        {
            m_RootScope.Dispose();
        }

        /// <summary>
        /// Checks that Matchmaking connects correctly
        /// </summary>
        [UnityTest]
        public IEnumerator ClientCanMatchmake()
        {
            var matchMaker = m_RootScope.Resolve<MatchplayMatchmaker>();
            var clientGameManager = m_RootScope.Resolve<ClientGameManager>();

            async Task TestMatchmakingAsync()
            {
                await m_AuthenticationHandler.Authenticating();
                clientGameManager.Matchmake();

                Assert.IsTrue(matchMaker.IsMatchmaking);
            }

            yield return AsyncTestHelpers.ExecuteTask(TestMatchmakingAsync());
        }



    }
}
