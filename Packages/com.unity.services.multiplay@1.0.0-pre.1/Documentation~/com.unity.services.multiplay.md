# Documentation for Multiplay SDK Daemon Game Server API
    <a name="documentation-for-api-endpoints"></a>
    ## Documentation for API Endpoints
    All URIs are relative to *http://localhost*
    Class | Method | HTTP request | Description
    ------------ | ------------- | ------------- | -------------
    *GameServerApi* | [**ServerReady**](Apis/GameServerApi.md#serverready) | **POST** /v1/server/{serverId}/ready | Indicates a server is ready to receive allocations
    *GameServerApi* | [**ServerSubscribe**](Apis/GameServerApi.md#serversubscribe) | **GET** /v1/subscribe/{serverId} | Subscribe to game server lifecycle events
    *GameServerApi* | [**ServerUnready**](Apis/GameServerApi.md#serverunready) | **POST** /v1/server/{serverId}/unready | Indicates a server is unready to receive allocations
    
    <a name="documentation-for-models"></a>
    ## Documentation for Models
         - [Models.ErrorResponseBody](Models/ErrorResponseBody.md)
         - [Models.KeyValuePair](Models/KeyValuePair.md)
        
<a name="documentation-for-authorization"></a>
## Documentation for Authorization
    All endpoints do not require authorization.
