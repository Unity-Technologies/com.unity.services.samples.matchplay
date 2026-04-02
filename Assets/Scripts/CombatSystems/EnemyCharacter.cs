using UnityEngine;

namespace MilehighWorld.CombatSystems
{
    public class EnemyCharacter : MonoBehaviour
    {
        public CharacterProfile profile;

        public void Speak(string line)
        {
            Debug.Log($"<color=red>[{profile?.name ?? name}]: {line}</color>");
        }

        public void React(string reaction)
        {
            Debug.Log($"<color=orange>[{profile?.name ?? name}]: {reaction}</color>");
        }
    }
}
