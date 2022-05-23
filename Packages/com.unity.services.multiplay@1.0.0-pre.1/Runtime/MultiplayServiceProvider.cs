using UnityEngine;
using System.Threading.Tasks;

using Unity.Services.Multiplay.Apis.GameServer;

using Unity.Services.Multiplay.Http;
using Unity.Services.Core.Internal;
using Unity.Services.Authentication.Internal;
using Unity.Services.Wire.Internal;

namespace Unity.Services.Multiplay
{
    internal class MultiplayServiceProvider : IInitializablePackage
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            // Pass an instance of this class to Core
            var generatedPackageRegistry =
                CoreRegistry.Instance.RegisterPackage(new MultiplayServiceProvider());
            // And specify what components it requires, or provides.
            generatedPackageRegistry.DependsOn<IWireDirect>();
            generatedPackageRegistry.OptionallyDependsOn<IAccessToken>();
        }

        public Task Initialize(CoreRegistry registry)
        {
            var httpClient = new HttpClient();

            var accessTokenMultiplay = registry.GetServiceComponent<IAccessToken>();
            var wireDirect = registry.GetServiceComponent<IWireDirect>();

            MultiplayServiceSdk.Instance = new InternalMultiplayService(httpClient, wireDirect, accessTokenMultiplay);

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// InternalMultiplayService
    /// </summary>
    internal class InternalMultiplayService : IMultiplayServiceSdk
    {
        /// <summary>
        /// Constructor for InternalMultiplayService
        /// </summary>
        /// <param name="httpClient">The HttpClient for InternalMultiplayService.</param>
        /// <param name="accessToken">The Authentication token for the service.</param>
        public InternalMultiplayService(HttpClient httpClient, IWireDirect wireDirect, IAccessToken accessToken = null)
        {
            GameServerApi = new GameServerApiClient(httpClient, accessToken);
            WireDirect = wireDirect;
            ServerConfigReader = new ServerConfigReader();
            Configuration = new Configuration("http://127.0.0.1:8086", 10, 4, null);
        }

        /// <summary> Instance of IGameServerApiClient interface</summary>
        public IGameServerApiClient GameServerApi { get; set; }

        public IWireDirect WireDirect { get; set; }

        public IServerConfigReader ServerConfigReader { get; set; }

        /// <summary> Configuration properties for the service.</summary>
        public Configuration Configuration { get; set; }
    }
}
