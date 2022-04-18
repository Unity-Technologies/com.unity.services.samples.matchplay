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
        Timeout //networkClient timed out while connecting
    }

//
//    public struct PlayerConnectionPayload
//    {
//        public string clientAuthID;
//        public string playerName;
//        public GameInfo clientMatchInfo;
//    }

    /// <summary>
    /// Represents a single player on the game server
    /// </summary>

}
