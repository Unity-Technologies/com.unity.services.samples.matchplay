using System;
using System.Collections.Generic;
using UnityEngine;

namespace MilehighWorld.CombatSystems
{
    [Serializable]
    public class Metadata
    {
        public string lightingState;
        public string environmentDescription;
        public int nineBitParity;
        public float voidSaturationLevel;
        public string mission;
        public string objective;
        public string location;
        public string initialNarrative;
        public string loreDeepDive;
    }

    [Serializable]
    public class CharacterProfile
    {
        public string name;
        public string role;
        public string archetype;
        public List<string> traits;
        public string motivation;
        public string behaviorScript;
        public List<string> abilities;
    }

    [Serializable]
    public class ObjectInteraction
    {
        public string objectName;
        public Vector3 coordinates;
        public float scaleFactor;
    }

    [Serializable]
    public class Dialogue
    {
        public string speaker;
        public string text;
        public string triggerEvent;
    }

    [Serializable]
    public class SceneScenario
    {
        public string scenarioID;
        public string description;
        public List<ObjectInteraction> interactiveObjects;
        public List<Dialogue> dialogueLines;
    }

    [Serializable]
    public class HorizonGameData
    {
        public Metadata metadata;
        public List<CharacterProfile> characters;
        public List<SceneScenario> scenarios;
    }
}
