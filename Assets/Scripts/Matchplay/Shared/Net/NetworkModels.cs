using System;
using System.Collections;
using System.Collections.Generic;
using Matchplay.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

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
        Timeout //Client timed out while connecting
    }

    [Serializable]
    public class ConnectionPayload
    {
        public string clientGUID;
        public string playerName;
        public MatchplayGameInfo clientMatchInfo;
    }

    /// <summary>
    /// Represents a single player on the game server
    /// </summary>
    public struct UserData
    {
        public string playerName; //name of the player
        public ulong clientId; //the identifying id of the client
        public MatchplayGameInfo playerGameInfo;//The game info the player thought he was joining with

        public UserData(string playerName, ulong clientId, MatchplayGameInfo gameInfo)
        {
            this.playerName = playerName;
            this.clientId = clientId;
            playerGameInfo = gameInfo;

        }


    }
}
