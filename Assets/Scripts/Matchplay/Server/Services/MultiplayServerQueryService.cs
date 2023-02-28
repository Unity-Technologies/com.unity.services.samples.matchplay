using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Multiplay;
using Debug = UnityEngine.Debug;

namespace Matchplay.Server
{
    public class MultiplayServerQueryService : IDisposable
    {
        IMultiplayService m_MultiplayService;
        IServerQueryHandler m_ServerQueryHandler;
        CancellationTokenSource m_ServerCheckCancel;
        public MultiplayServerQueryService()
        {
            try
            {
                m_MultiplayService = MultiplayService.Instance;
                m_ServerCheckCancel = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error creating Multiplay allocation service.\n{ex}");
            }
        }

        public async Task BeginServerQueryHandler(int maxPlayers, string serverName, string gameType, string map)
        {
            if (m_MultiplayService == null)
                return;

            m_ServerQueryHandler = await m_MultiplayService.StartServerQueryHandlerAsync((ushort)maxPlayers,
                serverName, gameType, "0", map);

#pragma warning disable 4014
            ServerQueryLoop(m_ServerCheckCancel.Token);
#pragma warning restore 4014
        }

        public void SetServerName(string name)
        {
            m_ServerQueryHandler.ServerName = name;
        }

        public void SetBuildID(string id)
        {
            m_ServerQueryHandler.BuildId = id;
        }

        public void SetMaxPlayers(ushort players)
        {
            m_ServerQueryHandler.MaxPlayers = players;
        }

        public void SetPlayerCount(ushort count)
        {
            m_ServerQueryHandler.CurrentPlayers = count;
        }

        public void AddPlayer()
        {
            m_ServerQueryHandler.CurrentPlayers += 1;
        }

        public void RemovePlayer()
        {
            m_ServerQueryHandler.CurrentPlayers -= 1;
        }

        public void SetMap(string newMap)
        {
            m_ServerQueryHandler.Map = newMap;
        }

        public void SetMode(string mode)
        {
            m_ServerQueryHandler.GameType = mode;
        }

        async Task ServerQueryLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Prompt the handler to deal with any incoming request packets.
                // Ensure the delay here is sub 1 second, to ensure that incoming packets are not dropped.
                m_ServerQueryHandler.UpdateServerCheck();
                await Task.Delay(100);
            }
        }

        public void Dispose()
        {
            if (m_ServerCheckCancel != null)
                m_ServerCheckCancel.Cancel();
        }
    }
}