using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Matchplay.Shared
{
    [Flags]
    public enum Map
    {
        None = 0,
        Lab = 1,
        Space = 2

        //Any = 0 or 3 (None or Both)
    }

    [Flags]
    public enum GameMode
    {
        None = 0,
        Staring = 1,
        Meditating = 2

        //Any = 0 or 3 (None or Both)
    }

    public enum GameQueue
    {
        None,
        Casual,
        Competetive
    }

    /// <summary>
    /// Wrapping the "user" into a class that will callback to listeners when changed, for example, UI.
    /// </summary>
    public class MatchplayUser
    {
        public MatchplayUser()
        {
            Data = new UserData("Player", Guid.NewGuid().ToString(), 0, new GameInfo());
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
            get => Data.userGamePreferences.GetMap();
            set { Data.userGamePreferences.SetMap(value); }
        }

        public GameMode GameModePreferences
        {
            get => Data.userGamePreferences.GetMode();
            set => Data.userGamePreferences.SetMode(value);
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
    /// Subset of information that sets up the gameMap and gameplay
    /// </summary>
    [Serializable]
    public class GameInfo
    {
        //In order to serialize this we can't have properties, hence the cumbersome Get and Set Methods
        //
        [SerializeField]
        Map gameMap;
        [SerializeField]
        int[] mapRules;

        public void SetMap(Map map)
        {
            gameMap = map;
            mapRules = MapRules();//Ensures the Data for the matchmaker is in synch
        }
        public Map GetMap()
        {
            return gameMap;
        }

        [SerializeField]
        GameMode gameMode;
        [SerializeField]
        int[] modeRules;

        public void SetMode(GameMode mode)
        {
            gameMode = mode;
            modeRules = ModeRules();
        }
        public GameMode GetMode()
        {
            return gameMode;
        }

        public GameQueue gameQueue = GameQueue.None;

        //TODO YAGNI if we had different maxPlayers per gameMode i'd expand this to change with the mode type
        public int MaxUsers = 10;
        public string ToScene => ConvertToScene(gameMap);

        //QueueNames in the dashboard can be different than your local queue definitions (If you want nice names for them)
        const string k_MultiplayCasualQueue = "casual-queue";
        const string k_MultiplayCompetetiveQueue = "competetive-queue";
        static readonly Dictionary<string, GameQueue> k_MultiplayToLocalQueueNames = new Dictionary<string, GameQueue>
        {
            { k_MultiplayCasualQueue, GameQueue.Casual },
            { k_MultiplayCompetetiveQueue, GameQueue.Competetive }
        };

        public GameInfo(GameQueue queue = GameQueue.Casual, Map map = Map.Lab, GameMode mode = GameMode.Staring)
        {
            gameQueue = queue;
            SetMap(map);
            SetMode(mode);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GameInfo: ");
            sb.AppendLine($"- gameMap:        {gameMap}");
            sb.AppendLine($"- gameMode:   {gameMode}");
            sb.AppendLine($"- gameQueue:  {gameQueue}");
            return sb.ToString();
        }

        /// <summary>
        /// Convert the gameMap flag enum to a scene name.
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

        public int[] MapRules()
        {
            return FlagsToMatchmakerRules((int)gameMap);
        }

        public int[] ModeRules()
        {
            return FlagsToMatchmakerRules((int)gameMode);
        }

        int[] FlagsToMatchmakerRules(int flagRule)
        {
            if (flagRule == 1)
                return new[] { 1 };
            if (flagRule == 2)
                return new[] { 2 };
            return new[] { 1, 2 };
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
                _ => k_MultiplayCasualQueue
            };
        }

        public static GameQueue ToGameQueue(string multiplayQueue)
        {
            if (!k_MultiplayToLocalQueueNames.ContainsKey(multiplayQueue))
            {
                Debug.LogWarning($"No QueuePreference that maps to {multiplayQueue}");
                return GameQueue.None;
            }

            return k_MultiplayToLocalQueueNames[multiplayQueue];
        }
    }
}