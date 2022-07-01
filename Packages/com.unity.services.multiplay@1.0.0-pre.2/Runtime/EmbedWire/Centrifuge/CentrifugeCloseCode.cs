namespace Unity.Services.Wire.Internal
{
    /// <summary>
    /// Centrifuge close code.
    /// </summary>
    public enum CentrifugeCloseCode
    {
        // Original websocket-sharp implementation close codes

        /// <summary>
        /// Close code not set
        /// </summary>
        WebsocketNotSet = 0,

        /// <summary>
        /// The connection successfully completed the purpose for which it was created.
        /// </summary>
        WebsocketNormal = 1000,

        /// <summary>
        /// The endpoint is going away, either because of a server failure or because the browser
        /// is navigating away from the page that opened the connection.
        /// </summary>
        WebsocketAway = 1001,

        /// <summary>
        /// The endpoint is terminating the connection due to a protocol error.
        /// </summary>
        WebsocketProtocolError = 1002,

        /// <summary>
        /// The connection is being terminated because the endpoint received data of a type it
        /// cannot accept. (For example, a text-only endpoint received binary data.)
        /// </summary>
        WebsocketUnsupportedData = 1003,

        /// <summary>
        /// Reserved. A meaning might be defined in the future.
        /// </summary>
        WebsocketUndefined = 1004,

        /// <summary>
        /// Reserved. Indicates that no status code was provided even though one was expected.
        /// </summary>
        WebsocketNoStatus = 1005,

        /// <summary>
        /// Reserved. Indicates that a connection was closed abnormally (that is, with no close
        /// frame being sent) when a status code is expected.
        /// </summary>
        WebsocketAbnormal = 1006,

        /// <summary>
        /// The endpoint is terminating the connection because a message was received that
        /// contained inconsistent data (e.g., non-UTF-8 data within a text message).
        /// </summary>
        WebsocketInvalidData = 1007,

        /// <summary>
        /// The endpoint is terminating the connection because it received a message that
        /// violates its policy. This is a generic status code, used when codes 1003 and
        /// 1009 are not suitable.
        /// </summary>
        WebsocketPolicyViolation = 1008,

        /// <summary>
        /// The endpoint is terminating the connection because a data frame was received
        /// that is too large.
        /// </summary>
        WebsocketTooBig = 1009,

        /// <summary>
        /// The client is terminating the connection because it expected the server to
        /// negotiate one or more extension, but the server didn't.
        /// </summary>
        WebsocketMandatoryExtension = 1010,

        /// <summary>
        /// The server is terminating the connection because it encountered an unexpected
        /// condition that prevented it from fulfilling the request.
        /// </summary>
        WebsocketServerError = 1011,

        /// <summary>
        /// Reserved. Indicates that the connection was closed due to a failure to perform
        /// a TLS handshake (e.g., the server certificate can't be verified).
        /// </summary>
        WebsocketTlsHandshakeFailure = 1015,

        // Centrifuge specific close codes

        /// <summary>
        /// is clean disconnect when client cleanly closed connection.
        /// </summary>
        Normal = 3000,

        /// <summary>
        /// sent when node is going to shut down.
        /// </summary>
        Shutdown = 3001,

        /// <summary>
        /// sent when client came with invalid token.
        /// </summary>
        InvalidToken = 3002,

        /// <summary>
        /// sent when client uses malformed protocol frames or wrong order of commands.
        /// </summary>
        BadRequest = 3003,

        /// <summary>
        ///sent when internal error occurred on server.
        /// </summary>
        InternalServerError = 3004,

        /// <summary>
        /// sent when client connection expired.
        /// </summary>
        Expired = 3005,

        /// <summary>
        /// sent when client subscription expired.
        /// </summary>
        SubscriptionExpired = 3006,

        /// <summary>
        /// sent to close connection that did not become authenticated in configured interval after dialing.
        /// </summary>
        Stale = 3007,

        /// <summary>
        /// sent when client can't read messages fast enough.
        /// </summary>
        Slow = 3008,

        /// <summary>
        /// sent when an error occurred while writing to client connection.
        /// </summary>
        WriteError = 3009,

        /// <summary>
        /// sent when server detects wrong client position in channel Publication stream. Disconnect allows client
        /// to restore missed publications on reconnect.
        /// </summary>
        InsufficientState = 3010,

        /// <summary>
        /// sent when server disconnects connection.
        /// </summary>
        ForceReconnect = 3011,

        /// <summary>
        /// sent when server disconnects connection and asks it to not reconnect again.
        /// </summary>
        ForceNoReconnect = 3012,

        /// <summary>
        /// can be sent when client connection exceeds configured connection limit (per user ID or due to other rule).
        /// </summary>
        ConnectionLimit = 3013,

        /// <summary>
        /// can be sent when client connection exceeds configured channel limit.
        /// </summary>
        ChannelLimit = 3014
    }
}
