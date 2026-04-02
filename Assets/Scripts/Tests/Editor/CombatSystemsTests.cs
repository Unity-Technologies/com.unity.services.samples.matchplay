using NUnit.Framework;
using UnityEngine;
using MilehighWorld.CombatSystems;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MilehighWorld.Tests
{
    public class CombatSystemsTests
    {
        [Test]
        public void EncounterDirector_CanBeCreated()
        {
            var go = new GameObject("EncounterDirector");
            var director = go.AddComponent<EncounterDirector>();
            Assert.IsNotNull(director);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Characters_CanBeCreated()
        {
            var allyGo = new GameObject("Ally");
            var ally = allyGo.AddComponent<NovomindadCharacter>();
            Assert.IsNotNull(ally);

            var enemyGo = new GameObject("Enemy");
            var enemy = enemyGo.AddComponent<EnemyCharacter>();
            Assert.IsNotNull(enemy);

            Object.DestroyImmediate(allyGo);
            Object.DestroyImmediate(enemyGo);
        }

        [Test]
        public void HorizonGameData_CanBeSerialized()
        {
            var data = new HorizonGameData
            {
                metadata = new Metadata
                {
                    lightingState = "Dim",
                    environmentDescription = "Spooky Spire",
                    nineBitParity = 1,
                    voidSaturationLevel = 0.5f
                },
                characters = new List<CharacterProfile>
                {
                    new CharacterProfile { name = "Sky.ix", role = "Protagonist" }
                },
                scenarios = new List<SceneScenario>
                {
                    new SceneScenario
                    {
                        scenarioID = "SCENARIO_01",
                        description = "Test Scenario",
                        dialogueLines = new List<Dialogue>
                        {
                            new Dialogue { speaker = "Sky.ix", text = "Test Dialogue" }
                        }
                    }
                }
            };

            string json = JsonUtility.ToJson(data);
            Assert.IsFalse(string.IsNullOrEmpty(json));

            var deserializedData = JsonUtility.FromJson<HorizonGameData>(json);
            Assert.AreEqual(data.metadata.environmentDescription, deserializedData.metadata.environmentDescription);
            Assert.AreEqual(data.characters.Count, deserializedData.characters.Count);
            Assert.AreEqual(data.scenarios[0].scenarioID, deserializedData.scenarios[0].scenarioID);
        }

        [Test]
        public void HorizonGameData_CanHandleInteractiveObjects()
        {
            var data = new HorizonGameData
            {
                scenarios = new List<SceneScenario>
                {
                    new SceneScenario
                    {
                        interactiveObjects = new List<ObjectInteraction>
                        {
                            new ObjectInteraction { objectName = "Crystal", coordinates = new Vector3(1, 2, 3), scaleFactor = 1.5f }
                        }
                    }
                }
            };

            string json = JsonUtility.ToJson(data);
            var deserializedData = JsonUtility.FromJson<HorizonGameData>(json);
            Assert.AreEqual("Crystal", deserializedData.scenarios[0].interactiveObjects[0].objectName);
            Assert.AreEqual(1.5f, deserializedData.scenarios[0].interactiveObjects[0].scaleFactor);
            Assert.AreEqual(new Vector3(1, 2, 3), deserializedData.scenarios[0].interactiveObjects[0].coordinates);
        }

        [Test]
        public void EncounterDirector_CanProcessScenarioData()
        {
            var go = new GameObject("EncounterDirector");
            var director = go.AddComponent<EncounterDirector>();

            var allyGo = new GameObject("Sky.ix");
            var ally = allyGo.AddComponent<NovomindadCharacter>();

            var data = new HorizonGameData
            {
                characters = new List<CharacterProfile>
                {
                    new CharacterProfile { name = "Sky.ix", role = "Protagonist" }
                },
                scenarios = new List<SceneScenario>
                {
                    new SceneScenario
                    {
                        scenarioID = "SCENARIO_01",
                        description = "Test Scenario",
                        dialogueLines = new List<Dialogue>
                        {
                            new Dialogue { speaker = "Sky.ix", text = "Test Dialogue" }
                        }
                    }
                }
            };

            // Use reflection to set private field for testing purposes
            var fieldInfo = typeof(EncounterDirector).GetField("sceneData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fieldInfo.SetValue(director, data);

            // Call internal methods to simulate execution
            var initMethod = typeof(EncounterDirector).GetMethod("InitializeEntities", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            initMethod.Invoke(director, null);

            Assert.IsNotNull(ally.profile);
            Assert.AreEqual("Sky.ix", ally.profile.name);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(allyGo);
        }
    }
}
