using UnityEngine;
#if UNITY_EDITOR
using System;
using ParrelSync;

#endif

namespace Matchplay.Shared.Tools
{
    public static class ProfileManager
    {
        static string s_Profile;

        public static string Profile => s_Profile ??= GetProfile();

        static string GetProfile()
        {
#if UNITY_EDITOR

            //The code below makes it possible for the clone instance to log in as a different user profile in Authentication service.
            //This allows us to test services integration locally by utilising Parrelsync.
            if (ClonesManager.IsClone())
            {
                var customArguments = ClonesManager.GetArgument().Split(',');
                var cloneSuffix = ClonesManager.CloneNameSuffix;
                //second argument is our custom ID, but if it's not set we would just use some default.

                var hardcodedProfileID = customArguments.Length > 1 ? customArguments[1] : $"clone_{cloneSuffix}";

                return hardcodedProfileID;
            }
#endif
            return "";
        }
    }
}
