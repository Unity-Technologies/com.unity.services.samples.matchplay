using System.Threading.Tasks;
using Matchplay.Client;
using Matchplay.Server;
using UnityEngine;

namespace Matchplay.Shared
{
    public class ApplicationController : MonoBehaviour
    {
        //Manager instances to be instantiated.
        [SerializeField]
        ServerSingleton m_ServerPrefab;
        [SerializeField]
        ClientSingleton m_ClientPrefab;

        ApplicationData m_AppData;
        public static bool IsServer;
        async void Start()
        {
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(gameObject);

            //We use EditorApplicationController for Editor launching.
            if (Application.isEditor)
                return;

            //If this is a build and we are headless, we are a server
            await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
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
        async Task LaunchInMode(bool isServer, string profileName = "default")
        {
            //init the command parser, get launch args
            m_AppData = new ApplicationData();
            IsServer = isServer;
            if (isServer)
            {
                var serverSingleton = Instantiate(m_ServerPrefab);
                await serverSingleton.CreateServer(); //run the init instead of relying on start.

                var defaultGameInfo = new GameInfo
                {
                    gameMode = GameMode.Staring,
                    map = Map.Lab,
                    gameQueue = GameQueue.Casual
                };

                await serverSingleton.Manager.StartGameServerAsync(defaultGameInfo);
            }
            else
            {
                var clientSingleton = Instantiate(m_ClientPrefab);
                clientSingleton.CreateClient(profileName);

                //We want to load the main menu while the client is still initializing.
                clientSingleton.Manager.ToMainMenu();
            }
        }
    }
}
