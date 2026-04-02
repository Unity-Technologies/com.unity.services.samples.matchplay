using UnityEngine;

namespace MilehighWorld.CombatSystems
{
    public class EnemyCharacter : MonoBehaviour
    {
        public CharacterProfile profile;

        private const string DialogueColor = "red"; // Red for enemy dialogue
        private const string ReactionColor = "orange"; // Orange for enemy reactions/actions

        public void Speak(string line)
        {
            Debug.Log($"<color={DialogueColor}>[{profile?.name ?? name}]: {line}</color>");
        }

        public void React(string reaction)
        {
            Debug.Log($"<color={ReactionColor}>[{profile?.name ?? name}]: {reaction}</color>");
        }
    }
}
