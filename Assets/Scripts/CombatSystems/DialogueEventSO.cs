using UnityEngine;
using System.Threading.Tasks;

namespace MilehighWorld.CombatSystems
{
    [CreateAssetMenu(fileName = "NewDialogueEvent", menuName = "Combat/Events/Dialogue")]
    public class DialogueEventSO : CombatEventSO
    {
        public string speakerName;
        public string text;

        public override Task ExecuteAsync(EncounterDirector director)
        {
            var ally = director.GetAlly(speakerName);
            if (ally != null)
            {
                ally.Speak(text);
            }
            else
            {
                var enemy = director.GetEnemy(speakerName);
                if (enemy != null)
                {
                    enemy.Speak(text);
                }
                else
                {
                    Debug.Log($"[{speakerName}]: {text}");
                }
            }
            return Task.CompletedTask;
        }
    }
}
