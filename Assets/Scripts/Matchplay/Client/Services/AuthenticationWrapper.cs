using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Matchplay.Client
{
    public static class AuthenticationWrapper
    {
        static bool IsAuthenticated => UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn;

        public static async void BeginAuth(int tries = 5)
        {
            if (IsAuthenticated)
                return;
            await SignInAnonymouslyAsync(tries);
        }

        //Awaitable task that will pass the clientID once authentication is done.
        public static async Task<string> GetClientId()
        {
            await Authenticating();
            return AuthenticationService.Instance.PlayerId;
        }

        //Awaitable task that will pass once authentication is done.
        public static async Task Authenticating()
        {
            while (!IsAuthenticated)
            {
                await Task.Delay(200);
            }
        }

        static async Task SignInAnonymouslyAsync(int maxRetries)
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
                        break;
                    }
                }
                catch (AuthenticationException ex)
                {
                    // Compare error code to AuthenticationErrorCodes
                    // Notify the player with the proper error message
                    Debug.LogException(ex);
                }
                catch (RequestFailedException exception)
                {
                    // Compare error code to CommonErrorCodes
                    // Notify the player with the proper error message
                    Debug.LogException(exception);
                }

                tries++;
                await Task.Delay(1000);
            }

            if (!IsAuthenticated)
            {
                Debug.LogError($"Player was not signed in successfully after {tries} attempts");
                return;
            }

            Debug.Log("Player signed in as player ID " + AuthenticationService.Instance.PlayerId);
        }
    }
}
