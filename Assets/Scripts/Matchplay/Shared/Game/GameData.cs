using System;
using System.Text;
using UnityEngine;

namespace Matchplay.Shared
{
    [Flags]
    public enum Map
    {
        Lab = 1,
        Space = 2
    }

    [Flags]
    public enum GameMode
    {
        Staring = 1,
        Meditating = 2
    }

    public enum GameQueue
    {
        Casual,
        Competetive
    }

    [Serializable]
    public class MatchplayGameInfo
    {
        public Map map;
        public GameMode gameMode;
        public GameQueue gameQueue;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GameInfo:");
            sb.AppendLine($"- map:        {map}");
            sb.AppendLine($"- gameMode:   {gameMode}");
            sb.AppendLine($"- GameQueue:  {gameQueue}");
            return sb.ToString();
        }
    }

    public class GameData
    {

    }
}
