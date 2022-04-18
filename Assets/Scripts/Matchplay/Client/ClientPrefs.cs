using UnityEngine;

namespace Matchplay.Client
{
    /// <summary>
    /// Wrapper around the PlayerPrefs system, so we don't have to deal with string keys.
    /// </summary>
    public class ClientPrefs
    {
        public static void SetName(string name)
        {
            PlayerPrefs.SetString("player_name", name);
        }

        public static string PlayerName => PlayerPrefs.GetString("player_name");

        /// <summary>
        /// Either loads a Guid string from Unity preferences, or creates one and checkpoints it, then returns it.
        /// </summary>
        /// <returns>The Guid that uniquely identifies this client install, in string form. </returns>
        public static string GetGuid()
        {
            if (PlayerPrefs.HasKey("client_guid"))
            {
                return PlayerPrefs.GetString("client_guid");
            }

            var guid = System.Guid.NewGuid();
            var guidString = guid.ToString();

            PlayerPrefs.SetString("client_guid", guidString);
            return guidString;
        }
    }
}
