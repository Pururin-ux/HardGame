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
        private static readonly string[] HandsFbxPaths =
        {
            "Assets/Models/Characters/Player/gamehand.fbx",
            "Assets/Models/Characters/Player/gamehands.fbx",
            "Assets/gamehand.fbx",
            "Assets/gamehands.fbx"
        };
        private static Material s_stoneMaterial;
        private static Material s_crystalEmissionMaterial;

        [MenuItem("Tools/Dungeon Prototype/Build Linear Tutorial Level")]
        public static void BuildLinearTutorialLevel()
        {
            EditorSceneManager.SaveOpenScenes();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "DungeonPrototype_Prototype";

            ConfigureLighting();
            EnsureInputSystemEventModule(scene);

            GameObject levelRoot = new GameObject("LevelRoot");
            levelRoot.AddComponent<LinearTutorialRuntimePatcher>();
            GameObject uiRoot = CreateTutorialUi();
            GameObject gameManager = new GameObject("GameManager");

            // Room 1: exposition.
            GameObject room1 = new GameObject("Room1_Exposition");
            room1.transform.SetParent(levelRoot.transform);
            CreateFloor(room1.transform, "Room1_Floor", new Vector3(0f, -0.1f, 0f), new Vector3(15f, 0.2f, 15f));
            CreateFloor(room1.transform, "Room1_Ceiling", new Vector3(0f, 4.2f, 0f), new Vector3(15f, 0.2f, 15f));
            CreateRoom1WallsWithHole(room1.transform);

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
            SetStringField(tutorialPrompt, "idleMessage", string.Empty);

            // Room 3: first risk.
            GameObject room3 = new GameObject("Room3_FirstRisk");
            room3.transform.SetParent(levelRoot.transform);
            CreateFloor(room3.transform, "Room3_Floor", new Vector3(30f, -0.1f, 0f), new Vector3(16f, 0.2f, 12f));
            CreateFloor(room3.transform, "Room3_Ceiling", new Vector3(30f, 4.2f, 0f), new Vector3(16f, 0.2f, 12f));
            CreateWall(room3.transform, "Room3_Wall_North", new Vector3(30f, 2f, 6f), new Vector3(16f, 4f, 0.8f));
            CreateWall(room3.transform, "Room3_Wall_South", new Vector3(30f, 2f, -6f), new Vector3(16f, 4f, 0.8f));
            CreateRoom3EastOpening(room3.transform);
            // West wall split with a walkable doorway to Room 3.
            CreateWall(room3.transform, "Room3_Wall_West_NorthPart", new Vector3(22f, 2f, 3.5f), new Vector3(0.8f, 4f, 5f));
            CreateWall(room3.transform, "Room3_Wall_West_SouthPart", new Vector3(22f, 2f, -3.5f), new Vector3(0.8f, 4f, 5f));
            CreateWall(room3.transform, "Room3_Wall_West_TopPart", new Vector3(22f, 3.2f, 0f), new Vector3(0.8f, 1.6f, 2f));

            // Transition floors to avoid seams/fall-through between room chunks.
            CreateFloor(levelRoot.transform, "Transition_Floor_Room1_To_Room2", new Vector3(7.75f, -0.1f, 0f), new Vector3(1f, 0.2f, 4.5f));
            CreateFloor(levelRoot.transform, "Transition_Ceiling_Room1_To_Room2", new Vector3(7.75f, 4.2f, 0f), new Vector3(1f, 0.2f, 4.5f));
            CreateFloor(levelRoot.transform, "Transition_Floor_Room2_To_Room3", new Vector3(21f, -0.1f, 0f), new Vector3(2f, 0.2f, 4.5f));
            CreateFloor(levelRoot.transform, "Transition_Ceiling_Room2_To_Room3", new Vector3(21f, 4.2f, 0f), new Vector3(2f, 0.2f, 4.5f));
            CreateFloor(levelRoot.transform, "Transition_Floor_Room3_To_Room4", new Vector3(39f, -0.1f, 0f), new Vector3(2f, 0.2f, 4.8f));
            CreateFloor(levelRoot.transform, "Transition_Ceiling_Room3_To_Room4", new Vector3(39f, 4.2f, 0f), new Vector3(2f, 0.2f, 4.8f));

            GameObject room3Crystal = CreateCrystalWithLight("Room3_ManaCrystal", new Vector3(30f, 1f, 0f), room3.transform, 35f);
            GameObject room4 = CreateAncientVaultRoom(levelRoot.transform);
            CreateCrystalNavigationPath(room4.transform);

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

            // Exit zone inside the dark arch at the far end of Room4.
            GameObject exitZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exitZone.name = "ExitZone";
            exitZone.transform.SetParent(room4.transform);
            exitZone.transform.position = new Vector3(63.2f, 2.1f, 0f);
            exitZone.transform.localScale = new Vector3(3.4f, 4f, 3f);
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
            Debug.Log("Linear tutorial level built: Room1 -> Room2 -> Room3 -> Room4 (Abyss Arch).");
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
            RenderSettings.ambientLight = Color.black;
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.01f, 0.015f, 0.03f, 1f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.045f;
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

            TryAttachFirstPersonHands(camera.transform);

            SetObjectField(firstPerson, "cameraPivot", camera.transform);
            return player;
        }

        private static void TryAttachFirstPersonHands(Transform cameraTransform)
        {
            if (cameraTransform == null)
            {
                return;
            }

            Transform existingHands = cameraTransform.Find("FirstPersonHands");
            if (existingHands != null)
            {
                if (ConfigureFirstPersonHands(existingHands.gameObject, cameraTransform))
                {
                    return;
                }

                UnityEngine.Object.DestroyImmediate(existingHands.gameObject);
            }

            GameObject handsAsset = null;
            for (int i = 0; i < HandsFbxPaths.Length && handsAsset == null; i++)
            {
                handsAsset = AssetDatabase.LoadAssetAtPath<GameObject>(HandsFbxPaths[i]);
            }

            if (handsAsset == null)
            {
                Debug.LogWarning("First-person hands FBX not found at expected paths.");
                return;
            }

            GameObject handsInstance = PrefabUtility.InstantiatePrefab(handsAsset) as GameObject;
            if (handsInstance == null)
            {
                return;
            }

            ConfigureFirstPersonHands(handsInstance, cameraTransform);

            Renderer[] renderers = handsInstance.GetComponentsInChildren<Renderer>(true);
            Debug.Log("First-person hands attached. Renderers: " + renderers.Length);
        }

        private static bool ConfigureFirstPersonHands(GameObject handsInstance, Transform cameraTransform)
        {
            if (handsInstance == null || cameraTransform == null)
            {
                return false;
            }

            handsInstance.name = "FirstPersonHands";
            handsInstance.transform.SetParent(cameraTransform, false);
            NormalizeExtremeImportedTransforms(handsInstance.transform);

            handsInstance.transform.localPosition = new Vector3(0.17f, -0.22f, 0.6f);
            handsInstance.transform.localRotation = Quaternion.Euler(6f, 180f, 0f);
            handsInstance.transform.localScale = Vector3.one * 0.25f;

            Collider[] colliders = handsInstance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(colliders[i]);
            }

            Camera[] childCameras = handsInstance.GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < childCameras.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(childCameras[i].gameObject);
            }

            Light[] childLights = handsInstance.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < childLights.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(childLights[i].gameObject);
            }

            Renderer[] renderers = handsInstance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = true;
                renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderers[i].receiveShadows = false;
                renderers[i].allowOcclusionWhenDynamic = false;
                renderers[i].forceRenderingOff = false;
            }

            SkinnedMeshRenderer[] skinned = handsInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinned.Length; i++)
            {
                skinned[i].updateWhenOffscreen = true;
                skinned[i].localBounds = new Bounds(Vector3.zero, Vector3.one * 8f);
            }

            Camera cam = cameraTransform.GetComponent<Camera>();
            if (cam != null)
            {
                cam.nearClipPlane = 0.01f;
                cam.useOcclusionCulling = false;
            }

            return true;
        }

        private static void NormalizeExtremeImportedTransforms(Transform root)
        {
            if (root == null)
            {
                return;
            }

            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform t = all[i];
                if (t == null || t == root)
                {
                    continue;
                }

                Vector3 s = t.localScale;
                if (Mathf.Abs(s.x) > 100f || Mathf.Abs(s.y) > 100f || Mathf.Abs(s.z) > 100f)
                {
                    t.localScale = Vector3.one;
                }

                if (t.localPosition.sqrMagnitude > 25f)
                {
                    t.localPosition = Vector3.zero;
                }
            }
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

        private static void CreateRoom3EastOpening(Transform parent)
        {
            CreateWall(parent, "Room3_Wall_East_NorthPart", new Vector3(38f, 2f, 4f), new Vector3(0.8f, 4f, 4f));
            CreateWall(parent, "Room3_Wall_East_SouthPart", new Vector3(38f, 2f, -4f), new Vector3(0.8f, 4f, 4f));
            CreateWall(parent, "Room3_Wall_East_TopPart", new Vector3(38f, 3.2f, 0f), new Vector3(0.8f, 1.6f, 4f));
        }

        private static GameObject CreateAncientVaultRoom(Transform levelRoot)
        {
            GameObject room4 = new GameObject("Room4_AncientVault");
            room4.transform.SetParent(levelRoot);

            CreateFloor(room4.transform, "Room4_Floor", new Vector3(52f, -0.1f, 0f), new Vector3(24f, 0.2f, 18f));
            CreateFloor(room4.transform, "Room4_Ceiling_Base", new Vector3(52f, 9.2f, 0f), new Vector3(24f, 0.2f, 18f));

            // Nave-like path framing, inspired by the reference composition.
            CreateFloor(room4.transform, "Room4_Aisle_Floor", new Vector3(52f, -0.03f, 0f), new Vector3(22f, 0.08f, 4.3f));
            CreateWall(room4.transform, "Room4_Aisle_Edge_N", new Vector3(52f, 0.08f, 2.2f), new Vector3(22f, 0.14f, 0.18f));
            CreateWall(room4.transform, "Room4_Aisle_Edge_S", new Vector3(52f, 0.08f, -2.2f), new Vector3(22f, 0.14f, 0.18f));

            CreateWall(room4.transform, "Room4_Wall_North", new Vector3(52f, 4.5f, 9f), new Vector3(24f, 9f, 0.9f));
            CreateWall(room4.transform, "Room4_Wall_South", new Vector3(52f, 4.5f, -9f), new Vector3(24f, 9f, 0.9f));

            // West side remains open toward Room3 transition.
            CreateWall(room4.transform, "Room4_Wall_West_NorthPart", new Vector3(40f, 4.5f, 6.1f), new Vector3(0.9f, 9f, 5.8f));
            CreateWall(room4.transform, "Room4_Wall_West_SouthPart", new Vector3(40f, 4.5f, -6.1f), new Vector3(0.9f, 9f, 5.8f));
            CreateWall(room4.transform, "Room4_Wall_West_TopPart", new Vector3(40f, 7.3f, 0f), new Vector3(0.9f, 3.4f, 6.4f));

            // Far wall with central arch opening to darkness.
            CreateWall(room4.transform, "Room4_Wall_East_NorthPart", new Vector3(64f, 4.5f, 6.6f), new Vector3(0.9f, 9f, 4.8f));
            CreateWall(room4.transform, "Room4_Wall_East_SouthPart", new Vector3(64f, 4.5f, -6.6f), new Vector3(0.9f, 9f, 4.8f));
            CreateWall(room4.transform, "Room4_Wall_East_TopPart", new Vector3(64f, 8f, 0f), new Vector3(0.9f, 2f, 5.8f));

            CreatePointedArch(room4.transform, "Room4_FarArch", new Vector3(63.6f, 0f, 0f), 4.8f, 8.6f, 0.8f);
            CreateCollapsedMasonry(room4.transform, "Room4_FarArch_Rubble", new Vector3(62.9f, 0f, 0f));

            float[] columnX = { 44f, 48f, 52f, 56f, 60f };
            for (int i = 0; i < columnX.Length; i++)
            {
                CreateMassiveColumn(room4.transform, "Room4_Column_N_" + i, new Vector3(columnX[i], 0f, 6.8f), 8.4f);
                CreateMassiveColumn(room4.transform, "Room4_Column_S_" + i, new Vector3(columnX[i], 0f, -6.8f), 8.4f);

                CreatePointedArch(room4.transform, "Room4_Arcade_N_" + i, new Vector3(columnX[i], 0f, 4.95f), 2.8f, 6.4f, 0.55f);
                CreatePointedArch(room4.transform, "Room4_Arcade_S_" + i, new Vector3(columnX[i], 0f, -4.95f), 2.8f, 6.4f, 0.55f);

                if (i < columnX.Length - 1)
                {
                    float archX = (columnX[i] + columnX[i + 1]) * 0.5f;
                    CreatePointedArch(room4.transform, "Room4_Nave_Arch_" + i, new Vector3(archX, 0f, 0f), 5.3f, 8.8f, 0.32f);
                }

                GameObject ribNorth = CreateWall(room4.transform, "Room4_VaultRib_N_" + i, new Vector3(columnX[i], 6.5f, -3.7f), new Vector3(0.24f, 5.4f, 0.24f));
                ribNorth.transform.rotation = Quaternion.Euler(58f, 0f, 0f);

                GameObject ribSouth = CreateWall(room4.transform, "Room4_VaultRib_S_" + i, new Vector3(columnX[i], 6.5f, 3.7f), new Vector3(0.24f, 5.4f, 0.24f));
                ribSouth.transform.rotation = Quaternion.Euler(-58f, 0f, 0f);
            }

            CreateWall(room4.transform, "Room4_VaultRidge", new Vector3(52f, 9f, 0f), new Vector3(22f, 0.24f, 0.24f));

            // Deep side niches echoing the image silhouette.
            CreatePointedArch(room4.transform, "Room4_Niche_North_A", new Vector3(43.2f, 0f, 8.45f), 2.1f, 4.2f, 0.45f);
            CreatePointedArch(room4.transform, "Room4_Niche_South_A", new Vector3(43.2f, 0f, -8.45f), 2.1f, 4.2f, 0.45f);
            CreatePointedArch(room4.transform, "Room4_Niche_North_B", new Vector3(60.8f, 0f, 8.45f), 2.1f, 4.2f, 0.45f);
            CreatePointedArch(room4.transform, "Room4_Niche_South_B", new Vector3(60.8f, 0f, -8.45f), 2.1f, 4.2f, 0.45f);

            GameObject abyss = CreateWall(room4.transform, "Room4_AbyssVoid", new Vector3(69f, 4.5f, 0f), new Vector3(8f, 9f, 8f));
            Renderer abyssRenderer = abyss.GetComponent<Renderer>();
            if (abyssRenderer != null)
            {
                Shader voidShader = Shader.Find("Universal Render Pipeline/Unlit");
                if (voidShader == null)
                {
                    voidShader = Shader.Find("Unlit/Color");
                }

                if (voidShader == null)
                {
                    voidShader = Shader.Find("Standard");
                }

                Material voidMaterial = new Material(voidShader);
                voidMaterial.color = Color.black;
                if (voidMaterial.HasProperty("_BaseColor"))
                {
                    voidMaterial.SetColor("_BaseColor", Color.black);
                }

                if (voidMaterial.HasProperty("_Color"))
                {
                    voidMaterial.SetColor("_Color", Color.black);
                }

                abyssRenderer.sharedMaterial = voidMaterial;
                abyssRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                abyssRenderer.receiveShadows = false;
            }

            return room4;
        }

        private static void CreateMassiveColumn(Transform parent, string name, Vector3 basePosition, float height)
        {
            float halfHeight = height * 0.5f;

            GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = name;
            shaft.transform.SetParent(parent);
            shaft.transform.position = basePosition + new Vector3(0f, halfHeight, 0f);
            shaft.transform.localScale = new Vector3(0.95f, halfHeight, 0.95f);
            Renderer shaftRenderer = shaft.GetComponent<Renderer>();
            if (shaftRenderer != null)
            {
                Material stone = GetStoneMaterial();
                if (stone != null)
                {
                    shaftRenderer.sharedMaterial = stone;
                }
            }

            CreateWall(parent, name + "_Base", basePosition + new Vector3(0f, 0.25f, 0f), new Vector3(1.6f, 0.5f, 1.6f));
            CreateWall(parent, name + "_Capital", basePosition + new Vector3(0f, height - 0.25f, 0f), new Vector3(1.7f, 0.5f, 1.7f));
        }

        private static void CreatePointedArch(Transform parent, string namePrefix, Vector3 baseCenter, float width, float height, float depth)
        {
            float halfWidth = width * 0.5f;
            float columnHeight = height * 0.64f;

            CreateWall(parent, namePrefix + "_LeftColumn", baseCenter + new Vector3(0f, columnHeight * 0.5f, -halfWidth), new Vector3(depth, columnHeight, 0.75f));
            CreateWall(parent, namePrefix + "_RightColumn", baseCenter + new Vector3(0f, columnHeight * 0.5f, halfWidth), new Vector3(depth, columnHeight, 0.75f));

            GameObject leftSpire = CreateWall(parent, namePrefix + "_LeftSpire", baseCenter + new Vector3(0f, height * 0.68f, -halfWidth * 0.33f), new Vector3(depth, height * 0.56f, 0.48f));
            leftSpire.transform.rotation = Quaternion.Euler(46f, 0f, 0f);

            GameObject rightSpire = CreateWall(parent, namePrefix + "_RightSpire", baseCenter + new Vector3(0f, height * 0.68f, halfWidth * 0.33f), new Vector3(depth, height * 0.56f, 0.48f));
            rightSpire.transform.rotation = Quaternion.Euler(-46f, 0f, 0f);
        }

        private static void CreateCollapsedMasonry(Transform parent, string prefix, Vector3 center)
        {
            Vector3[] offsets =
            {
                new Vector3(-0.35f, 0.2f, 2.45f),
                new Vector3(-0.3f, 0.35f, -2.5f),
                new Vector3(-0.2f, 0.15f, 3.1f),
                new Vector3(-0.2f, 0.25f, -3.15f),
                new Vector3(-0.15f, 0.75f, 2.2f),
                new Vector3(-0.15f, 0.85f, -2.25f),
                new Vector3(-0.1f, 0.55f, 2.75f),
                new Vector3(-0.1f, 0.65f, -2.8f)
            };

            Vector3[] scales =
            {
                new Vector3(0.6f, 0.35f, 0.55f),
                new Vector3(0.58f, 0.34f, 0.52f),
                new Vector3(0.42f, 0.28f, 0.5f),
                new Vector3(0.44f, 0.3f, 0.48f),
                new Vector3(0.48f, 0.38f, 0.45f),
                new Vector3(0.47f, 0.38f, 0.44f),
                new Vector3(0.52f, 0.32f, 0.43f),
                new Vector3(0.5f, 0.32f, 0.43f)
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                GameObject chunk = CreateWall(parent, prefix + "_" + i, center + offsets[i], scales[i]);
                chunk.transform.rotation = Quaternion.Euler(0f, (i % 2 == 0 ? 12f : -14f), i * 3f);
            }
        }

        private static void CreateCrystalNavigationPath(Transform room4)
        {
            // The path is marked by side-paired clusters, not center spam.
            CreateDecorativeGuideCrystal(room4, "GuideCrystal_Entry_Left", new Vector3(42.2f, 0f, 1.72f), 0.74f, 0.2f, 1.18f, 1.05f, 2.0f);
            CreateDecorativeGuideCrystal(room4, "GuideCrystal_Entry_Right", new Vector3(42.2f, 0f, -1.72f), 0.74f, 0.24f, 1.18f, 1.05f, 2.0f);

            float[] pairX = { 46f, 50f, 54f, 58f };
            for (int i = 0; i < pairX.Length; i++)
            {
                float t = pairX.Length > 1 ? i / (float)(pairX.Length - 1) : 0f;
                float sideZ = Mathf.Lerp(1.65f, 1.25f, t);
                float scale = Mathf.Lerp(0.7f, 0.84f, t);
                float emission = Mathf.Lerp(1.12f, 1.42f, t);
                float intensity = Mathf.Lerp(1.0f, 1.58f, t);
                float range = Mathf.Lerp(1.95f, 2.65f, t);
                float paletteLeft = Mathf.Lerp(0.2f, 0.55f, t);
                float paletteRight = Mathf.Lerp(0.24f, 0.64f, t);

                CreateDecorativeGuideCrystal(room4, "GuideCrystal_PathL_" + (i + 1), new Vector3(pairX[i], 0f, sideZ), scale, paletteLeft, emission, intensity, range);
                CreateDecorativeGuideCrystal(room4, "GuideCrystal_PathR_" + (i + 1), new Vector3(pairX[i], 0f, -sideZ), scale, paletteRight, emission, intensity, range);
            }

            // Dim niche accents in the side bays.
            CreateDecorativeGuideCrystal(room4, "GuideCrystal_Niche_North_A", new Vector3(43.3f, 0f, 7.65f), 0.58f, 0.22f, 0.98f, 0.72f, 1.75f);
            CreateDecorativeGuideCrystal(room4, "GuideCrystal_Niche_South_A", new Vector3(43.3f, 0f, -7.65f), 0.58f, 0.27f, 0.98f, 0.72f, 1.75f);
            CreateDecorativeGuideCrystal(room4, "GuideCrystal_Niche_North_B", new Vector3(60.9f, 0f, 7.65f), 0.58f, 0.3f, 0.98f, 0.72f, 1.75f);
            CreateDecorativeGuideCrystal(room4, "GuideCrystal_Niche_South_B", new Vector3(60.9f, 0f, -7.65f), 0.58f, 0.35f, 0.98f, 0.72f, 1.75f);

            // Final emphasis at the abyss gate.
            CreateDecorativeGuideCrystal(room4, "AbyssBeacon_Left", new Vector3(62.45f, 0f, 1.08f), 0.9f, 0.62f, 1.5f, 1.82f, 2.82f);
            CreateDecorativeGuideCrystal(room4, "AbyssBeacon_Right", new Vector3(62.45f, 0f, -1.08f), 0.9f, 0.69f, 1.5f, 1.82f, 2.82f);
        }

        private static GameObject CreateDecorativeGuideCrystal(Transform parent, string name, Vector3 worldPosition, float scaleMultiplier, float paletteT, float emissionMultiplier, float lightIntensity, float lightRange)
        {
            GameObject crystal = CreateCrystalGeometry(name, worldPosition, parent, scaleMultiplier, paletteT, emissionMultiplier, false);

            GameObject lightObj = new GameObject(name + "_Light");
            lightObj.transform.SetParent(crystal.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 0.8f * scaleMultiplier, 0f);

            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = EvaluateCrystalPalette(paletteT);
            l.intensity = lightIntensity;
            l.range = lightRange;
            l.shadows = LightShadows.None;

            return crystal;
        }

        private static Color EvaluateCrystalPalette(float t)
        {
            t = Mathf.Clamp01(t);
            Color cyan = new Color(0.08f, 1f, 0.96f, 1f);
            Color teal = new Color(0.08f, 0.78f, 0.88f, 1f);
            Color softPurple = new Color(0.66f, 0.38f, 1f, 1f);
            if (t < 0.55f)
            {
                return Color.Lerp(cyan, teal, t / 0.55f);
            }

            return Color.Lerp(teal, softPurple, (t - 0.55f) / 0.45f);
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

            Renderer wallRenderer = wall.GetComponent<Renderer>();
            if (wallRenderer != null)
            {
                Material stone = GetStoneMaterial();
                if (stone != null)
                {
                    wallRenderer.sharedMaterial = stone;
                }
            }

            return wall;
        }

        private static bool ShouldTorchCastSoftShadows(string torchName)
        {
            return torchName == "Room3_Torch";
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
            bool castSoftShadows = ShouldTorchCastSoftShadows(name);
            l.range = castSoftShadows ? 6f : 7.5f;

            if (castSoftShadows)
            {
                l.shadows = LightShadows.Soft;
                l.shadowStrength = 0.6f;
                l.shadowCustomResolution = 512;
            }
            else
            {
                l.shadows = LightShadows.None;
            }
        }

        private static GameObject CreateCrystalWithLight(string name, Vector3 pos, Transform parent, float maxMana)
        {
            GameObject crystal = CreateCrystalGeometry(name, pos, parent, 1f, 0.24f, 3.9f, true);
            Renderer crystalRenderer = crystal.GetComponent<Renderer>();

            ManaCrystal manaCrystal = crystal.AddComponent<ManaCrystal>();
            LightManager lightManager = crystal.AddComponent<LightManager>();
            SetObjectField(manaCrystal, "crystalRenderer", crystalRenderer);
            SetFloatField(manaCrystal, "maxMana", maxMana);
            SetColorField(manaCrystal, "activeEmissionColor", EvaluateCrystalPalette(0.25f) * 2.8f);
            SetColorField(manaCrystal, "drainingEmissionColor", EvaluateCrystalPalette(0.85f) * 2.1f);
            SetColorField(manaCrystal, "depletedEmissionColor", new Color(0.02f, 0.02f, 0.03f, 1f));

            GameObject lightObj = new GameObject("CrystalLight");
            lightObj.transform.SetParent(crystal.transform);
            lightObj.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = EvaluateCrystalPalette(0.3f);
            l.range = 4.4f;
            l.intensity = 2.8f;
            l.shadows = LightShadows.None;

            SetFloatField(lightManager, "maxIntensity", 2.8f);
            SetFloatField(lightManager, "minIntensity", 0.15f);

            SetObjectField(manaCrystal, "crystalPointLight", l);
            return crystal;
        }

        private static GameObject CreateCrystalGeometry(string name, Vector3 worldPosition, Transform parent, float scaleMultiplier, float paletteT, float emissionMultiplier, bool interactiveCollider)
        {
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crystal.name = name;
            crystal.transform.SetParent(parent);
            crystal.transform.position = worldPosition;
            crystal.transform.localScale = new Vector3(0.62f, 2f, 0.62f) * scaleMultiplier;
            crystal.transform.rotation = Quaternion.Euler(-8f, 0f, 6f);

            Renderer crystalRenderer = crystal.GetComponent<Renderer>();
            if (crystalRenderer != null)
            {
                ApplyCrystalShardVisual(crystalRenderer, EvaluateCrystalPalette(paletteT), emissionMultiplier);
            }

            GameObject shardA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shardA.name = "ShardA";
            shardA.transform.SetParent(crystal.transform, false);
            shardA.transform.localPosition = new Vector3(0.24f, -0.1f, 0.18f) * scaleMultiplier;
            shardA.transform.localScale = new Vector3(0.35f, 0.95f, 0.35f) * scaleMultiplier;
            shardA.transform.localRotation = Quaternion.Euler(-18f, 14f, 12f);
            DestroyCollider(shardA);
            ApplyCrystalShardVisual(shardA.GetComponent<Renderer>(), EvaluateCrystalPalette(Mathf.Clamp01(paletteT + 0.1f)), emissionMultiplier);

            GameObject shardB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shardB.name = "ShardB";
            shardB.transform.SetParent(crystal.transform, false);
            shardB.transform.localPosition = new Vector3(-0.2f, -0.15f, 0.14f) * scaleMultiplier;
            shardB.transform.localScale = new Vector3(0.3f, 0.85f, 0.3f) * scaleMultiplier;
            shardB.transform.localRotation = Quaternion.Euler(-16f, -18f, -10f);
            DestroyCollider(shardB);
            ApplyCrystalShardVisual(shardB.GetComponent<Renderer>(), EvaluateCrystalPalette(Mathf.Clamp01(paletteT + 0.2f)), emissionMultiplier);

            if (interactiveCollider)
            {
                DestroyCollider(crystal);
                CapsuleCollider interactionCollider = crystal.GetComponent<CapsuleCollider>();
                if (interactionCollider == null)
                {
                    interactionCollider = crystal.AddComponent<CapsuleCollider>();
                }

                interactionCollider.height = 2.4f * scaleMultiplier;
                interactionCollider.radius = 0.95f * scaleMultiplier;
                interactionCollider.center = Vector3.zero;
            }
            else
            {
                DestroyCollider(crystal);
            }

            return crystal;
        }

        private static void DestroyCollider(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            Collider collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void ApplyCrystalShardVisual(Renderer renderer, Color emission, float emissionMultiplier = 3.6f)
        {
            if (renderer == null)
            {
                return;
            }

            Material crystalMaterial = GetCrystalEmissionMaterial();
            if (crystalMaterial != null)
            {
                renderer.sharedMaterial = crystalMaterial;
            }

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);
            if (renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_BaseColor"))
            {
                mpb.SetColor("_BaseColor", Color.Lerp(emission, Color.white, 0.08f));
            }

            if (renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_Color"))
            {
                mpb.SetColor("_Color", Color.Lerp(emission, Color.white, 0.08f));
            }

            mpb.SetColor("_EmissionColor", emission * emissionMultiplier);
            renderer.SetPropertyBlock(mpb);
        }

        private static Material GetStoneMaterial()
        {
            if (s_stoneMaterial != null)
            {
                return s_stoneMaterial;
            }

            s_stoneMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Floor.mat");
            if (s_stoneMaterial != null)
            {
                return s_stoneMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                return null;
            }

            s_stoneMaterial = new Material(shader);
            if (s_stoneMaterial.HasProperty("_BaseColor"))
            {
                s_stoneMaterial.SetColor("_BaseColor", new Color(0.11f, 0.115f, 0.13f, 1f));
            }

            if (s_stoneMaterial.HasProperty("_Color"))
            {
                s_stoneMaterial.SetColor("_Color", new Color(0.11f, 0.115f, 0.13f, 1f));
            }

            return s_stoneMaterial;
        }

        private static Material GetCrystalEmissionMaterial()
        {
            if (s_crystalEmissionMaterial != null)
            {
                return s_crystalEmissionMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                return null;
            }

            s_crystalEmissionMaterial = new Material(shader);
            if (s_crystalEmissionMaterial.HasProperty("_BaseColor"))
            {
                s_crystalEmissionMaterial.SetColor("_BaseColor", new Color(0.2f, 0.9f, 0.95f, 1f));
            }

            if (s_crystalEmissionMaterial.HasProperty("_Color"))
            {
                s_crystalEmissionMaterial.SetColor("_Color", new Color(0.2f, 0.9f, 0.95f, 1f));
            }

            if (s_crystalEmissionMaterial.HasProperty("_EmissionColor"))
            {
                s_crystalEmissionMaterial.EnableKeyword("_EMISSION");
                s_crystalEmissionMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                s_crystalEmissionMaterial.SetColor("_EmissionColor", new Color(0.16f, 1f, 0.95f, 1f) * 3f);
            }

            return s_crystalEmissionMaterial;
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

        private static void SetColorField(UnityEngine.Object target, string fieldName, Color value)
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

            prop.colorValue = value;
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
