using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Matchplay.Client
{
    public class AuthenticationHandler
    {
        int m_MaxRetries = 5;

        public async Task BeginAuth(int tries = 5)
        {
            m_MaxRetries = tries;
            await UnityServices.InitializeAsync();
            await SignInAnonymouslyAsync();
        }

        public async Task Authenticating()
        {
            while (!IsAuthenticated)
            {
                await Task.Delay(200);
            }
        }

        public bool IsAuthenticated => UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn;

        async Task SignInAnonymouslyAsync()
        {
            var tries = 0;
            while (!IsAuthenticated && tries < m_MaxRetries)
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
