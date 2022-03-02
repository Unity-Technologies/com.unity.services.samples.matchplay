using System;
using System.Text;
using Unity.Netcode;
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

    public struct MatchplayGameInfo : INetworkSerializable
    {
        public Map map;
        public GameMode gameMode;
        public GameQueue gameQueue;
        public int maxPlayers;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref map);
            serializer.SerializeValue(ref gameMode);
            serializer.SerializeValue(ref gameQueue);
            serializer.SerializeValue(ref maxPlayers);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("GameInfo:");
            sb.AppendLine($"- map:        {map}");
            sb.AppendLine($"- gameMode:   {gameMode}");
            sb.AppendLine($"- GameQueue:  {gameQueue}");
            sb.AppendLine($"- maxPlayers: {maxPlayers}");
            return sb.ToString();
        }
    }
}
