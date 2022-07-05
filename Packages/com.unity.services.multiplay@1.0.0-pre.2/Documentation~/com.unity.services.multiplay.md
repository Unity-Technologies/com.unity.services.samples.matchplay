# Documentation for Multiplay SDK Daemon Game Server API
    <a name="documentation-for-api-endpoints"></a>
    ## Documentation for API Endpoints
    All URIs are relative to *http://localhost*
    Class | Method | HTTP request | Description
    ------------ | ------------- | ------------- | -------------
    *GameServerApi* | [**ReadyServer**](Apis/GameServerApi.md#readyserver) | **POST** /v1/server/{serverId}/allocation/{allocationId}/ready-for-players | Indicates a server is ready to receive players
    *GameServerApi* | [**SubscribeServer**](Apis/GameServerApi.md#subscribeserver) | **GET** /v1/subscribe/{serverId} | Subscribe to game server lifecycle events
    *GameServerApi* | [**UnreadyServer**](Apis/GameServerApi.md#unreadyserver) | **POST** /v1/server/{serverId}/unready | Indicates a server is not ready to receive allocations
    *PayloadApi* | [**PayloadAllocation**](Apis/PayloadApi.md#payloadallocation) | **GET** /payload/{allocationId} | Retrieve an allocation's payload
    *PayloadApi* | [**PayloadToken**](Apis/PayloadApi.md#payloadtoken) | **GET** /token | Retrieve a JWT token for payloads
    
    <a name="documentation-for-models"></a>
    ## Documentation for Models
         - [Models.ErrorResponseBody](Models/ErrorResponseBody.md)
         - [Models.PayloadAllocationErrorResponseBody](Models/PayloadAllocationErrorResponseBody.md)
         - [Models.PayloadTokenResponseBody](Models/PayloadTokenResponseBody.md)
        
<a name="documentation-for-authorization"></a>
## Documentation for Authorization
    All endpoints do not require authorization.
