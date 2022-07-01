using System;
using Unity.Services.Core;

namespace Unity.Services.Wire.Internal
{
    class NoChannelPublicationException : Exception
    {
        public NoChannelPublicationException(string originalData)
            : base($"can't parse publication's channel: {originalData}")
        {
        }
    }

    /// <summary>
    /// This exception is thrown when attempting to re-subscribe to a subscription that has already been subscribed to.
    /// </summary>
    public class AlreadySubscribedException : RequestFailedException
    {
        /// <inheritdoc cref="AlreadySubscribedException"/>
        public AlreadySubscribedException(string alias)
            : base((int)WireErrorCode.AlreadySubscribed, $"Already subscribed to {alias}.")
        {
        }
    }

    /// <summary>
    /// This exception is thrown when attempting to unsubscribe from a subscription that has already been unsubscribed.
    /// </summary>
    public class AlreadyUnsubscribedException : RequestFailedException
    {
        /// <inheritdoc cref="AlreadyUnsubscribedException"/>
        public AlreadyUnsubscribedException(string alias)
            : base((int)WireErrorCode.AlreadyUnsubscribed, $"Already unsubscribed from {alias}")
        {
        }
    }

    class UnknownCommandReplyException : Exception
    {
        public UnknownCommandReplyException(UInt32 id) :
            base($"Received a command reply with unknown id: {id}")
        {
        }
    }

    /// <summary>
    /// This exception is thrown when the connection is lost or dropped during a command execution (i.e. subscription, unsubscription, etc.).
    /// </summary>
    public class CommandInterruptedException : RequestFailedException
    {
        /// <inheritdoc cref="CommandInterruptedException"/>
        public CommandInterruptedException(string reason, CentrifugeCloseCode code) : base((int)WireErrorCode.CommandFailed, $"Command interrupted, reason: {reason}")
        {
            m_Code = code;
        }

        /// <summary>
        /// The close code explaining the reason why the Wire connection was interrupted in the middle of a Command.
        /// <see cref="CentrifugeCloseCode"/>
        /// </summary>
        public CentrifugeCloseCode m_Code { get; private set; }
    }

    class CommandNotFoundException : Exception
    {
        public CommandNotFoundException(UInt32 id) : base($"Command not found (id: {id})")
        {
        }
    }

    class CommandAlreadyExists : Exception
    {
        public CommandAlreadyExists(UInt32 id) : base($"Command already exists (id: {id})")
        {
        }
    }

    /// <summary>
    /// This exception is thrown when the channel provided by the <see cref="IChannelTokenProvider"/> is null or empty.
    /// </summary>
    public class EmptyChannelException : RequestFailedException
    {
        /// <inheritdoc cref="EmptyChannelException"/>
        public EmptyChannelException() : base((int)WireErrorCode.InvalidChannelName, "The channel provided by the token provider is empty or null.") {}
    }

    /// <summary>
    /// This exception is thrown when the token provided by the <see cref="IChannelTokenProvider"/> is null or empty.
    /// </summary>
    public class EmptyTokenException : RequestFailedException
    {
        /// <inheritdoc cref="EmptyTokenException"/>
        public EmptyTokenException() : base((int)WireErrorCode.InvalidToken, "The token provided by the token provider is empty or null.") {}
    }

    /// <summary>
    /// This exception is thrown when a same <see cref="IChannelTokenProvider"/> provides a channel that is inconsistant with the previous calls to <see cref="IChannelTokenProvider.GetTokenAsync"/>.
    /// </summary>
    public class ChannelChangedException : RequestFailedException
    {
        /// <inheritdoc cref="ChannelChangedException"/>
        public ChannelChangedException(string newAlias, string oldAlias)
            : base((int)WireErrorCode.InvalidChannelName, $"The token retriever is not consistent, the alias has changed: {oldAlias}->{newAlias}.")
        {
        }
    }

    /// <summary>
    /// An error occured during an attempt to connect to the Wire service.
    /// </summary>
    public class ConnectionFailedException : RequestFailedException
    {
        /// <inheritdoc cref="ConnectionFailedException"/>
        public ConnectionFailedException(string reason) : base((int)WireErrorCode.ConnectionFailed, $"Connection failed: {reason}.") {}
    }
}
