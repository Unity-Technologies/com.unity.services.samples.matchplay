# How to install Multiplay SDK Daemon Game Server API into your Unity project

## Open The package manager

Window > Package Manager

## Install Multiplay SDK Daemon Game Server API

In the Package Manager Click on the "+" sign in the top left corner then select "Add Package From Disk" then browse to
`com.unity.services.multiplay` and select package.json > open

## Setting Up the Scoped Repository to Access Preview Packages
The SDK has a dependency on the Core package, which is currently only available inside the internal Unity scoped repository.
You will need the scoped repository available in your project for the generated package to work.

* In the Unity Editor, open the Package Manager.
* Click on Advanced Project Settings.
* In Packages->Scoped Registries, add a new registry.
    * Name: Internal Unity Registry
    * URL: https://artifactory.prd.it.unity3d.com/artifactory/api/npm/upm-candidates
    * Scopes: com.unity.services
* Click Apply (you will need to be on the VPN).

## Using the Configuration Object
The Unity CSharp generator produces a Configuration class with the following parameters:

| Name      | Type | Description |
| ----------- | ----------- | ----------- |
|basePath| string | The base service path which is overridable. Should be set to a valid URL.|
|requestTimeout| int? | Request timeout for requests in seconds. |
|numRetries | int? | Number of retries for requests.|
|headers| IDictionary<string, string> | Additional headers to add to the request.|

Configuration can be set on three levels: global, api, and request.

###Global Configuration
Global configuration is created automatically. The generated global configuration is:

| Param      | Value |
| ----------- | ----------- |
|basePath| "http://localhost"|
|requestTimeout| 10 |
|numRetries | 4 |
|headers| null |

It can be set and accessed from the service object.

###Api Configuration
Configuration can be set for specific APIs within a package. To set an API-level Configuration, pass it into the constructor:

```
public ApiNameClient(IHttpClient httpClient,
    IAccessToken accessToken,
Configuration configuration = null)
```

When set, the configuration will be merged with the global configuration as follows:

| Param      | Merge or Override? |
| ----------- | ----------- |
|basePath| Overrides global configuration.|
|requestTimeout| Overrides global configuration. |
|numRetries | Overrides global configuration. |
|headers| Overrides headers also defined in the global configuration, then merges list of unique headers. |

###Request Configuration

Configuration can also be set per-request. To use a unique configuration for a request, pass it into the request as follows:

```
public async Task<ResponseReturnType> OperationIdAsync
        (OperationIdRequest request, Configuration requestConfiguration)
```

It will be merged as follows:

| Param      | Merge or Override? |
| ----------- | ----------- |
|basePath| Overrides global and api configuration.|
|requestTimeout| Overrides global and api configuration. |
|numRetries | Overrides global and api configuration. |
|headers| Overrides headers also defined in the global or api configuration, then merges list of unique headers in all three. |

## Using OneOf Types
The Unity CSharp generator supports the keyword OneOf.

When a OneOf schema is used, it will generate a class of type IOneOf, with a Type and an object.

To use the result, cast the object to the given Type.

Example where CheckedBag is the actual type, and PassengerLuggage is the IOneOf type:
```csharp
PassengerLuggage response = await _flightsApiClient.GetPassengerLuggageAsync(request);
if (response.Result.Type == typeof(CheckedBag)
{
    CheckedBag result = (CheckedBag) response.Result.Value;
}
```

This behavior also extends to errors/exceptions e.g.

```csharp
try
{
    PassengerLuggage response = await _flightsApiClient.GetPassengerLuggageAsync(request);
}
catch(HttpException<LuggageTooHeavy> e)
{
    // handle heavyluggage error
}
catch(HttpException<RateLimitError> e)
{
    // handle rate limit error
}
```

You will need to initialize the Core package in your code before using the generated code. (Instructions below.)
## Unity Authentication Support

### To install the Authentication SDK
Since this is an internal package, you will need to use the internal packages registry.

* Open your projects manifest.json file.
* Add the following line to the 'dependencies' section:
```json
"com.unity.services.authentication": "0.7.1-preview"
```

### To use Unity Authentication
To use authentication, you will need to import the package:
```csharp
using Unity.Services.Authentication;
```

Once imported, you will need to log in before using API calls.

Sample Usage:
```csharp
async void Start()
{
    await UnityServices.InitializeAsync();
    await Authentication.SignInAnonymously();
    if (Authentication.IsSignedIn)
    {
        MakeAPICall();
    }
    else
    {
        Debug.Log("Player was not signed in successfully?");
    }

}

async void MakeAPICall()
{
    FakeApiGetRequest r = new FakeApiGetRequest("fakeParameter");
    var response = await MultiplayApiService.FakeClient.FakeApiGetAsync(r);
}
```


