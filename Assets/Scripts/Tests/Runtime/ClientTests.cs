using System.Collections;
using System.Threading.Tasks;
using Matchplay.Client;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;

namespace Matchplay.Tests
{
    public class ClientTests
    {
        NetworkManager m_TestManager;

        [OneTimeSetUp]
        [RequiresPlayMode]
        public void OneTimeSetup()
        {
            m_TestManager = TestResources.TestNetworkManager();
        }

        [TearDown]
        [RequiresPlayMode]
        public void TearDown()
        {
            if (m_TestManager.IsListening)
            {
                m_TestManager.Shutdown();
            }
        }

        [UnityTest]
        [RequiresPlayMode]
        public IEnumerator Client_Initializes()
        {
            ClientGameManager clientManager = new ClientGameManager();
            var awaitClientInitialization = AwaitClientInitializedOrTimeout(clientManager, 6);
            yield return new WaitUntil(()=>awaitClientInitialization.IsCompleted);

            Assert.NotNull(clientManager.User);
            Assert.NotNull(clientManager.NetworkClient);
            Assert.NotNull(clientManager.Matchmaker);
            Assert.AreEqual(AuthState.Authenticated,AuthenticationWrapper.AuthorizationState);
            Debug.Log("Client started with services");

        }

        [UnityTest]
        [RequiresPlayMode]
        public IEnumerator Client_Initialization_Service_Time_Out()
        {
            ClientGameManager  clientManager = new ClientGameManager(false);
            var awaitClientInitialization = AwaitClientInitializedOrTimeout(clientManager, 6);
            yield return new WaitUntil(()=>awaitClientInitialization.IsCompleted);

            Assert.NotNull(clientManager.User);
            Assert.NotNull(clientManager.NetworkClient);
            Assert.NotNull(clientManager.Matchmaker);

            Assert.AreNotEqual(AuthState.Authenticated, AuthenticationWrapper.AuthorizationState);
            Debug.Log("Client started without services");
        }

        async Task AwaitClientInitializedOrTimeout(ClientGameManager manager, int timeOutSeconds)
        {
            await Task.WhenAny(new Task(async () =>
            {
                while (!manager.Initialized)
                {
                    await Task.Delay(100);
                }
            }), Task.Delay(timeOutSeconds * 1000));
        }


    }
}
