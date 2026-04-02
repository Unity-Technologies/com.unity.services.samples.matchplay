using UnityEngine;

namespace MilehighWorld.CombatSystems
{
    public class NovomindadCharacter : MonoBehaviour
    {
        public CharacterProfile profile;

        public void Speak(string line)
        {
            Debug.Log($"<color=cyan>[{profile?.name ?? name}]: {line}</color>");
        }

        public void UseAbility(string ability)
        {
            Debug.Log($"<color=#00FF00>[{profile?.name ?? name}]: Uses '{ability}'!</color>");
        }
    }
}
