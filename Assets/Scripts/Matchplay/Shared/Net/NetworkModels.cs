using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Matchplay.Networking
{
    public enum ConnectStatus
    {
        Undefined,
        Success, //client successfully connected. This may also be a successful reconnect.
        ServerFull, //can't join, server is already at capacity.
        LoggedInAgain, //logged in on a separate client, causing this one to be kicked out.
        UserRequestedDisconnect, //Intentional Disconnect triggered by the user.
        GenericDisconnect, //server disconnected, but no specific reason given.
    }

    [Serializable]
    public class ConnectionPayload
    {
        public string clientGUID;
        public string playerName;
    }

    /// <summary>
    /// Represents a single player on the game server
    /// </summary>
    public struct PlayerData : INetworkSerializable
    {
        public string m_PlayerName; //name of the player
        public ulong m_ClientID; //the identifying id of the client

        public PlayerData(string playerName, ulong clientId)
        {
            m_PlayerName = playerName;
            m_ClientID = clientId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_PlayerName);
            serializer.SerializeValue(ref m_ClientID);
        }
    }
}
