using UnityEngine;
using System.Threading.Tasks;

namespace MilehighWorld.CombatSystems
{
    [CreateAssetMenu(fileName = "NewInteractionEvent", menuName = "Combat/Events/Interaction")]
    public class InteractionEventSO : CombatEventSO
    {
        public string objectName;
        public string action;

        public override Task ExecuteAsync(EncounterDirector director)
        {
            Debug.Log($"<i>Interacting with {objectName}: {action}</i>");
            return Task.CompletedTask;
        }
    }
}
