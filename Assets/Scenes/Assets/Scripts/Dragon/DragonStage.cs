using UnityEngine;

namespace DungeonPrototype.Dragon
{
    public enum DragonStage
    {
        Hatchling = 1,
        Companion = 2,
        Sacred = 3
    }

    [System.Serializable]
    public class DragonStageData
    {
        public DragonStage stage;
        public GameObject modelPrefab;
        public Vector3 scale = Vector3.one;
        [HideInInspector] public GameObject instantiatedModel;
    }
}
