using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Matchplay.Client
{
    public enum AuthState
    {
        Initialized,
        Authenticating,
        Authenticated,
        Error,
        TimedOut
    }

    public static class AuthenticationWrapper
    {

        public static AuthState AuthorizationState { get; private set; } = AuthState.Initialized;

        public static async Task<AuthState> DoAuth(int tries = 5)
        {
            if (AuthorizationState == AuthState.Authenticating||AuthorizationState==AuthState.Authenticated)
            {
                Debug.LogWarning("Cant Authenticate if we are authenticating or authenticated");
                return AuthorizationState;
            }

            var signinResult = await SignInAnonymouslyAsync(tries);
            Debug.Log($"Auth attempts Finished : {signinResult.ToString()}");

            return signinResult;
        }

        //Awaitable task that will pass the clientID once authentication is done.
        public static string ClientId()
        {
            return AuthenticationService.Instance.PlayerId;
        }

        //Awaitable task that will pass once authentication is done.
        public static async Task<AuthState> Authenticating()
        {
            while (AuthorizationState == AuthState.Authenticating||AuthorizationState==AuthState.Initialized)
            {
                await Task.Delay(200);
            }
            return AuthorizationState;
        }

        static async Task<AuthState> SignInAnonymouslyAsync(int maxRetries)
        {
            AuthorizationState = AuthState.Authenticating;
            var tries = 0;
            while (AuthorizationState==AuthState.Authenticating && tries < maxRetries)
            {
                try
                {
                    //To ensure staging login vs non staging
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                    if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                    {
                        AuthorizationState = AuthState.Authenticated;
                        break;
                    }
                }
                catch (AuthenticationException ex)
                {
                    // Compare error code to AuthenticationErrorCodes
                    // Notify the player with the proper error message
                    Debug.LogException(ex);
                    AuthorizationState = AuthState.Error;
                }
                catch (RequestFailedException exception)
                {
                    // Compare error code to CommonErrorCodes
                    // Notify the player with the proper error message
                    Debug.LogException(exception);
                    AuthorizationState = AuthState.Error;
                }

                tries++;
                await Task.Delay(1000);
            }

            if (AuthorizationState!=AuthState.Authenticated )
            {
                Debug.LogError($"Player was not signed in successfully after {tries} attempts");
                AuthorizationState = AuthState.TimedOut;
            }

            return AuthorizationState;
        }

        public static void ResetAuthForTests()
        {
            AuthorizationState = AuthState.Initialized;
        }
    }
}
