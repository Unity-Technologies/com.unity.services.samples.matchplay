using System;
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
        public Map CurrentMap;
        public GameMode CurrentGameMode;
        public GameQueue CurrentGameQueue;
        public int MaxPlayers;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref CurrentMap);
            serializer.SerializeValue(ref CurrentGameMode);
            serializer.SerializeValue(ref CurrentGameQueue);
            serializer.SerializeValue(ref MaxPlayers);
        }
    }
}
