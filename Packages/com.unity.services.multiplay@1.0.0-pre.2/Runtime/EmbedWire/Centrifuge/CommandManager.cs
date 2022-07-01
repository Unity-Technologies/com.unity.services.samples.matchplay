using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Scheduler.Internal;

namespace Unity.Services.Wire.Internal
{
    class CommandManager
    {
        readonly ConcurrentDictionary<uint, TaskCompletionSource<Reply>> m_Commands;
        public Configuration Config;

        readonly IActionScheduler m_ActionScheduler;

        public CommandManager(Configuration configuration, Core.Scheduler.Internal.IActionScheduler actionScheduler)
        {
            m_Commands = new ConcurrentDictionary<uint, TaskCompletionSource<Reply>> {};
            m_ActionScheduler = actionScheduler;
            Config = configuration;
        }

        public void RegisterCommand(UInt32 id)
        {
            var commandTCS = new TaskCompletionSource<Reply>();

            // command timeout
            m_ActionScheduler.ScheduleAction(() =>
            {
                commandTCS?.TrySetCanceled();
            }, Config.CommandTimeoutInSeconds);

            if (!m_Commands.TryAdd(id, commandTCS))
            {
                // false if the command has already been added.
                throw new CommandAlreadyExists(id);
            }
        }

        public async Task<Reply> WaitForCommandAsync(uint id)
        {
            Reply result;
            if (!m_Commands.TryGetValue(id, out var commandCompletionSource))
            {
                throw new CommandNotFoundException(id);
            }
            try
            {
                result = await commandCompletionSource.Task;
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException($"Command {id} timed out.");
            }
            finally
            {
                m_Commands.TryRemove(id, out _);
            }

            return result;
        }

        public void OnDisconnect(Exception exceptionToThrow)
        {
            foreach (var command in m_Commands)
            {
                command.Value.TrySetException(exceptionToThrow);
            }
        }

        public void OnCommandReplyReceived(Reply reply)
        {
            if (m_Commands.TryGetValue(reply.id, out TaskCompletionSource<Reply> commandTCS))
            {
                // We try to set exception or try set result because the command exception might already have been set
                // during socket disconnection.
                if (reply.HasError())
                {
                    // TODO have a wider variety of server side errors handled.
                    commandTCS.TrySetException(CentrifugeErrorToException(reply.error));
                }
                else
                {
                    commandTCS.TrySetResult(reply);
                }
            }
            else
            {
                throw new UnknownCommandReplyException(reply.id);
            }
        }

        Exception CentrifugeErrorToException(CentrifugeError error)
        {
            switch (error.code)
            {
                case CentrifugeErrorCode.ErrorUnauthorized:
                    return new RequestFailedException((int)WireErrorCode.Unauthorized, error.message);
                default:
                    return new RequestFailedException((int)WireErrorCode.Unknown, error.message);
            }
        }
    }
}
