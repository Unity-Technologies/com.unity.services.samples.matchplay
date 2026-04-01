using UnityEngine;
using System.Collections.Generic;

namespace MilehighWorld.CombatSystems
{
    public class NovomindadCharacter : MonoBehaviour
    {
        public CharacterProfile profile;

        private const string AbilityColor = "#00FF00"; // Green for actions
        private const string DialogueColor = "cyan"; // Cyan for dialogue
        private const string WarningColor = "yellow"; // Yellow for warnings

        public void Speak(string line)
        {
            Debug.Log($"<color={DialogueColor}>[{profile?.name ?? name}]: {line}</color>");
        }

        public void UseAbility(string ability)
        {
            if (profile != null && profile.abilities != null && profile.abilities.Contains(ability))
            {
                Debug.Log($"<color={AbilityColor}>[{profile.name}]: Uses '{ability}'!</color>");
            }
            else
            {
                Debug.LogWarning($"<color={WarningColor}>[{profile?.name ?? name}]: Tries to use '{ability}' but does not possess this ability.</color>");
            }
        }
    }
}
