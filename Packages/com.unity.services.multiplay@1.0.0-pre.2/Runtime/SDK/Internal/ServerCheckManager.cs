using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Ucg.Usqp;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    internal class ServerCheckManager : IServerCheckManager
    {
        private UsqpServer m_Server;

        public ushort MaxPlayers { get => m_MaxPlayers; set { m_MaxPlayers = value; UpdateMaxPlayers(value); } }
        public string ServerName { get => m_ServerName; set { m_ServerName = value; UpdateServerName(value); } }
        public string GameType { get => m_GameType; set { m_GameType = value; UpdateGameType(value); } }
        public string BuildId { get => m_BuildId; set { m_BuildId = value; UpdateBuildId(value); } }
        public string Map { get => m_Map; set { m_Map = value; UpdateMap(value); } }
        public ushort Port { get => m_Port; set { m_Port = value; UpdatePort(value); } }
        public ushort CurrentPlayers { get => m_CurrentPlayers; set { m_CurrentPlayers = value; UpdateCurrentPlayers(value); } }

        private bool IsServerInitialized => m_Server != null;

        private ushort m_MaxPlayers;
        private string m_ServerName;
        private string m_GameType;
        private string m_BuildId;
        private string m_Map;
        private ushort m_Port = 0;
        private ushort m_CurrentPlayers = 0;

        private bool isDisposed = false;

        public ServerCheckManager(ushort maxPlayers, string serverName, string gameType, string buildId, string map)
        {
            m_MaxPlayers = maxPlayers;
            m_ServerName = serverName;
            m_GameType = gameType;
            m_BuildId = buildId;
            m_Map = map;
        }

        public void Connect(ushort port)
        {
            m_Port = port;
            InitializeServer();
        }

        public void UpdateServerCheck()
        {
            if (IsServerInitialized)
            {
                m_Server.Update();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }
            if (disposing)
            {
                m_Server.Dispose();
                m_Server = null;
            }
            isDisposed = true;
        }

        private void InitializeServer()
        {
            if (m_Server == null)
            {
                m_Server = new UsqpServer(Port);
            }

            m_Server.ServerInfoData.MaxPlayers = MaxPlayers;
            m_Server.ServerInfoData.ServerName = ServerName;
            m_Server.ServerInfoData.GameType = GameType;
            m_Server.ServerInfoData.BuildId = BuildId;
            m_Server.ServerInfoData.Map = Map;
            m_Server.ServerInfoData.CurrentPlayers = CurrentPlayers;
        }

        private void UpdateMaxPlayers(ushort value)
        {
            if (IsServerInitialized)
            {
                m_Server.ServerInfoData.MaxPlayers = value;
            }
        }

        private void UpdateServerName(string value)
        {
            if (IsServerInitialized)
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                m_Server.ServerInfoData.ServerName = value;
            }
        }

        private void UpdateGameType(string value)
        {
            if (IsServerInitialized)
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                m_Server.ServerInfoData.GameType = value;
            }
        }

        private void UpdateBuildId(string value)
        {
            if (IsServerInitialized)
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                m_Server.ServerInfoData.BuildId = value;
            }
        }

        private void UpdateMap(string value)
        {
            m_Map = value;
            if (IsServerInitialized)
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                m_Server.ServerInfoData.Map = value;
            }
        }

        private void UpdatePort(ushort value)
        {
            if (IsServerInitialized)
            {
                m_Server.ServerInfoData.Port = value;
            }
        }

        private void UpdateCurrentPlayers(ushort value)
        {
            if (IsServerInitialized)
            {
                m_Server.ServerInfoData.CurrentPlayers = value;
            }
        }
    }
}
