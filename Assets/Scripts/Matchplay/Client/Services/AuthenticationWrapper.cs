using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Matchplay.Client
{
    public enum AuthResult
    {
        Authenticating,
        Authenticated,
        Error,
        TimedOut
    }

    public static class AuthenticationWrapper
    {
        static bool IsAuthenticated => UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn;

        static AuthResult s_AuthResult = AuthResult.Error;

        public static async void BeginAuth(int tries = 5)
        {
            if (IsAuthenticated)
                return;
            await SignInAnonymouslyAsync(tries);
        }

        //Awaitable task that will pass the clientID once authentication is done.
        public static string ClientId()
        {
            return AuthenticationService.Instance.PlayerId;
        }

        //Awaitable task that will pass once authentication is done.
        public static async Task<AuthResult> Authenticating()
        {
            while (s_AuthResult == AuthResult.Authenticating)
            {
                await Task.Delay(200);
            }

            return s_AuthResult;
        }

        static async Task<AuthResult> SignInAnonymouslyAsync(int maxRetries)
        {
            var tries = 0;
            while (!IsAuthenticated && tries < maxRetries)
            {
                try
                {
                    //To ensure staging login vs nonstaging
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    if (IsAuthenticated)
                    {
                        s_AuthResult = AuthResult.Authenticated;
                        break;
                    }
                }
                catch (AuthenticationException ex)
                {
                    // Compare error code to AuthenticationErrorCodes
                    // Notify the player with the proper error message
                    Debug.LogException(ex);
                    s_AuthResult = AuthResult.Error;
                }
                catch (RequestFailedException exception)
                {
                    // Compare error code to CommonErrorCodes
                    // Notify the player with the proper error message
                    Debug.LogException(exception);
                    s_AuthResult = AuthResult.Error;
                }

                tries++;
                await Task.Delay(1000);
            }

            if (!IsAuthenticated)
            {
                Debug.LogError($"Player was not signed in successfully after {tries} attempts");
                s_AuthResult = AuthResult.TimedOut;
            }

            return s_AuthResult;
        }
    }
}
