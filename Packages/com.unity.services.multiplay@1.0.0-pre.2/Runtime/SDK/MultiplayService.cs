using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Services.Multiplay.Internal;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    /// <summary>
    /// Here is the first point and call for accessing Multiplay Package's features!
    /// Use the .Instance method to get a singleton of the IMultiplayService, and from there you can make various requests to the Multiplay service API.
    /// </summary>
    public static class MultiplayService
    {
        private static IMultiplayService m_ServiceSingleton;

        /// <summary>
        /// Provides the Multiplay Service interface for making service API requests.
        /// </summary>
        public static IMultiplayService Instance
        {
            get
            {
                if (m_ServiceSingleton == null)
                {
                    InitializeWrappedMultiplayService();
                }
                return m_ServiceSingleton;
            }
        }

        private static void InitializeWrappedMultiplayService()
        {
            var service = MultiplayServiceSdk.Instance;
            if (service == null)
            {
                throw new InvalidOperationException($"Unable to get {nameof(IMultiplayService)} because Multiplay API is not initialized. Make sure you call UnityServices.InitializeAsync();");
            }
            m_ServiceSingleton = new WrappedMultiplayService(service);
        }
    }
}
