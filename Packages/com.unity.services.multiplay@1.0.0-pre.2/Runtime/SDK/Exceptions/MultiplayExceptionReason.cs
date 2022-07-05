namespace Unity.Services.Multiplay
{
    /// <summary>
    /// Enumerates the known error causes when communicating with the Multiplay Service.
    /// N.B. Error code range for this service: 25000-25999
    /// </summary>
    public enum MultiplayExceptionReason
    {
        #region Multiplay Errors
        /// <summary>
        /// The returned value could not be parsed, such as an error code was not included in the response.
        /// </summary>
        UnknownErrorCode = 0,

        #endregion

        #region Http Errors

        /// <summary>
        /// Error code representing HTTP Status Code of 400 for the Multiplay Service.
        /// The request made was invalid and will not be processed by the service.
        /// </summary>
        BadRequest = 25400,

        /// <summary>
        /// Error code representing HTTP Status Code of 401 for the Multiplay Service.
        /// The request requires authentication.
        /// </summary>
        Unauthorized = 25401,

        /// <summary>
        /// Error code representing HTTP Status Code of 402 for the Multiplay Service.
        /// This error code is reserved for future use.
        /// </summary>
        PaymentRequired = 25402,

        /// <summary>
        /// Error code representing HTTP Status Code of 403 for the Multiplay Service.
        /// The server understood the request, and refuses to fulfill it.
        /// </summary>
        Forbidden = 25403,

        /// <summary>
        /// Error code representing HTTP Status Code of 404 for the Multiplay Service.
        /// The server has not found the specified resource.
        /// </summary>
        EntityNotFound = 25404,

        /// <summary>
        /// Error code representing HTTP Status Code of 405 for the Multiplay Service.
        /// The method specified is not allowed for the specified resource.
        /// </summary>
        MethodNotAllowed = 25405,

        /// <summary>
        /// Error code representing HTTP Status Code of 406 for the Multiplay Service.
        /// The server cannot provide a response that matches the acceptable values for the request.
        /// </summary>
        NotAcceptable = 25406,

        /// <summary>
        /// Error code representing HTTP Status Code of 407 for the Multiplay Service.
        /// The request requires authentication with the proxy.
        /// </summary>
        ProxyAuthenticationRequired = 25407,

        /// <summary>
        /// Error code representing HTTP Status Code of 408 for the Multiplay Service.
        /// The request was not made within the time the server was prepared to wait.
        /// </summary>
        RequestTimeOut = 25408,

        /// <summary>
        /// Error code representing HTTP Status Code of 409 for the Multiplay Service.
        /// The request could not be completed due to a conflict with the current state on the server.
        /// </summary>
        Conflict = 25409,

        /// <summary>
        /// Error code representing HTTP Status Code of 410 for the Multiplay Service.
        /// The requested resource is no longer available and there is no known forwarding address.
        /// </summary>
        Gone = 25410,

        /// <summary>
        /// Error code representing HTTP Status Code of 411 for the Multiplay Service.
        /// The server refuses to accept the request without a defined content-length.
        /// </summary>
        LengthRequired = 25411,

        /// <summary>
        /// Error code representing HTTP Status Code of 412 for the Multiplay Service.
        /// A precondition given in the request was not met when tested on the server.
        /// </summary>
        PreconditionFailed = 25412,

        /// <summary>
        /// Error code representing HTTP Status Code of 413 for the Multiplay Service.
        /// The request entity is larger than the server is willing or able to process.
        /// </summary>
        RequestEntityTooLarge = 25413,

        /// <summary>
        /// Error code representing HTTP Status Code of 414 for the Multiplay Service.
        /// The request URI is longer than the server is willing to interpret.
        /// </summary>
        RequestUriTooLong = 25414,

        /// <summary>
        /// Error code representing HTTP Status Code of 415 for the Multiplay Service.
        /// The request is in a format not supported by the requested resource for the requested method.
        /// </summary>
        UnsupportedMediaType = 25415,

        /// <summary>
        /// Error code representing HTTP Status Code of 416 for the Multiplay Service.
        /// The requested ranges cannot be served.
        /// </summary>
        RangeNotSatisfiable = 25416,

        /// <summary>
        /// Error code representing HTTP Status Code of 417 for the Multiplay Service.
        /// An expectation in the request cannot be met by the server.
        /// </summary>
        ExpectationFailed = 25417,

        /// <summary>
        /// Error code representing HTTP Status Code of 418 for the Multiplay Service.
        /// The server refuses to brew coffee because it is, permanently, a teapot. Defined by the Hyper Text Coffee Pot Control Protocol defined in April Fools' jokes in 1998 and 2014.
        /// </summary>
        Teapot = 25418,

        /// <summary>
        /// Error code representing HTTP Status Code of 421 for the Multiplay Service.
        /// The request was directed to a server that is not able to produce a response.
        /// </summary>
        Misdirected = 25421,

        /// <summary>
        /// Error code representing HTTP Status Code of 422 for the Multiplay Service.
        /// The request is understood, but the server was unable to process its instructions.
        /// </summary>
        UnprocessableTransaction = 25422,

        /// <summary>
        /// Error code representing HTTP Status Code of 423 for the Multiplay Service.
        /// The source or destination resource is locked.
        /// </summary>
        Locked = 25423,

        /// <summary>
        /// Error code representing HTTP Status Code of 424 for the Multiplay Service.
        /// The method could not be performed on the resource because a dependency for the action failed.
        /// </summary>
        FailedDependency = 25424,

        /// <summary>
        /// Error code representing HTTP Status Code of 425 for the Multiplay Service.
        /// The server is unwilling to risk processing a request that may be replayed.
        /// </summary>
        TooEarly = 25425,

        /// <summary>
        /// Error code representing HTTP Status Code of 426 for the Multiplay Service.
        /// The server refuses to perform the request using the current protocol.
        /// </summary>
        UpgradeRequired = 25426,

        /// <summary>
        /// Error code representing HTTP Status Code of 428 for the Multiplay Service.
        /// The server requires the request to be conditional.
        /// </summary>
        PreconditionRequired = 25428,

        /// <summary>
        /// Error code representing HTTP Status Code of 429 for the Multiplay Service.
        /// Too many requests have been sent in a given amount of time.
        /// </summary>
        RateLimited = 25429,

        /// <summary>
        /// Error code representing HTTP Status Code of 431 for the Multiplay Service.
        /// The request has been refused because its HTTP headers are too long.
        /// </summary>
        RequestHeaderFieldsTooLarge = 25431,

        /// <summary>
        /// Error code representing HTTP Status Code of 451 for the Multiplay Service.
        /// The requested resource is not available for legal reasons.
        /// </summary>
        UnavailableForLegalReasons = 25451,

        /// <summary>
        /// Error code representing HTTP Status Code of 500 for the Multiplay Service.
        /// The server encountered an unexpected condition which prevented it from fulfilling the request.
        /// </summary>
        InternalServerError = 25500,

        /// <summary>
        /// Error code representing HTTP Status Code of 501 for the Multiplay Service.
        /// The server does not support the functionality required to fulfil the request.
        /// </summary>
        NotImplemented = 25501,

        /// <summary>
        /// Error code representing HTTP Status Code of 502 for the Multiplay Service.
        /// The server, while acting as a gateway or proxy, received an invalid response from the upstream server.
        /// </summary>
        BadGateway = 25502,

        /// <summary>
        /// Error code representing HTTP Status Code of 503 for the Multiplay Service.
        /// The server is currently unable to handle the request due to a temporary reason.
        /// </summary>
        ServiceUnavailable = 25503,

        /// <summary>
        /// Error code representing HTTP Status Code of 504 for the Multiplay Service.
        /// The server, while acting as a gateway or proxy, did not get a response in time from the upstream server that it needed in order to complete the request.
        /// </summary>
        GatewayTimeout = 25504,

        /// <summary>
        /// Error code representing HTTP Status Code of 505 for the Multiplay Service.
        /// The server does not support the HTTP protocol that was used in the request.
        /// </summary>
        HttpVersionNotSupported = 25505,

        /// <summary>
        /// Error code representing HTTP Status Code of 506 for the Multiplay Service.
        /// The server has an internal configuration error: the chosen variant resource is configured to engage in transparent content negotiation itself, and is therefore not a proper end point in the negotiation process.
        /// </summary>
        VariantAlsoNegotiates = 25506,

        /// <summary>
        /// Error code representing HTTP Status Code of 507 for the Multiplay Service.
        /// The server has insufficient storage space to complete the request.
        /// </summary>
        InsufficientStorage = 25507,

        /// <summary>
        /// Error code representing HTTP Status Code of 508 for the Multiplay Service.
        /// The server terminated the request because it encountered an infinite loop.
        /// </summary>
        LoopDetected = 25508,

        /// <summary>
        /// Error code representing HTTP Status Code of 510 for the Multiplay Service.
        /// The policy for accessing the resource has not been met in the request.
        /// </summary>
        NotExtended = 25510,

        /// <summary>
        /// Error code representing HTTP Status Code of 511 for the Multiplay Service.
        /// The request requires authentication for network access.
        /// </summary>
        NetworkAuthenticationRequired = 25511,

        /// <summary>
        /// Error code representing a ServerEvent error for the Multiplay Service.
        /// You are already subscribed to this lobby and have attempted to subscribe to it again.
        /// </summary>
        AlreadySubscribedToLobby = 25601,

        /// <summary>
        /// Error code representing a ServerEvent error for the Multiplay Service.
        /// You are already unsubscribed from this lobby and have attempted to unsubscribe from it again.
        /// </summary>
        AlreadyUnsubscribedFromLobby = 25602,

        /// <summary>
        /// Error code representing a ServerEvent error for the Multiplay Service.
        /// The connection was lost or dropped while attempting to do something with the connection such as subscribe or unsubscribe.
        /// </summary>
        SubscriptionToLobbyLostWhileBusy = 25603,

        /// <summary>
        /// Error code representing a ServerEvent error for the Multiplay Service.
        /// Something went wrong when trying to connect to the lobby service. Ensure a valid Lobby ID was sent.
        /// </summary>
        LobbyEventServiceConnectionError = 25604,

        #endregion

        /// <summary>
        /// NetworkError is returned when the UnityWebRequest failed with this flag set. See the exception stack trace when this reason is provided for context.
        /// </summary>
        NetworkError = 25998,

        /// <summary>
        /// Unknown is returned when a unrecognized error code is returned by the service. Check the inner exception to get more information.
        /// </summary>
        Unknown = 25999
    }
}
