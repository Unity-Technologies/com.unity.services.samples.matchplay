using UnityEngine;
using System.Threading.Tasks;

namespace MilehighWorld.CombatSystems
{
    public abstract class CombatEventSO : ScriptableObject
    {
        public abstract Task ExecuteAsync(EncounterDirector director);
    }
}
