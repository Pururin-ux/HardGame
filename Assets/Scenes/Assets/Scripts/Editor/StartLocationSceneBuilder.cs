#if UNITY_EDITOR
using System;
using DungeonPrototype.Dragon;
using DungeonPrototype.Environment;
using DungeonPrototype.Mana;
using DungeonPrototype.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace DungeonPrototype.EditorTools
{
    public static class StartLocationSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/DungeonPrototype_Start.unity";

        [MenuItem("Tools/Dungeon Prototype/Create Start Location Scene")]
        public static void CreateStartLocationScene()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "DungeonPrototype_Start";

            EnsureInputSystemEventModule(scene);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "StartGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3.2f, 1f, 3.2f);
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null)
            {
                groundRenderer.sharedMaterial.color = new Color(0.17f, 0.2f, 0.24f, 1f);
            }

            CreatePerimeterWalls();
            CreateLanternPosts();

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1.1f, -10f);
            player.tag = "Player";
            UnityEngine.Object.DestroyImmediate(player.GetComponent<Collider>());

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.center = new Vector3(0f, 0.9f, 0f);
            characterController.radius = 0.35f;

            PlayerHealth playerHealth = player.AddComponent<PlayerHealth>();
            player.AddComponent<PlayerResourceInventory>();
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
            dragon.transform.position = new Vector3(1.3f, 0.6f, -8.4f);
            DragonCompanion dragonCompanion = dragon.AddComponent<DragonCompanion>();

            CreateStartCrystal(new Vector3(-4f, 1f, -7f));
            CreateStartCrystal(new Vector3(4f, 1f, -7f));

            GameObject portalRoot = new GameObject("PortalRoot");
            portalRoot.transform.position = new Vector3(0f, 0f, 10f);
            CreatePortalVisual(portalRoot.transform);

            GameObject portalTrigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            portalTrigger.name = "PortalTrigger";
            portalTrigger.transform.SetParent(portalRoot.transform);
            portalTrigger.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            portalTrigger.transform.localScale = new Vector3(3.6f, 2.6f, 1.2f);

            Collider portalCollider = portalTrigger.GetComponent<Collider>();
            if (portalCollider != null)
            {
                portalCollider.isTrigger = true;
            }

            Renderer portalTriggerRenderer = portalTrigger.GetComponent<Renderer>();
            if (portalTriggerRenderer != null)
            {
                portalTriggerRenderer.enabled = false;
            }

            StartLocationPortal portal = portalTrigger.AddComponent<StartLocationPortal>();

            GameObject systems = new GameObject("SceneSystems");
            Component pauseMenu = TryAddComponentByName(systems, "DungeonPrototype.UI.PauseMenuController");

            SetObjectField(firstPersonController, "cameraPivot", camera.transform);
            SetObjectField(drainInteractor, "dragon", dragonCompanion);
            SetObjectField(drainInteractor, "sourcePoint", player.transform);
            SetIntField(drainInteractor, "crystalMask", ~0);

            if (pauseMenu != null)
            {
                SetObjectField(pauseMenu, "playerController", firstPersonController);
                SetStringField(pauseMenu, "fallbackGameplayScene", "DungeonPrototype_Start");
                SetStringField(pauseMenu, "mainMenuScene", "MainMenu");
            }

            SetStringField(portal, "targetScene", "DungeonPrototype_Prototype");

            EnsureFolder("Assets/Scenes");
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            if (!saved)
            {
                Debug.LogWarning("Start location scene was created but could not be saved automatically.");
            }

            EnsureBuildScenes(ScenePath, "Assets/Scenes/DungeonPrototype_Prototype.unity", "Assets/Scenes/MainMenu.unity");

            Selection.activeGameObject = player;
            EditorGUIUtility.PingObject(player);
            Debug.Log("Start location scene created. Use portal to enter the main gameplay level.");
        }

        private static void CreatePerimeterWalls()
        {
            CreateWall("Wall_North", new Vector3(0f, 2f, 16f), new Vector3(32f, 4f, 1f));
            CreateWall("Wall_South", new Vector3(0f, 2f, -16f), new Vector3(32f, 4f, 1f));
            CreateWall("Wall_West", new Vector3(-16f, 2f, 0f), new Vector3(1f, 4f, 32f));
            CreateWall("Wall_East", new Vector3(16f, 2f, 0f), new Vector3(1f, 4f, 32f));
        }

        private static void CreateLanternPosts()
        {
            CreatePost(new Vector3(-8f, 0f, -2f));
            CreatePost(new Vector3(8f, 0f, -2f));
            CreatePost(new Vector3(-8f, 0f, 6f));
            CreatePost(new Vector3(8f, 0f, 6f));
        }

        private static void CreatePost(Vector3 basePosition)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "LanternPost";
            post.transform.position = basePosition + new Vector3(0f, 1.2f, 0f);
            post.transform.localScale = new Vector3(0.25f, 1.2f, 0.25f);

            GameObject lightObj = new GameObject("LanternLight");
            lightObj.transform.SetParent(post.transform);
            lightObj.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 9f;
            l.intensity = 2.1f;
            l.color = new Color(0.58f, 0.82f, 1f, 1f);
        }

        private static void CreateWall(string name, Vector3 pos, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            Renderer r = wall.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial.color = new Color(0.14f, 0.16f, 0.2f, 1f);
            }
        }

        private static void CreateStartCrystal(Vector3 position)
        {
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crystal.name = "StartCrystal";
            crystal.transform.position = position;
            crystal.transform.localScale = new Vector3(0.65f, 1f, 0.65f);

            ManaCrystal manaCrystal = crystal.AddComponent<ManaCrystal>();
            SetObjectField(manaCrystal, "crystalRenderer", crystal.GetComponent<Renderer>());
            SetFloatField(manaCrystal, "maxMana", 15f);
        }

        private static void CreatePortalVisual(Transform parent)
        {
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "PortalFrame";
            frame.transform.SetParent(parent);
            frame.transform.localPosition = new Vector3(0f, 2f, 0f);
            frame.transform.localScale = new Vector3(4.2f, 4f, 0.6f);

            Renderer frameRenderer = frame.GetComponent<Renderer>();
            if (frameRenderer != null)
            {
                frameRenderer.sharedMaterial.color = new Color(0.12f, 0.18f, 0.28f, 1f);
            }

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Quad);
            core.name = "PortalCore";
            core.transform.SetParent(parent);
            core.transform.localPosition = new Vector3(0f, 2f, -0.31f);
            core.transform.localScale = new Vector3(2.8f, 2.8f, 1f);
            core.transform.localRotation = Quaternion.identity;

            Renderer coreRenderer = core.GetComponent<Renderer>();
            if (coreRenderer != null)
            {
                coreRenderer.sharedMaterial.color = new Color(0.25f, 0.62f, 1f, 0.85f);
            }
        }

        private static void EnsureInputSystemEventModule(Scene scene)
        {
            EventSystem eventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject es = new GameObject("EventSystem", typeof(EventSystem));
                eventSystem = es.GetComponent<EventSystem>();
            }

            Type inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                Component standalone = eventSystem.GetComponent("StandaloneInputModule");
                if (standalone != null)
                {
                    UnityEngine.Object.DestroyImmediate(standalone);
                }

                if (eventSystem.GetComponent(inputSystemModuleType) == null)
                {
                    eventSystem.gameObject.AddComponent(inputSystemModuleType);
                }
            }
            else if (eventSystem.GetComponent("StandaloneInputModule") == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }

            if (!eventSystem.gameObject.scene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(eventSystem.gameObject, scene);
            }
        }

        private static Component TryAddComponentByName(GameObject target, string fullTypeName)
        {
            if (target == null || string.IsNullOrWhiteSpace(fullTypeName))
            {
                return null;
            }

            Type resolved = null;
            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                resolved = assemblies[i].GetType(fullTypeName, false);
                if (resolved != null)
                {
                    break;
                }
            }

            if (resolved == null)
            {
                return null;
            }

            Component existing = target.GetComponent(resolved);
            return existing != null ? existing : target.AddComponent(resolved);
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

        private static void SetStringField(UnityEngine.Object target, string fieldName, string value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                return;
            }

            prop.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void EnsureBuildScenes(params string[] paths)
        {
            var current = EditorBuildSettings.scenes;
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(current);

            for (int i = 0; i < paths.Length; i++)
            {
                string p = paths[i];
                if (!System.IO.File.Exists(p))
                {
                    continue;
                }

                bool exists = false;
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].path == p)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    list.Add(new EditorBuildSettingsScene(p, true));
                }
            }

            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
#endif
