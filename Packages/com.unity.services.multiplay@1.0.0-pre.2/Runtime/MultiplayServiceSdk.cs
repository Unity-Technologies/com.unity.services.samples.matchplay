using Unity.Services.Multiplay.Apis.GameServer;
using Unity.Services.Multiplay.Apis.Payload;
using Unity.Services.Wire.Internal;

namespace Unity.Services.Multiplay
{
    /// <summary>
    /// MultiplayService
    /// </summary>
    internal static class MultiplayServiceSdk
    {
        /// <summary>
        /// The static instance of MultiplayService.
        /// </summary>
        public static IMultiplayServiceSdk Instance { get; internal set; }
    }

    /// <summary> Interface for MultiplayService</summary>
    internal interface IMultiplayServiceSdk
    {
        /// <summary> Accessor for GameServerApi methods.</summary>
        IGameServerApiClient GameServerApi { get; set; }

        /// <summary> Accessor for GameServerApi methods.</summary>
        IPayloadApiClient PayloadApi { get; set; }

        IWireDirect WireDirect { get; set; }

        IServerConfigReader ServerConfigReader { get; set; }

        /// <summary> Configuration properties for the service.</summary>
        Configuration Configuration { get; set; }
    }
}
