using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Services.Multiplay
{
    internal static class MultiplayServiceLogging
    {
        public static void Verbose(string message)
        {
#if ENABLE_UNITY_MULTIPLAY_VERBOSE_LOGGING
            Debug.Log(message);
#endif
        }
    }
}
