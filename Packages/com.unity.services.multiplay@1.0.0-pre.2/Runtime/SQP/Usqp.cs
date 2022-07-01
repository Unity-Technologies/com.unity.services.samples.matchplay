using System;
using System.Text;
using UnityEngine;

namespace Unity.Ucg.Usqp
{
    [Flags]
    internal enum UsqpChunkType
    {
        ServerInfo = 1,
        ServerRules = 2,
        PlayerInfo = 4,
        TeamInfo = 8
    }

    internal enum UsqpMessageType : byte
    {
        ChallengeRequest = 0,
        ChallengeResponse = 0,
        QueryRequest = 1,
        QueryResponse = 1
    }

    internal interface IUsqpMessage
    {
        void ToStream(ref DataStreamWriter writer);
        void FromStream(ref DataStreamReader reader);
    }

    internal struct UsqpHeader : IUsqpMessage
    {
        public byte Type { get; internal set; }
        public uint ChallengeId;

        public void ToStream(ref DataStreamWriter writer)
        {
            writer.WriteByte(Type);
            writer.WriteUIntNetworkByteOrder(ChallengeId);
        }

        public void FromStream(ref DataStreamReader reader)
        {
            Type = reader.ReadByte();
            ChallengeId = reader.ReadUIntNetworkByteOrder();
        }
    }

    internal struct ChallengeRequest : IUsqpMessage
    {
        public UsqpHeader Header;

        public void ToStream(ref DataStreamWriter writer)
        {
            Header.Type = (byte)UsqpMessageType.ChallengeRequest;
            Header.ToStream(ref writer);
        }

        public void FromStream(ref DataStreamReader reader)
        {
            Header.FromStream(ref reader);
        }
    }

    internal struct ChallengeResponse
    {
        public UsqpHeader Header;

        public void ToStream(ref DataStreamWriter writer)
        {
            Header.Type = (byte)UsqpMessageType.ChallengeResponse;
            Header.ToStream(ref writer);
        }

        public void FromStream(ref DataStreamReader reader)
        {
            Header.FromStream(ref reader);
        }
    }

    internal struct QueryRequest
    {
        public UsqpHeader Header;
        public ushort Version;

        public byte RequestedChunks;

        public void ToStream(ref DataStreamWriter writer)
        {
            Header.Type = (byte)UsqpMessageType.QueryRequest;
            Header.ToStream(ref writer);
            writer.WriteUShortNetworkByteOrder(Version);
            writer.WriteByte(RequestedChunks);
        }

        public void FromStream(ref DataStreamReader reader, bool header = true)
        {
            if (header)
                Header.FromStream(ref reader);

            Version = reader.ReadUShortNetworkByteOrder();
            RequestedChunks = reader.ReadByte();
        }
    }

    internal struct QueryResponseHeader
    {
        public UsqpHeader Header;
        public ushort Version;
        public byte CurrentPacket;
        public byte LastPacket;
        public ushort Length;
        private DataStreamWriter m_LengthWriter;

        public void ToStream(ref DataStreamWriter writer)
        {
            Header.Type = (byte)UsqpMessageType.QueryResponse;
            Header.ToStream(ref writer);
            writer.WriteUShortNetworkByteOrder(Version);
            writer.WriteByte(CurrentPacket);
            writer.WriteByte(LastPacket);
            m_LengthWriter = writer; // Remember DataStreamWriter position to write length.
            writer.WriteUShortNetworkByteOrder(Length); // Neads rewriting once the length is known.
        }

        public void UpdateLength(ushort length)
        {
            Length = length;
            m_LengthWriter.WriteUShortNetworkByteOrder(length);
        }

        public void FromStream(ref DataStreamReader reader, bool header = true)
        {
            if (header)
                Header.FromStream(ref reader);
            Version = reader.ReadUShortNetworkByteOrder();
            CurrentPacket = reader.ReadByte();
            LastPacket = reader.ReadByte();
            Length = reader.ReadUShortNetworkByteOrder();
        }
    }

    internal class ServerInfo
    {
        static Encoding s_Encoding = new UTF8Encoding();
        static Encoder s_Encoder;

        public uint ChunkLen;
        public QueryResponseHeader QueryHeader;
        public Data ServerInfoData;

        public ServerInfo()
        {
            ServerInfoData = new Data();
        }

        public void ToStream(ref DataStreamWriter writer)
        {
            QueryHeader.ToStream(ref writer);

            var start = (ushort)writer.Length;
            var chunkWriter = writer; // Remember DataStreamWriter position to write length.
            writer.WriteUIntNetworkByteOrder(ChunkLen); // Neads rewriting once the length is known.

            var chunkStart = (uint)writer.Length;
            ServerInfoData.ToStream(ref writer);
            ChunkLen = (uint)writer.Length - chunkStart;

            // Now we know the request length and the chunk length rewrite them in place.
            QueryHeader.UpdateLength((ushort)(writer.Length - start));
            chunkWriter.WriteUIntNetworkByteOrder(ChunkLen);
        }

        public void FromStream(ref DataStreamReader reader, bool header = true)
        {
            QueryHeader.FromStream(ref reader, header);
            ChunkLen = reader.ReadUIntNetworkByteOrder();

            ServerInfoData.FromStream(ref reader);
        }

        [Serializable]
        public class Data
        {
            public string BuildId = "";
            public ushort CurrentPlayers;
            public string GameType = "";
            public string Map = "";
            public ushort MaxPlayers;
            public ushort Port;
            public string ServerName = "";

            static unsafe void WriteString(ref DataStreamWriter writer, string value)
            {
                s_Encoder = s_Encoder ?? s_Encoding.GetEncoder();
                var buffer = new byte[byte.MaxValue];
                var chars = value.ToCharArray();

                s_Encoder.Convert(chars, 0, chars.Length, buffer, 0, byte.MaxValue, true, out var charsUsed, out var bytesUsed, out var completed);
                Debug.Assert(bytesUsed <= byte.MaxValue);

                writer.WriteByte((byte)bytesUsed);

                fixed(byte* bufferPtr = &buffer[0])
                {
                    writer.WriteBytes(bufferPtr, bytesUsed);
                }
            }

            static unsafe string ReadString(ref DataStreamReader reader)
            {
                var length = reader.ReadByte();
                var buffer = new byte[length];

                fixed(byte* bufferPtr = &buffer[0])
                {
                    reader.ReadBytes(bufferPtr, length);
                }

                return s_Encoding.GetString(buffer, 0, length);
            }

            public void ToStream(ref DataStreamWriter writer)
            {
                writer.WriteUShortNetworkByteOrder(CurrentPlayers);
                writer.WriteUShortNetworkByteOrder(MaxPlayers);

                WriteString(ref writer, ServerName);
                WriteString(ref writer, GameType);
                WriteString(ref writer, BuildId);
                WriteString(ref writer, Map);

                writer.WriteUShortNetworkByteOrder(Port);
            }

            public void FromStream(ref DataStreamReader reader)
            {
                CurrentPlayers = reader.ReadUShortNetworkByteOrder();
                MaxPlayers = reader.ReadUShortNetworkByteOrder();

                ServerName = ReadString(ref reader);
                GameType = ReadString(ref reader);
                BuildId = ReadString(ref reader);
                Map = ReadString(ref reader);

                Port = reader.ReadUShortNetworkByteOrder();
            }
        }
    }
}
