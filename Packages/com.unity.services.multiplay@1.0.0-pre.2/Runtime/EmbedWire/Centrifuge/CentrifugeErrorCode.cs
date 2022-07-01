namespace Unity.Services.Wire.Internal
{
    enum CentrifugeErrorCode : int
    {
        /// <summary>
        /// ErrorInternal means server error, if returned this is a signal
        /// that something went wrong with server itself and client most probably
        /// not responsible.
        /// </summary>
        ErrorInternal = 100,

        /// <summary>
        /// ErrorUnauthorized says that request is unauthorized.
        /// </summary>
        ErrorUnauthorized = 101,

        /// <summary>
        /// ErrorUnknownChannel means that channel name does not exist.
        /// </summary>
        ErrorUnknownChannel = 102,

        /// <summary>
        /// ErrorPermissionDenied means that access to resource not allowed.
        /// </summary>
        ErrorPermissionDenied = 103,

        /// <summary>
        /// ErrorMethodNotFound means that method sent in command does not exist.
        /// </summary>
        ErrorMethodNotFound = 104,

        /// <summary>
        /// ErrorAlreadySubscribed returned when client wants to subscribe on channel
        /// it already subscribed to.
        /// </summary>
        ErrorAlreadySubscribed = 105,

        /// <summary>
        /// ErrorLimitExceeded says that some sort of limit exceeded, server logs should
        /// give more detailed information. See also ErrorTooManyRequests which is more
        /// specific for rate limiting purposes.
        /// </summary>
        ErrorLimitExceeded = 106,

        /// <summary>
        ///  ErrorBadRequest says that server can not process received
        ///  data because it is malformed. Retrying request does not make sense.
        /// </summary>
        ErrorBadRequest = 107,

        /// <summary>
        /// ErrorNotAvailable means that resource is not enabled.
        /// </summary>
        ErrorNotAvailable = 108,

        /// <summary>
        /// ErrorTokenExpired indicates that connection token expired.
        /// </summary>
        ErrorTokenExpired = 109,

        /// <summary>
        /// ErrorExpired indicates that connection expired (no token involved).
        /// </summary>
        ErrorExpired = 110,

        /// <summary>
        /// ErrorTooManyRequests means that server rejected request due to
        /// its rate limiting strategies.
        /// </summary>
        ErrorTooManyRequests = 111,

        /// <summary>
        /// ErrorUnrecoverablePosition means that stream does not contain required
        /// range of publications to fulfill a history query. This can happen due to
        /// expiration, size limitation or due to wrong epoch.
        /// </summary>
        ErrorUnrecoverablePosition = 112,
    }
}
