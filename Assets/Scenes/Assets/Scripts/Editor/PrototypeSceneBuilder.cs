#if UNITY_EDITOR
using System;
using DungeonPrototype.Dragon;
using DungeonPrototype.Environment;
using DungeonPrototype.Guardians;
using DungeonPrototype.Mana;
using DungeonPrototype.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace DungeonPrototype.EditorTools
{
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/DungeonPrototype_Prototype.unity";

        [MenuItem("Tools/Dungeon Prototype/Create New Gameplay Scene")]
        public static void CreateNewGameplayScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "DungeonPrototype_Prototype";

            int crystalLayer = EnsureLayerExists("Crystal", 8);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3f, 1f, 3f);
            TryAddNavMeshSurfaceAndBuild(ground);

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1.1f, -8f);
            player.tag = "Player";
            UnityEngine.Object.DestroyImmediate(player.GetComponent<Collider>());

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.center = new Vector3(0f, 0.9f, 0f);
            characterController.radius = 0.35f;

            PlayerHealth playerHealth = player.AddComponent<PlayerHealth>();
            PlayerResourceInventory inventory = player.AddComponent<PlayerResourceInventory>();
            ManaDrainInteractor drainInteractor = player.AddComponent<ManaDrainInteractor>();
            SimpleFirstPersonController firstPersonController = player.AddComponent<SimpleFirstPersonController>();

            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camera = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }

            camera.transform.SetParent(player.transform);
            camera.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            camera.transform.localRotation = Quaternion.identity;

            GameObject dragon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dragon.name = "Dragon";
            dragon.transform.position = new Vector3(1.5f, 0.6f, -6f);
            DragonCompanion dragonCompanion = dragon.AddComponent<DragonCompanion>();

            GameObject systems = new GameObject("SceneSystems");
            DragonHungerSystem hungerSystem = systems.AddComponent<DragonHungerSystem>();
            systems.AddComponent<CrystalDrainAlarmRelay>();
            GuardianDeathRewardRelay rewardRelay = systems.AddComponent<GuardianDeathRewardRelay>();

            GameObject gateRoot = new GameObject("GateRoot");
            gateRoot.transform.position = new Vector3(0f, 0f, 7f);
            GateController gateController = gateRoot.AddComponent<GateController>();

            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.SetParent(gateRoot.transform);
            door.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            door.transform.localScale = new Vector3(3f, 3f, 0.4f);

            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "PressurePlate";
            plate.transform.position = new Vector3(0f, 0.08f, 4.5f);
            plate.transform.localScale = new Vector3(2f, 0.15f, 2f);
            BoxCollider plateCollider = plate.GetComponent<BoxCollider>();
            plateCollider.isTrigger = true;
            PressurePlateGate plateGate = plate.AddComponent<PressurePlateGate>();

            GameObject guardian = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            guardian.name = "Guardian";
            guardian.transform.position = new Vector3(5f, 1f, 2f);
            NavMeshAgent navMeshAgent = guardian.AddComponent<NavMeshAgent>();
            navMeshAgent.stoppingDistance = 1.2f;
            GuardianController guardianController = guardian.AddComponent<GuardianController>();

            CreateCrystal("Crystal_A", new Vector3(-2f, 1f, -2f), crystalLayer);
            CreateCrystal("Crystal_B", new Vector3(2.5f, 1f, -0.5f), crystalLayer);
            CreateCrystal("Crystal_C", new Vector3(-1f, 1f, 2f), crystalLayer);

            SetObjectField(firstPersonController, "cameraPivot", camera.transform);
            SetObjectField(drainInteractor, "dragon", dragonCompanion);
            SetObjectField(drainInteractor, "sourcePoint", player.transform);
            SetIntField(drainInteractor, "crystalMask", crystalLayer >= 0 ? 1 << crystalLayer : ~0);

            SetObjectField(hungerSystem, "dragon", dragonCompanion);
            SetObjectField(hungerSystem, "playerHealth", playerHealth);

            SetObjectField(rewardRelay, "dragon", dragonCompanion);
            SetObjectField(rewardRelay, "inventory", inventory);

            SetObjectField(gateController, "gateMesh", door.transform);
            SetObjectField(gateController, "slotRenderers", new[] { door.GetComponent<Renderer>() });

            SetObjectField(plateGate, "gate", gateController);
            SetObjectField(plateGate, "runeRenderer", plate.GetComponent<Renderer>());
            SetFloatField(plateGate, "requiredManaWeight", 20f);

            SetObjectField(guardianController, "player", player.transform);
            SetObjectField(guardianController, "dragon", dragonCompanion);

            string folder = "Assets/Scenes";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            if (!saved)
            {
                Debug.LogWarning("Scene was created but could not be saved automatically.");
            }

            Selection.activeGameObject = player;
            EditorGUIUtility.PingObject(player);
            Debug.Log("Prototype scene created with Crystal layer and NavMeshSurface auto-setup when package is available.");
        }

        private static void CreateCrystal(string name, Vector3 position, int crystalLayer)
        {
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crystal.name = name;
            crystal.transform.position = position;
            crystal.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
            if (crystalLayer >= 0)
            {
                crystal.layer = crystalLayer;
            }

            ManaCrystal manaCrystal = crystal.AddComponent<ManaCrystal>();
            SetObjectField(manaCrystal, "crystalRenderer", crystal.GetComponent<Renderer>());
        }

        private static int EnsureLayerExists(string layerName, int preferredIndex)
        {
            int existing = LayerMask.NameToLayer(layerName);
            if (existing >= 0)
            {
                return existing;
            }

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            if (layersProp == null)
            {
                return -1;
            }

            if (preferredIndex >= 8 && preferredIndex < layersProp.arraySize)
            {
                SerializedProperty preferred = layersProp.GetArrayElementAtIndex(preferredIndex);
                if (string.IsNullOrEmpty(preferred.stringValue))
                {
                    preferred.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return preferredIndex;
                }
            }

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty slot = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(slot.stringValue))
                {
                    slot.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return i;
                }
            }

            return -1;
        }

        private static void TryAddNavMeshSurfaceAndBuild(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            Type navMeshSurfaceType = Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
            if (navMeshSurfaceType == null)
            {
                navMeshSurfaceType = Type.GetType("NavMeshSurface, Assembly-CSharp");
            }

            if (navMeshSurfaceType == null)
            {
                Debug.LogWarning("AI Navigation package not found. Add NavMeshSurface manually and click Bake.");
                return;
            }

            Component surface = target.GetComponent(navMeshSurfaceType) ?? target.AddComponent(navMeshSurfaceType);
            if (surface == null)
            {
                return;
            }

            var buildMethod = navMeshSurfaceType.GetMethod("BuildNavMesh", Type.EmptyTypes);
            if (buildMethod != null)
            {
                buildMethod.Invoke(surface, null);
            }
        }

        private static void SetObjectField(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectField(UnityEngine.Object target, string fieldName, UnityEngine.Object[] values)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null || !prop.isArray)
            {
                return;
            }

            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloatField(UnityEngine.Object target, string fieldName, float value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                return;
            }

            prop.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetIntField(UnityEngine.Object target, string fieldName, int value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                return;
            }

            prop.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
