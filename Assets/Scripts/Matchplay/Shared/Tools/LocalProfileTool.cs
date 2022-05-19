using UnityEngine;
#if UNITY_EDITOR
using System;
using ParrelSync;

#endif

namespace Matchplay.Shared.Tools
{
    public static class LocalProfileTool
    {
        static string s_LocalProfileSuffix;

        public static string LocalProfileSuffix => s_LocalProfileSuffix ??= CreateParrelSyncSuffix();

        static string CreateParrelSyncSuffix()
        {
#if UNITY_EDITOR

            //The code below makes it possible for the clone instance to log in as a different user profile in Authentication service.
            //This allows us to test services integration locally by utilising Parrelsync.
            if (ClonesManager.IsClone())
            {
                var cloneSuffix = ClonesManager.CloneNameSuffix;
                //second argument is our custom ID, but if it's not set we would just use some default.

                var hardcodedProfileID = $"_clone_{cloneSuffix}";

                return hardcodedProfileID;
            }
#endif
            return "";
        }
    }
}
