using System;
using System.Collections;
using System.Threading.Tasks;
using Matchplay.Server;
using Matchplay.Shared;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Matchplay.Tests
{
    public class ServerTests
    {
        NetworkManager m_TestManager;

        const string k_LocalIP = "127.0.0.1";
        const int k_DefaultPort = 7777;
        const int k_DefaultQPort = 7787;

        [OneTimeSetUp]
        [RequiresPlayMode]
        public void OneTimeSetup()
        {
            ApplicationData.IsServerUnitTest = true;
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
        public IEnumerator Create_Local_Server_Staring_Lab_Casual()
        {
            var startingGameInfo = new GameInfo
            {
                gameMode = GameMode.Staring,
                map = Map.Lab,
                gameQueue = GameQueue.Casual
            };

            var createServerTask = CreateServerAsync(k_LocalIP, k_DefaultPort, k_DefaultQPort,
                startingGameInfo);

            yield return new WaitUntil(() => createServerTask.IsCompleted);
            var createdServer = createServerTask.Result;
            Assert.AreEqual(SceneManager.GetActiveScene(), SceneManager.GetSceneByName(startingGameInfo.ToSceneName));
            Assert.NotNull(createdServer.ServerData);
            Assert.AreEqual(startingGameInfo.gameMode, createdServer.ServerData.gameMode.Value);
            Assert.AreEqual(startingGameInfo.map, createdServer.ServerData.map.Value);
            Assert.AreEqual(startingGameInfo.gameQueue, createdServer.ServerData.gameQueue.Value);
            Assert.IsFalse(createdServer.StartedServices);
        }

        async Task<ServerGameManager> CreateServerAsync(string ip, int port, int qport, GameInfo gameInfo)
        {
            var serverGameManager = new ServerGameManager(ip, port, qport, NetworkManager.Singleton);
            await serverGameManager.StartGameServerAsync(gameInfo);
            return serverGameManager;
        }
    }
}