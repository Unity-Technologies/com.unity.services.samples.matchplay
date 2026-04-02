using UnityEngine;
using System.Threading.Tasks;

namespace MilehighWorld.CombatSystems
{
    public abstract class CombatEventSO : ScriptableObject
    {
        public abstract Task ExecuteAsync(EncounterDirector director);

        public virtual void RaiseEvent(Vector3 position)
        {
            Debug.Log($"[{name}] Event raised at position {position}");
        }
    }
}
