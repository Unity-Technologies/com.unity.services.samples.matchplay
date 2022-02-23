using Unity.Netcode;
using UnityEngine;

namespace Matchplay
{
    public enum NetworkMessage
    {
        ConnectionResult,
        DisconnectionResult
    }

    /// <summary>
    /// Small Wrapper that centralizes the custom network message types that can pass between client and server.
    /// </summary>
    public class MatchplayNetworkMessenger
    {
        public static void SendMessage(NetworkMessage messageType, ulong clientID, FastBufferWriter writer)
        {
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(messageType.ToString(), clientID, writer);
        }

        public static void RegisterListener(NetworkMessage messageType, CustomMessagingManager.HandleNamedMessageDelegate listenerMethod)
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(messageType.ToString(), listenerMethod);
        }

        public static void UnRegisterListener(NetworkMessage messageType)
        {
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(messageType.ToString());
        }
    }
}
