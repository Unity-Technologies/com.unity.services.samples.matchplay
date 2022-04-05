using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Matchplay.Shared
{
    [Flags]
    public enum Map
    {
        None = 0,
        Lab = 1,
        Space = 2
    }

    [Flags]
    public enum GameMode
    {
        None = 0,
        Staring = 1,
        Meditating = 2
    }

    public enum GameQueue
    {
        Casual,
        Competetive,
        Missing
    }

    /// <summary>
    /// Wrapping the userData into a class that will callback to listeners when changed, for example, UI.
    /// </summary>
    public class ObservableUser
    {
        public ObservableUser()
        {
            Data = new UserData("player", "", 0, new GameInfo());
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
            set
            {
                Data.userAuthId = value;
                onAuthChanged?.Invoke(Data.userAuthId);
            }
        }

        public Action<string> onAuthChanged;

        public ulong SetNetworkID
        {
            get => Data.networkId;
            set => Data.networkId = value;
        }

        public Map MapPreferences
        {
            get => Data.userGamePreferences.map;
            set
            {
                Data.userGamePreferences.map = value;
                onMapPreferencesChanged?.Invoke(Data.userGamePreferences.map);
            }
        }

        public Action<Map> onMapPreferencesChanged;

        public GameMode GameModePreferences
        {
            get => Data.userGamePreferences.gameMode;
            set
            {
                Data.userGamePreferences.gameMode = value;
                onModePreferencesChanged?.Invoke(Data.userGamePreferences.gameMode);
            }
        }

        public Action<GameMode> onModePreferencesChanged;

        public GameQueue QueuePreference
        {
            get => Data.userGamePreferences.gameQueue;
            set
            {
                Data.userGamePreferences.gameQueue = value;
                onQueuePreferenceChanged?.Invoke(Data.userGamePreferences.gameQueue);
            }
        }

        public Action<GameQueue> onQueuePreferenceChanged;

        public override string ToString()
        {
            var userData = new StringBuilder("ObservableUser: ");
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
        [FormerlySerializedAs("clientAuthId")] public string userAuthId; //Auth Player ID
        public ulong networkId;
        [FormerlySerializedAs("gamePreferences")] [FormerlySerializedAs("modePreferences")] public GameInfo userGamePreferences; //The game info the player thought he was joining with

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
            sb.AppendLine($"- User Name:       {userName}");
            sb.AppendLine($"- User Auth Id:   {userAuthId}");
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
        public Map map = Map.None;
        public GameMode gameMode = GameMode.None;
        public GameQueue gameQueue = GameQueue.Missing;

        //QueueNames in the dashboard can be different than your local queue definitions (If you want nice names for them)
        const string k_MultiplayCasualQueue = "casual-queue";
        const string k_MultiplayCompetetiveQueue = "competetive-queue";
        static readonly Dictionary<string, GameQueue> k_MultiplayToLocalQueueNames = new Dictionary<string, GameQueue>
        {
            { k_MultiplayCasualQueue, GameQueue.Casual },
            { k_MultiplayCompetetiveQueue, GameQueue.Competetive }
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
        /// Convert queue enums to ticket queue name
        /// (Same as your queue name in the matchmaker dashboard)
        /// </summary>
        public string ToMultiplayQueue()
        {
            return gameQueue switch
            {
                GameQueue.Casual => k_MultiplayCasualQueue,
                GameQueue.Competetive => k_MultiplayCompetetiveQueue,
                _ => "casual-queue"
            };
        }

        public static GameQueue ToGameQueue(string multiplayQueue)
        {
            if (!k_MultiplayToLocalQueueNames.ContainsKey(multiplayQueue))
            {
                Debug.LogWarning($"No QueuePreference that maps to {multiplayQueue}");
                return GameQueue.Missing;
            }

            return k_MultiplayToLocalQueueNames[multiplayQueue];
        }
    }
}
