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
        static bool IsAuthenticated => UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn;

        static AuthState s_AuthState = AuthState.Initialized;

        public static async Task BeginAuth(int tries = 5)
        {
            if (IsAuthenticated)
                return;
            var signinResult = await SignInAnonymouslyAsync(tries);
            Debug.Log($"Auth attempts Finished : {signinResult.ToString()}");
        }

        //Awaitable task that will pass the clientID once authentication is done.
        public static string ClientId()
        {
            return AuthenticationService.Instance.PlayerId;
        }

        //Awaitable task that will pass once authentication is done.
        public static async Task<AuthState> Authenticating()
        {
            while (s_AuthState == AuthState.Authenticating||s_AuthState==AuthState.Initialized)
            {
                await Task.Delay(200);
            }

            return s_AuthState;
        }

        static async Task<AuthState> SignInAnonymouslyAsync(int maxRetries)
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
                        s_AuthState = AuthState.Authenticated;
                        break;
                    }
                }
                catch (AuthenticationException ex)
                {
                    // Compare error code to AuthenticationErrorCodes
                    // Notify the player with the proper error message
                    Debug.LogException(ex);
                    s_AuthState = AuthState.Error;
                }
                catch (RequestFailedException exception)
                {
                    // Compare error code to CommonErrorCodes
                    // Notify the player with the proper error message
                    Debug.LogException(exception);
                    s_AuthState = AuthState.Error;
                }

                tries++;
                await Task.Delay(1000);
            }

            if (!IsAuthenticated)
            {
                Debug.LogError($"Player was not signed in successfully after {tries} attempts");
                s_AuthState = AuthState.TimedOut;
            }

            return s_AuthState;
        }
    }
}
