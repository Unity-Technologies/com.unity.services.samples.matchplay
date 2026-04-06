using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MilehighWorld.CombatSystems
{
    /// <summary>
    /// Replaces the monolithic GeneratedScenario.cs to process data-driven events.
    /// </summary>
    public class EncounterDirector : MonoBehaviour
    {
        [Header("Encounter Timeline")]
        [SerializeField] private List<CombatEventSO> sequenceEvents;

        [Header("Scene Data")]
        [SerializeField] private HorizonGameData sceneData;

        // Entity Registries
        private Dictionary<string, NovomindadCharacter> _novomindad = new Dictionary<string, NovomindadCharacter>();
        private Dictionary<string, EnemyCharacter> _enemies = new Dictionary<string, EnemyCharacter>();

        private async void Start()
        {
            InitializeEntities();
            await ProcessEncounterAsync();
        }

        private void InitializeEntities()
        {
            // Register characters from the scene
            var allies = FindObjectsByType<NovomindadCharacter>(FindObjectsSortMode.None);
            foreach (var ally in allies)
            {
                _novomindad[ally.name] = ally;
                if (sceneData != null && sceneData.characters != null)
                {
                    ally.profile = sceneData.characters.Find(c => c.name == ally.name);
                }
            }

            var enemies = FindObjectsByType<EnemyCharacter>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                _enemies[enemy.name] = enemy;
                if (sceneData != null && sceneData.characters != null)
                {
                    enemy.profile = sceneData.characters.Find(c => c.name == enemy.name);
                }
            }
        }

        private async Task ProcessEncounterAsync()
        {
            Debug.Log("<color=#E0BBE4>--- SCENARIO SEQUENCE COMMENCING ---</color>");

            if (sceneData != null && sceneData.metadata != null)
            {
                Debug.Log($"<color=white><b>Mission:</b> {sceneData.metadata.mission}</color>");
                Debug.Log($"<color=white><b>Objective:</b> {sceneData.metadata.objective}</color>");
                Debug.Log($"<color=white><b>Location:</b> {sceneData.metadata.location}</color>");
                Debug.Log($"<color=white><b>Initial Narrative:</b>\n{sceneData.metadata.initialNarrative}</color>");
                Debug.Log($"Environment: {sceneData.metadata.environmentDescription} (Saturation: {sceneData.metadata.voidSaturationLevel})");
            }

            // Process static timeline events
            foreach (var combatEvent in sequenceEvents)
            {
                if (combatEvent == null) continue;
                await combatEvent.ExecuteAsync(this);
            }

            // Process data-driven scenario lines if available
            if (sceneData != null && sceneData.scenarios != null)
            {
                foreach (var scenario in sceneData.scenarios)
                {
                    Debug.Log($"<color=white><b>Scenario:</b> {scenario.description}</color>");

                    // Process interactive objects
                    foreach (var interaction in scenario.interactiveObjects)
                    {
                        Debug.Log($"<i>Interacting with {interaction.objectName} at {interaction.coordinates} (Scale: {interaction.scaleFactor})</i>");
                    }

                    // Process dialogue lines
                    foreach (var dialogue in scenario.dialogueLines)
                    {
                        var ally = GetAlly(dialogue.speaker);
                        if (ally != null)
                        {
                            ally.Speak(dialogue.text);
                        }
                        else
                        {
                            var enemy = GetEnemy(dialogue.speaker);
                            if (enemy != null)
                            {
                                enemy.Speak(dialogue.text);
                            }
                            else
                            {
                                Debug.Log($"[{dialogue.speaker}]: {dialogue.text}");
                            }
                        }
                    }
                }
            }

            if (sceneData != null && sceneData.metadata != null && !string.IsNullOrEmpty(sceneData.metadata.loreDeepDive))
            {
                Debug.Log($"<color=white><b>Lore Deep Dive:</b>\n{sceneData.metadata.loreDeepDive}</color>");
            }

            Debug.Log("<color=#E0BBE4>--- SCENARIO SEQUENCE CONCLUDED ---</color>");
        }

        public NovomindadCharacter GetAlly(string id) => _novomindad.TryGetValue(id, out var character) ? character : null;
        public EnemyCharacter GetEnemy(string id) => _enemies.TryGetValue(id, out var enemy) ? enemy : null;
    }
}
