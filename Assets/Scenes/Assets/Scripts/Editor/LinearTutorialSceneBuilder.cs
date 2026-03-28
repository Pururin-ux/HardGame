#if UNITY_EDITOR
using System;
using DungeonPrototype.Dragon;
using DungeonPrototype.Environment;
using DungeonPrototype.Gameplay;
using DungeonPrototype.Guardians;
using DungeonPrototype.Mana;
using DungeonPrototype.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DungeonPrototype.EditorTools
{
    public static class LinearTutorialSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/DungeonPrototype_Prototype.unity";

        [MenuItem("Tools/Dungeon Prototype/Build Linear Tutorial Level")]
        public static void BuildLinearTutorialLevel()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "DungeonPrototype_Prototype";

            ConfigureLighting();
            EnsureInputSystemEventModule(scene);

            GameObject levelRoot = new GameObject("LevelRoot");
            GameObject uiRoot = CreateTutorialUi();
            GameObject gameManager = new GameObject("GameManager");

            // Room 1: exposition.
            GameObject room1 = new GameObject("Room1_Exposition");
            room1.transform.SetParent(levelRoot.transform);
            CreateFloor(room1.transform, "Room1_Floor", new Vector3(0f, -0.1f, 0f), new Vector3(15f, 0.2f, 15f));
            CreateFloor(room1.transform, "Room1_Ceiling", new Vector3(0f, 4.2f, 0f), new Vector3(15f, 0.2f, 15f));
            CreateRoom1WallsWithHole(room1.transform);
            CreateTorch(room1.transform, "Room1_Torch_A", new Vector3(-6.5f, 1.8f, 6.4f));
            CreateTorch(room1.transform, "Room1_Torch_B", new Vector3(-6.5f, 1.8f, -6.4f));

            // Player and dragon in room center.
            GameObject player = CreatePlayer(new Vector3(0f, 1.1f, 0f));
            player.transform.SetParent(room1.transform);
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            ManaDrainInteractor drainInteractor = player.GetComponent<ManaDrainInteractor>();
            SimpleFirstPersonController firstPerson = player.GetComponent<SimpleFirstPersonController>();

            GameObject dragon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dragon.name = "Dragon";
            dragon.transform.SetParent(room1.transform);
            dragon.transform.position = new Vector3(1.5f, 0.6f, 0f);
            dragon.transform.localScale = Vector3.one * 0.5f;
            DragonCompanion dragonCompanion = dragon.AddComponent<DragonCompanion>();
            SetVector3Field(dragonCompanion, "hatchlingScale", Vector3.one * 0.5f);

            GameObject gateRoot = new GameObject("MainGateRoot");
            gateRoot.transform.SetParent(room1.transform);
            gateRoot.transform.position = new Vector3(0f, 0f, 6.6f);
            GateController gateController = gateRoot.AddComponent<GateController>();

            GameObject mainGate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainGate.name = "MainGate_Cube";
            mainGate.transform.SetParent(gateRoot.transform);
            mainGate.transform.localPosition = new Vector3(0f, 2f, 0f);
            mainGate.transform.localScale = new Vector3(5f, 4f, 0.6f);

            GameObject altarPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            altarPlate.name = "AltarPlate";
            altarPlate.transform.SetParent(room1.transform);
            altarPlate.transform.position = new Vector3(0f, 0.08f, 4.8f);
            altarPlate.transform.localScale = new Vector3(4.5f, 0.15f, 4.5f);
            BoxCollider altarCollider = altarPlate.GetComponent<BoxCollider>();
            altarCollider.isTrigger = true;
            PressurePlateGate altarGate = altarPlate.AddComponent<PressurePlateGate>();

            SetObjectField(gateController, "gateMesh", mainGate.transform);
            SetObjectField(gateController, "slotRenderers", new[] { mainGate.GetComponent<Renderer>() });
            SetObjectField(altarGate, "gate", gateController);
            SetObjectField(altarGate, "runeRenderer", altarPlate.GetComponent<Renderer>());
            SetFloatField(altarGate, "requiredManaWeight", 5f);

            // Room 2: safe tutorial corridor.
            GameObject room2 = new GameObject("Room2_SafeTraining");
            room2.transform.SetParent(levelRoot.transform);
            CreateFloor(room2.transform, "Room2_Floor", new Vector3(14f, -0.1f, 0f), new Vector3(12f, 0.2f, 4.5f));
            CreateFloor(room2.transform, "Room2_Ceiling", new Vector3(14f, 4.2f, 0f), new Vector3(12f, 0.2f, 4.5f));
            CreateWall(room2.transform, "Room2_Wall_North", new Vector3(14f, 2f, 2.25f), new Vector3(12f, 4f, 0.8f));
            CreateWall(room2.transform, "Room2_Wall_South", new Vector3(14f, 2f, -2.25f), new Vector3(12f, 4f, 0.8f));
            CreateWall(room2.transform, "Room2_End_NorthPart", new Vector3(20f, 2f, 1.625f), new Vector3(0.8f, 4f, 1.25f));
            CreateWall(room2.transform, "Room2_End_SouthPart", new Vector3(20f, 2f, -1.625f), new Vector3(0.8f, 4f, 1.25f));
            CreateWall(room2.transform, "Room2_End_TopPart", new Vector3(20f, 3.1f, 0f), new Vector3(0.8f, 1.8f, 2f));
            CreateTorch(room2.transform, "Room2_Torch", new Vector3(19.2f, 1.8f, 1.7f));

            GameObject room2Crystal = CreateCrystalWithLight("Room2_ManaCrystal", new Vector3(16.2f, 1f, 0f), room2.transform, 30f);

            GameObject promptTrigger = new GameObject("Room2_PromptTrigger");
            promptTrigger.transform.SetParent(room2.transform);
            promptTrigger.transform.position = new Vector3(16.2f, 1f, 0f);
            SphereCollider promptCollider = promptTrigger.AddComponent<SphereCollider>();
            promptCollider.isTrigger = true;
            promptCollider.radius = 2.2f;
            TutorialPromptTrigger tutorialPrompt = promptTrigger.AddComponent<TutorialPromptTrigger>();

            Text promptText = uiRoot.transform.Find("TutorialUI/PromptText")?.GetComponent<Text>();
            SetObjectField(tutorialPrompt, "promptText", promptText);
            SetStringField(tutorialPrompt, "enterMessage", "Вытянуть ману");
            SetStringField(tutorialPrompt, "idleMessage", "Нажмите E для взаимодействия");

            // Room 3: first risk.
            GameObject room3 = new GameObject("Room3_FirstRisk");
            room3.transform.SetParent(levelRoot.transform);
            CreateFloor(room3.transform, "Room3_Floor", new Vector3(30f, -0.1f, 0f), new Vector3(16f, 0.2f, 12f));
            CreateFloor(room3.transform, "Room3_Ceiling", new Vector3(30f, 4.2f, 0f), new Vector3(16f, 0.2f, 12f));
            CreateWall(room3.transform, "Room3_Wall_North", new Vector3(30f, 2f, 6f), new Vector3(16f, 4f, 0.8f));
            CreateWall(room3.transform, "Room3_Wall_South", new Vector3(30f, 2f, -6f), new Vector3(16f, 4f, 0.8f));
            CreateWall(room3.transform, "Room3_Wall_East", new Vector3(38f, 2f, 0f), new Vector3(0.8f, 4f, 12f));
            CreateWall(room3.transform, "Room3_Wall_West_NorthPart", new Vector3(22f, 2f, 4f), new Vector3(0.8f, 4f, 8f));
            CreateWall(room3.transform, "Room3_Wall_West_SouthPart", new Vector3(22f, 2f, -4f), new Vector3(0.8f, 4f, 8f));
            CreateWall(room3.transform, "Room3_Wall_West_TopPart", new Vector3(22f, 3.1f, 0f), new Vector3(0.8f, 1.8f, 4f));
            CreateTorch(room3.transform, "Room3_Torch", new Vector3(22.8f, 1.8f, -5f));

            // Transition floors to avoid seams/fall-through between room chunks.
            CreateFloor(levelRoot.transform, "Transition_Floor_Room1_To_Room2", new Vector3(7.75f, -0.1f, 0f), new Vector3(1f, 0.2f, 4.5f));
            CreateFloor(levelRoot.transform, "Transition_Ceiling_Room1_To_Room2", new Vector3(7.75f, 4.2f, 0f), new Vector3(1f, 0.2f, 4.5f));
            CreateFloor(levelRoot.transform, "Transition_Floor_Room2_To_Room3", new Vector3(21f, -0.1f, 0f), new Vector3(2f, 0.2f, 4.5f));
            CreateFloor(levelRoot.transform, "Transition_Ceiling_Room2_To_Room3", new Vector3(21f, 4.2f, 0f), new Vector3(2f, 0.2f, 4.5f));

            GameObject room3Crystal = CreateCrystalWithLight("Room3_ManaCrystal", new Vector3(30f, 1f, 0f), room3.transform, 35f);

            GameObject nicheWallA = CreateWall(room3.transform, "GuardianNicheWall_A", new Vector3(35.7f, 2f, 4.1f), new Vector3(0.8f, 4f, 3f));
            GameObject nicheWallB = CreateWall(room3.transform, "GuardianNicheWall_B", new Vector3(33.9f, 2f, 5.4f), new Vector3(3.6f, 4f, 0.8f));

            GameObject guardian = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            guardian.name = "Guardian_Sleeper";
            guardian.transform.SetParent(room3.transform);
            guardian.transform.position = new Vector3(34.9f, 1f, 3.7f);
            NavMeshAgent guardianAgent = guardian.AddComponent<NavMeshAgent>();
            guardianAgent.stoppingDistance = 1.2f;
            GuardianController guardianController = guardian.AddComponent<GuardianController>();
            SetObjectField(guardianController, "player", player.transform);
            SetObjectField(guardianController, "dragon", dragonCompanion);

            // Systems + UI controllers.
            GameObject systems = new GameObject("SceneSystems");
            systems.transform.SetParent(levelRoot.transform);
            DragonHungerSystem hunger = systems.AddComponent<DragonHungerSystem>();
            systems.AddComponent<CrystalDrainAlarmRelay>();
            GuardianDeathRewardRelay rewardRelay = systems.AddComponent<GuardianDeathRewardRelay>();
            GameplayFlowController flow = systems.AddComponent<GameplayFlowController>();
            FogAndPostProcessingSetup fogAndPostProcessing = systems.AddComponent<FogAndPostProcessingSetup>();
            HungerVisualEffects hungerVisualEffects = systems.AddComponent<HungerVisualEffects>();
            Component pauseMenu = TryAddComponentByName(systems, "DungeonPrototype.UI.PauseMenuController");

            SetObjectField(hunger, "dragon", dragonCompanion);
            SetObjectField(hunger, "playerHealth", playerHealth);
            SetObjectField(hungerVisualEffects, "postProcessing", fogAndPostProcessing);
            SetObjectField(hungerVisualEffects, "dragonHunger", hunger);
            SetObjectField(rewardRelay, "dragon", dragonCompanion);
            SetObjectField(rewardRelay, "inventory", player.GetComponent<PlayerResourceInventory>());

            SetObjectField(flow, "playerHealth", playerHealth);
            SetObjectField(flow, "dragon", dragonCompanion);
            SetObjectField(flow, "playerController", firstPerson);
            SetObjectField(flow, "gate", gateController);
            SetStringField(flow, "mainMenuScene", "MainMenu");
            SetStringField(flow, "fallbackGameplayScene", "DungeonPrototype_Prototype");

            if (pauseMenu != null)
            {
                SetObjectField(pauseMenu, "playerController", firstPerson);
                SetStringField(pauseMenu, "mainMenuScene", "MainMenu");
                SetStringField(pauseMenu, "fallbackGameplayScene", "DungeonPrototype_Prototype");
            }

            // Exit zone past gate.
            GameObject exitZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exitZone.name = "ExitZone";
            exitZone.transform.SetParent(room1.transform);
            exitZone.transform.position = new Vector3(0f, 1f, 8.8f);
            exitZone.transform.localScale = new Vector3(4f, 2f, 1f);
            BoxCollider exitCollider = exitZone.GetComponent<BoxCollider>();
            exitCollider.isTrigger = true;
            Renderer exitRenderer = exitZone.GetComponent<Renderer>();
            if (exitRenderer != null)
            {
                exitRenderer.enabled = false;
            }
            LevelExitZone levelExit = exitZone.AddComponent<LevelExitZone>();
            SetObjectField(levelExit, "flow", flow);

            // Drain interactor setup after crystals exist.
            SetObjectField(drainInteractor, "dragon", dragonCompanion);
            SetObjectField(drainInteractor, "sourcePoint", player.transform);
            SetIntField(drainInteractor, "crystalMask", ~0);

            // Bake NavMesh via NavMeshSurface when available.
            TryAddNavMeshSurfaceAndBuild(levelRoot);

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);

            Selection.activeGameObject = levelRoot;
            EditorGUIUtility.PingObject(levelRoot);
            Debug.Log("Linear tutorial level built: Room1 -> Hole -> Room2 -> Room3.");
        }

        private static GameObject CreateTutorialUi()
        {
            GameObject canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas c = canvas.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            GameObject panel = CreateUIObject("TutorialUI", canvas.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(820f, 84f), new Vector2(0f, 70f));
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.04f, 0.05f, 0.1f, 0.7f);

            GameObject textObj = CreateUIObject("PromptText", panel.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(-24f, -14f), Vector2.zero);
            Text text = textObj.AddComponent<Text>();
            text.font = font;
            text.fontSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.9f, 0.95f, 1f, 1f);
            text.text = string.Empty;
            text.enabled = false;

            return canvas;
        }

        private static void ConfigureLighting()
        {
            GameObject directional = GameObject.Find("Directional Light");
            if (directional != null)
            {
                UnityEngine.Object.DestroyImmediate(directional);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color32(10, 10, 15, 255);
            RenderSettings.fog = false;
        }

        private static GameObject CreatePlayer(Vector3 position)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = position;
            player.tag = "Player";
            UnityEngine.Object.DestroyImmediate(player.GetComponent<Collider>());

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.radius = 0.35f;

            player.AddComponent<PlayerHealth>();
            player.AddComponent<PlayerResourceInventory>();
            player.AddComponent<ManaDrainInteractor>();
            SimpleFirstPersonController firstPerson = player.AddComponent<SimpleFirstPersonController>();

            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.gameObject.AddComponent<AudioListener>();
            camera.transform.SetParent(player.transform);
            camera.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            camera.transform.localRotation = Quaternion.identity;

            SetObjectField(firstPerson, "cameraPivot", camera.transform);
            return player;
        }

        private static void CreateRoom1WallsWithHole(Transform parent)
        {
            CreateWall(parent, "Room1_Wall_North", new Vector3(0f, 2f, 7.5f), new Vector3(15f, 4f, 0.8f));
            CreateWall(parent, "Room1_Wall_South", new Vector3(0f, 2f, -7.5f), new Vector3(15f, 4f, 0.8f));
            CreateWall(parent, "Room1_Wall_West", new Vector3(-7.5f, 2f, 0f), new Vector3(0.8f, 4f, 15f));

            // East wall split with a passable opening for the default CharacterController.
            CreateWall(parent, "Room1_EastWall_NorthPart", new Vector3(7.5f, 2f, 4.125f), new Vector3(0.8f, 4f, 6.75f));
            CreateWall(parent, "Room1_EastWall_SouthPart", new Vector3(7.5f, 2f, -4.125f), new Vector3(0.8f, 4f, 6.75f));
            CreateWall(parent, "Room1_EastWall_TopPart", new Vector3(7.5f, 3.1f, 0f), new Vector3(0.8f, 1.8f, 1.5f));
        }

        private static GameObject CreateFloor(Transform parent, string name, Vector3 position, Vector3 size)
        {
            return CreateWall(parent, name, position, size);
        }

        private static GameObject CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            return wall;
        }

        private static void CreateTorch(Transform parent, string name, Vector3 position)
        {
            GameObject torch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torch.name = name;
            torch.transform.SetParent(parent);
            torch.transform.position = position;
            torch.transform.localScale = new Vector3(0.12f, 0.45f, 0.12f);

            GameObject flame = new GameObject(name + "_Light");
            flame.transform.SetParent(torch.transform);
            flame.transform.localPosition = new Vector3(0f, 0.7f, 0f);

            Light l = flame.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.72f, 0.35f, 1f);
            l.intensity = 1.35f;
            l.range = 7.5f;
            l.shadows = LightShadows.Soft;
        }

        private static GameObject CreateCrystalWithLight(string name, Vector3 pos, Transform parent, float maxMana)
        {
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crystal.name = name;
            crystal.transform.SetParent(parent);
            crystal.transform.position = pos;
            crystal.transform.localScale = new Vector3(0.75f, 1f, 0.75f);

            ManaCrystal manaCrystal = crystal.AddComponent<ManaCrystal>();
            crystal.AddComponent<LightManager>();
            SetObjectField(manaCrystal, "crystalRenderer", crystal.GetComponent<Renderer>());
            SetFloatField(manaCrystal, "maxMana", maxMana);

            GameObject lightObj = new GameObject("CrystalLight");
            lightObj.transform.SetParent(crystal.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.42f, 0.34f, 1f, 1f);
            l.range = 8f;
            l.intensity = 2f;

            SetObjectField(manaCrystal, "crystalPointLight", l);
            return crystal;
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

            if (navMeshSurfaceType != null)
            {
                Component surface = target.GetComponent(navMeshSurfaceType) ?? target.AddComponent(navMeshSurfaceType);
                var buildMethod = navMeshSurfaceType.GetMethod("BuildNavMesh", Type.EmptyTypes);
                if (surface != null && buildMethod != null)
                {
                    buildMethod.Invoke(surface, null);
                    return;
                }
            }

            EditorApplication.ExecuteMenuItem("AI/Bake");
        }

        private static void EnsureInputSystemEventModule(Scene scene)
        {
            EventSystem eventSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
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

        private static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;
            return go;
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
            if (target == null)
            {
                return;
            }

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
            if (target == null)
            {
                return;
            }

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
            if (target == null)
            {
                return;
            }

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
            if (target == null)
            {
                return;
            }

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
            if (target == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                return;
            }

            prop.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetVector3Field(UnityEngine.Object target, string fieldName, Vector3 value)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                return;
            }

            prop.vector3Value = value;
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
    }
}
#endif
