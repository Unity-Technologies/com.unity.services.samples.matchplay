using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Matchplay.Networking
{
    public enum NetworkMessage
    {
        ConnectionResult,
        DisconnectionResult,
        ServerChangedMap,
        ServerChangedGameMode,
        ServerChangedQueue
    }

    /// <summary>
    /// Small Wrapper that centralizes the custom network message types that can pass between client and server.
    /// </summary>
    public class MatchplayNetworkMessenger
    {
        public static void SendMessageToAll(NetworkMessage mesageType, FastBufferWriter writer)
        {
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll(mesageType.ToString(), writer);
        }

        public static void SendMessageTo(NetworkMessage messageType, ulong clientID, FastBufferWriter writer)
        {
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(messageType.ToString(), clientID, writer);
        }

        public static void RegisterListener(NetworkMessage messageType, CustomMessagingManager.HandleNamedMessageDelegate listenerMethod)
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(messageType.ToString(), listenerMethod);
        }

        public static void UnRegisterListener(NetworkMessage messageType)
        {
            if (NetworkManager.Singleton == null)
                return;
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(messageType.ToString());
        }
    }
}
