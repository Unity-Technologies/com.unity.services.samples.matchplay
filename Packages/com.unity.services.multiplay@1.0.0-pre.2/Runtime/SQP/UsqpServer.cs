using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Collections;
using UnityEngine;
using Random = System.Random;

namespace Unity.Ucg.Usqp
{
    internal class UsqpServer : IDisposable
    {
        // Settings
        const int k_BufferSize = 1472;
        readonly Socket m_Socket;

        // Re-used data buffer (for optimization)
        NativeArray<byte> m_Buffer = new NativeArray<byte>(k_BufferSize, Allocator.Persistent);
        // TODO(steve): Eliminate duplicate buffer.
        byte[] m_RecvBuffer = new byte[k_BufferSize];

        // dictionary of outstanding tokens
        Dictionary<EndPoint, uint> m_OutstandingTokens = new Dictionary<EndPoint, uint>();
        Random m_Random;
        ServerInfo m_ServerInfo = new ServerInfo();

        // Re-used endpoint (for optimization)
        EndPoint m_RemoteEndpoint = new IPEndPoint(0, 0);

        /// <summary>
        ///     Construct an SQP server using IPAddress.Any (usually 0.0.0.0) and the specified port
        /// </summary>
        public UsqpServer(ushort port)
            : this(new IPEndPoint(IPAddress.Any, port)) {}

        /// <summary>
        ///     Construct an SQP server using the specified IPEndPoint (IP and Port)
        /// </summary>
        public UsqpServer(IPEndPoint endpoint)
        {
            ServerEndpoint = endpoint;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_Socket.SetupAndBind(ServerEndpoint.Address, ServerEndpoint.Port);
            m_Random = new Random();
            ServerInfoData = new ServerInfo.Data();

            Debug.Log($"SQP server: SQP server started on {ServerEndpoint.Address}:{ServerEndpoint.Port}");
        }

        /// <summary>
        ///     The server info data (game, map, etc.) used to construct SQP query responses
        /// </summary>
        public ServerInfo.Data ServerInfoData
        {
            get => m_ServerInfo.ServerInfoData;
            set => m_ServerInfo.ServerInfoData = value;
        }

        /// <summary>
        ///     The endpoint that the SQP server is bound to
        /// </summary>
        public IPEndPoint ServerEndpoint { get; }

        public void Dispose()
        {
            Debug.Log($"SQP server: SQP server at {ServerEndpoint.Address}:{ServerEndpoint.Port} shutting down");
            m_Socket?.Dispose();
            m_Buffer.Dispose();
        }

        /// <summary>
        ///     Process all packets we've received and reply to them
        ///     TODO: There's a possibility for us to spend forever processing packets if the volume is too high; need to add DDoS
        ///     protection
        /// </summary>
        public void Update()
        {
            if (m_Socket.Poll(0, SelectMode.SelectRead))
            {
                var read = 0;

                do
                {
                    try
                    {
                        // TODO: Investigate replacing with ReceiveFromAsync
                        // Also, this will currently throw a SocketError.WouldBlock exception if trying to read past data length
                        read = m_Socket.ReceiveFrom(m_RecvBuffer, m_RecvBuffer.Length, SocketFlags.None,
                            ref m_RemoteEndpoint);
                        if (read > 0)
                            PopPacketAndProcess(read);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        else
                            Debug.LogWarning("SQP server: Socket poll was successful but unable to read data");
#endif
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
                    {
                        // Nothing left to read, exit the loop
                        return;
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode != SocketError.WouldBlock)
                    {
                        throw new SqpException("SQP Server Socket Error", ex);
                    }
                }
                while (read > 0);
            }
        }

        // Process a single packet from the socket
        void PopPacketAndProcess(int read)
        {
            // Copy socket data to native array
            NativeArray<byte>.Copy(m_RecvBuffer, m_Buffer, read);

            var reader = new DataStreamReader(m_Buffer);
            var header = new UsqpHeader();
            header.FromStream(ref reader);

            var type = (UsqpMessageType)header.Type;
            switch (type)
            {
                case UsqpMessageType.ChallengeRequest:
                    SendChallengeResponse();
                    break;

                case UsqpMessageType.QueryRequest:
                    SendQueryResponse(ref reader);
                    break;

                default:
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    Debug.LogWarning("SQP server: Received a non-supported header type: " + type);
#endif
                    break;
            }
        }

        void SendChallengeResponse()
        {
            if (!m_OutstandingTokens.ContainsKey(m_RemoteEndpoint))
            {
                var token = GetNextToken();
                var writer = new DataStreamWriter(m_Buffer);

                try
                {
                    var rsp = new ChallengeResponse();
                    rsp.Header.ChallengeId = token;
                    rsp.ToStream(ref writer);

                    m_Socket.SendTo(m_Buffer.GetSubArray(0, writer.Length).ToArray(), SocketFlags.None, m_RemoteEndpoint);

                    m_OutstandingTokens.Add(m_RemoteEndpoint, token);
                }
                catch (Exception e)
                {
                    throw new SqpException("Failed to send challenge response", e);
                }
            }
        }

        // Send a query response to the remote client
        void SendQueryResponse(ref DataStreamReader reader)
        {
            if (!m_OutstandingTokens.TryGetValue(m_RemoteEndpoint, out var token))
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                Debug.LogWarning("SQP server: Received a query request, but dropped it because the client token was invalid");
#endif
                return;
            }

            var req = new QueryRequest();
            req.FromStream(ref reader, false);

            if ((UsqpChunkType)req.RequestedChunks == UsqpChunkType.ServerInfo)
            {
                var rsp = m_ServerInfo;
                var writer = new DataStreamWriter(m_Buffer);

                try
                {
                    rsp.QueryHeader.Header.ChallengeId = token;
                    rsp.ToStream(ref writer);
                    m_Socket.SendTo(m_Buffer.GetSubArray(0, writer.Length).ToArray(), writer.Length, SocketFlags.None, m_RemoteEndpoint);

                    m_OutstandingTokens.Remove(m_RemoteEndpoint);
                }
                catch (Exception e)
                {
                    throw new SqpException("Failed to send a query response", e);
                }
            }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            else
            {
                Debug.LogWarning($"SQP server: Received a query request from a client, but RequestedChunks was {req.RequestedChunks} not {nameof(UsqpChunkType.ServerInfo)}.");
            }
#endif
        }

        uint GetNextToken()
        {
            var thirtyBits = (uint)m_Random.Next(1 << 30);
            var twoBits = (uint)m_Random.Next(1 << 2);
            return (thirtyBits << 2) | twoBits;
        }
    }
}
