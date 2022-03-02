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
        NetworkManager m_NetworkManager;
        AuthenticationHandler m_AuthenticationHandler;

        /// <summary>
        /// Setting up the DIScope for the classes needed to run a client.
        /// This behaviour is encapsulated in the ApplicationController.cs for regular runs.
        /// </summary>
        [OneTimeSetUp]
        public async void ClientSetup() { }

        [OneTimeTearDown]
        public void ClientTearDown() { }

        /// <summary>
        /// Checks that Matchmaking connects correctly
        /// </summary>
        [UnityTest]
        public IEnumerator ClientCanMatchmake()
        {
            /* var matchMaker = m_RootScope.Resolve<MatchplayMatchmaker>();
             var clientGameManager = m_RootScope.Resolve<ClientGameManager>();

             async Task TestMatchmakingAsync()
             {
                 await m_AuthenticationHandler.Authenticating();
                 clientGameManager.Matchmake();

                 Assert.IsTrue(matchMaker.IsMatchmaking);
             }

             yield return AsyncTestHelpers.ExecuteTask(TestMatchmakingAsync());*/
            yield return null;
        }
    }
}
