using System;
using System.Collections.Generic;
using System.Text;
using Matchplay.Shared.Tools;
using UnityEngine;

namespace Matchplay.Shared
{
    public enum Map
    {
        Lab,
        Space
    }

    public enum GameMode
    {
        Staring,
        Meditating
    }

    public enum GameQueue
    {
        Casual,
        Competitive
    }

    /// <summary>
    /// Wrapping the "user" into a class that will callback to listeners when changed, for example, UI.
    /// </summary>
    public class MatchplayUser
    {
        public MatchplayUser()
        {
            var tempId = Guid.NewGuid().ToString();
            Data = new UserData(NameGenerator.GetName(tempId), tempId, 0, new GameInfo());
        }

        public UserData Data { get; }

        public string Name
        {
            get => Data.userName;
            set
            {
                Data.userName = value;
                onNameChanged?.Invoke(Data.userName);
            }
        }

        public Action<string> onNameChanged;

        public string AuthId
        {
            get => Data.userAuthId;
            set => Data.userAuthId = value;
        }

        public Map MapPreferences
        {
            get => Data.userGamePreferences.map;
            set { Data.userGamePreferences.map = value; }
        }

        public GameMode GameModePreferences
        {
            get => Data.userGamePreferences.gameMode;
            set => Data.userGamePreferences.gameMode = value;
        }

        public GameQueue QueuePreference
        {
            get => Data.userGamePreferences.gameQueue;
            set => Data.userGamePreferences.gameQueue = value;
        }

        public override string ToString()
        {
            var userData = new StringBuilder("MatchplayUser: ");
            userData.AppendLine($"- {Data}");
            return userData.ToString();
        }
    }

    /// <summary>
    /// All the data we would want to pass across the network.
    /// </summary>
    [Serializable]
    public class UserData
    {
        public string userName; //name of the player
        public string userAuthId; //Auth Player ID
        public ulong networkId;
        public GameInfo userGamePreferences; //The game info the player thought he was joining with

        public UserData(string userName, string userAuthId, ulong networkId, GameInfo userGamePreferences)
        {
            this.userName = userName;
            this.userAuthId = userAuthId;
            this.networkId = networkId;
            this.userGamePreferences = userGamePreferences;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("UserData: ");
            sb.AppendLine($"- User Name:             {userName}");
            sb.AppendLine($"- User Auth Id:          {userAuthId}");
            sb.AppendLine($"- User Game Preferences: {userGamePreferences}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Subset of information that sets up the map and gameplay
    /// </summary>
    [Serializable]
    public class GameInfo
    {
        public Map map;
        public GameMode gameMode;
        public GameQueue gameQueue;

        //TODO YAGNI if we had different maxPlayers per gameMode i'd expand this to change with the mode type
        public int MaxUsers = 10;
        public string ToSceneName => ConvertToScene(map);

        //QueueNames in the dashboard can be different than your local queue definitions (If you want nice names for them)
        const string k_MultiplayCasualQueue = "casual-queue";
        const string k_MultiplayCompetetiveQueue = "competetive-queue";
        static readonly Dictionary<string, GameQueue> k_MultiplayToLocalQueueNames = new Dictionary<string, GameQueue>
        {
            { k_MultiplayCasualQueue, GameQueue.Casual },
            { k_MultiplayCompetetiveQueue, GameQueue.Competitive }
        };

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GameInfo: ");
            sb.AppendLine($"- map:        {map}");
            sb.AppendLine($"- gameMode:   {gameMode}");
            sb.AppendLine($"- gameQueue:  {gameQueue}");
            return sb.ToString();
        }

        /// <summary>
        /// Convert the map flag enum to a scene name.
        /// </summary>
        public static string ConvertToScene(Map map)
        {
            switch (map)
            {
                case Map.Lab:
                    return "game_lab";
                case Map.Space:
                    return "game_space";
                default:
                    Debug.LogWarning($"{map} - is not supported.");
                    return "";
            }
        }

        /// <summary>
        /// Convert queue enums to ticket queue name
        /// (Same as your queue name in the matchmaker dashboard)
        /// </summary>
        public string ToMultiplayQueue()
        {
            return gameQueue switch
            {
                GameQueue.Casual => k_MultiplayCasualQueue,
                GameQueue.Competitive => k_MultiplayCompetetiveQueue,
                _ => k_MultiplayCasualQueue
            };
        }

        public static GameQueue ToGameQueue(string multiplayQueue)
        {
            if (!k_MultiplayToLocalQueueNames.ContainsKey(multiplayQueue))
            {
                Debug.LogWarning($"No QueuePreference that maps to {multiplayQueue}");
                return GameQueue.Casual;
            }

            return k_MultiplayToLocalQueueNames[multiplayQueue];
        }
    }
}