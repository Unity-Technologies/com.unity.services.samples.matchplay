using System.Threading.Tasks;
using Matchplay.Shared.Infrastructure;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Matchplay.Client
{
    public class AuthenticationHandler
    {
        bool m_SignedIn = false;

        [Inject]
        async void InjectAndAuth()
        {
            await UnityServices.InitializeAsync();
            await SignInAnonymouslyAsync();
        }

        public async Task Authenticating()
        {
            while (!m_SignedIn)
            {
                await Task.Delay(500);
            }
        }

        async Task SignInAnonymouslyAsync()
        {
            try
            {
                //To ensure staging login vs nonstaging
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Sign in anonymously succeeded!");

                // Shows how to get the playerID
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
                m_SignedIn = true;
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
        }
    }
}
