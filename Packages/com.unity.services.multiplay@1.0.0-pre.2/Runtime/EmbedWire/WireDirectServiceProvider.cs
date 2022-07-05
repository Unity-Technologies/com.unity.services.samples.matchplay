using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Services.Authentication.Internal;
using Unity.Services.Core.Internal;
using Unity.Services.Core.Scheduler.Internal;
using Unity.Services.Core.Threading.Internal;
using Unity.Services.Core.Telemetry.Internal;
using UnityEngine;

namespace Unity.Services.Wire.Internal
{
    class WireDirectServiceProvider : IInitializablePackage
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            // Pass an instance of this class to Core
            var generatedPackageRegistry =
                CoreRegistry.Instance.RegisterPackage(new WireDirectServiceProvider());
            // And specify what components it requires, or provides.
            generatedPackageRegistry
                .DependsOn<IActionScheduler>()
                .DependsOn<IUnityThreadUtils>()
                .DependsOn<IMetricsFactory>()
                .OptionallyDependsOn<IAccessToken>()
                .ProvidesComponent<IWireDirect>();
        }

        public Task Initialize(CoreRegistry registry)
        {
            var actionScheduler = registry.GetServiceComponent<IActionScheduler>();
            var threadUtils = registry.GetServiceComponent<IUnityThreadUtils>();
            registry.RegisterServiceComponent<IWireDirect>(new WireDirect(actionScheduler, null, threadUtils, null));
            return Task.CompletedTask;
        }
    }
}
