using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DungeonPrototype.Gameplay
{
    /// <summary>
    /// Applies runtime fixes for previously built tutorial scenes so Tools rebuild is not required.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public class LinearTutorialRuntimePatcher : MonoBehaviour
    {
        private static readonly string[] HandsFbxPaths =
        {
            "Assets/Models/Characters/Player/gamehand.fbx",
            "Assets/Models/Characters/Player/gamehands.fbx",
            "Assets/gamehand.fbx",
            "Assets/gamehands.fbx"
        };

        private bool _applied;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            GameObject levelRoot = GameObject.Find("LevelRoot");
            if (levelRoot == null)
            {
                return;
            }

            if (levelRoot.GetComponent<LinearTutorialRuntimePatcher>() == null)
            {
                levelRoot.AddComponent<LinearTutorialRuntimePatcher>();
            }
        }

        private void Awake()
        {
            ApplyOnce();
        }

        private void Start()
        {
            ApplyOnce();
        }

        private void ApplyOnce()
        {
            if (_applied)
            {
                return;
            }

            if (GameObject.Find("Room1_Exposition") == null)
            {
                return;
            }

            ApplyGeometryFixes();
            ApplyLightingFixes();
            ApplyPromptFixes();
            EnsureSolidEnvironmentColliders();
            EnsureFirstPersonHands();
            _applied = true;
        }

        private static void EnsureFirstPersonHands()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            Transform existingHands = mainCamera.transform.Find("FirstPersonHands");
            if (existingHands != null)
            {
                if (ConfigureFirstPersonHands(existingHands.gameObject, mainCamera.transform))
                {
                    return;
                }

                Destroy(existingHands.gameObject);
            }

#if UNITY_EDITOR
            GameObject handsAsset = null;
            for (int i = 0; i < HandsFbxPaths.Length && handsAsset == null; i++)
            {
                handsAsset = AssetDatabase.LoadAssetAtPath<GameObject>(HandsFbxPaths[i]);
            }
            if (handsAsset == null)
            {
                return;
            }

            GameObject handsInstance = PrefabUtility.InstantiatePrefab(handsAsset) as GameObject;
            if (handsInstance == null)
            {
                return;
            }

            ConfigureFirstPersonHands(handsInstance, mainCamera.transform);
#endif
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
                Destroy(colliders[i]);
            }

            Camera[] childCameras = handsInstance.GetComponentsInChildren<Camera>(true);
            for (int i = 0; i < childCameras.Length; i++)
            {
                Destroy(childCameras[i].gameObject);
            }

            Light[] childLights = handsInstance.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < childLights.Length; i++)
            {
                Destroy(childLights[i].gameObject);
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

        private static void ApplyGeometryFixes()
        {
            GameObject levelRoot = GameObject.Find("LevelRoot");
            GameObject room1 = GameObject.Find("Room1_Exposition");
            GameObject room2 = GameObject.Find("Room2_SafeTraining");
            GameObject room3 = GameObject.Find("Room3_FirstRisk");

            if (levelRoot == null || room1 == null || room2 == null || room3 == null)
            {
                return;
            }

            DestroyIfExists("Room2_End");
            DestroyIfExists("Room3_Wall_West");

            EnsureCube(room2.transform, "Room2_End_NorthPart", new Vector3(20f, 2f, 1.625f), new Vector3(0.8f, 4f, 1.25f));
            EnsureCube(room2.transform, "Room2_End_SouthPart", new Vector3(20f, 2f, -1.625f), new Vector3(0.8f, 4f, 1.25f));
            EnsureCube(room2.transform, "Room2_End_TopPart", new Vector3(20f, 3.1f, 0f), new Vector3(0.8f, 1.8f, 2f));

            // Keep a walkable opening to Room 3.
            EnsureCube(room3.transform, "Room3_Wall_West_NorthPart", new Vector3(22f, 2f, 3.5f), new Vector3(0.8f, 4f, 5f));
            EnsureCube(room3.transform, "Room3_Wall_West_SouthPart", new Vector3(22f, 2f, -3.5f), new Vector3(0.8f, 4f, 5f));
            EnsureCube(room3.transform, "Room3_Wall_West_TopPart", new Vector3(22f, 3.2f, 0f), new Vector3(0.8f, 1.6f, 2f));

            DestroyIfExists("Room3_Wall_East");
            EnsureCube(room3.transform, "Room3_Wall_East_NorthPart", new Vector3(38f, 2f, 4f), new Vector3(0.8f, 4f, 4f));
            EnsureCube(room3.transform, "Room3_Wall_East_SouthPart", new Vector3(38f, 2f, -4f), new Vector3(0.8f, 4f, 4f));
            EnsureCube(room3.transform, "Room3_Wall_East_TopPart", new Vector3(38f, 3.2f, 0f), new Vector3(0.8f, 1.6f, 4f));

            EnsureCube(levelRoot.transform, "Transition_Floor_Room1_To_Room2", new Vector3(7.75f, -0.1f, 0f), new Vector3(1f, 0.2f, 4.5f));
            EnsureCube(levelRoot.transform, "Transition_Ceiling_Room1_To_Room2", new Vector3(7.75f, 4.2f, 0f), new Vector3(1f, 0.2f, 4.5f));
            EnsureCube(levelRoot.transform, "Transition_Floor_Room2_To_Room3", new Vector3(21f, -0.1f, 0f), new Vector3(2f, 0.2f, 4.5f));
            EnsureCube(levelRoot.transform, "Transition_Ceiling_Room2_To_Room3", new Vector3(21f, 4.2f, 0f), new Vector3(2f, 0.2f, 4.5f));
            EnsureCube(levelRoot.transform, "Transition_Floor_Room3_To_Room4", new Vector3(39f, -0.1f, 0f), new Vector3(2f, 0.2f, 4.8f));
            EnsureCube(levelRoot.transform, "Transition_Ceiling_Room3_To_Room4", new Vector3(39f, 4.2f, 0f), new Vector3(2f, 0.2f, 4.8f));

            EnsureCube(room1.transform, "Room1_Ceiling", new Vector3(0f, 4.2f, 0f), new Vector3(15f, 0.2f, 15f));
            EnsureCube(room2.transform, "Room2_Ceiling", new Vector3(14f, 4.2f, 0f), new Vector3(12f, 0.2f, 4.5f));
            EnsureCube(room3.transform, "Room3_Ceiling", new Vector3(30f, 4.2f, 0f), new Vector3(16f, 0.2f, 12f));

            DestroyIfExists("Room1_Torch_A");
            DestroyIfExists("Room1_Torch_B");
            DestroyIfExists("Room2_Torch");
            DestroyIfExists("Room3_Torch");

            CleanupLegacyGuideCrystals();
        }

        private static void CleanupLegacyGuideCrystals()
        {
            for (int i = 1; i <= 12; i++)
            {
                DestroyIfExists("GuideCrystal_" + i);
            }

            for (int i = 0; i < 5; i++)
            {
                DestroyIfExists("ColumnCrystal_N_" + i);
                DestroyIfExists("ColumnCrystal_S_" + i);
            }

            DestroyIfExists("AbyssBeaconCluster");
        }

        private static void ApplyLightingFixes()
        {
            GameObject directionalLight = GameObject.Find("Directional Light");
            if (directionalLight != null)
            {
                Destroy(directionalLight);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;
        }

        private static void ApplyPromptFixes()
        {
            GameObject promptTextGo = GameObject.Find("Canvas/TutorialUI/PromptText");
            Text promptText = promptTextGo != null ? promptTextGo.GetComponent<Text>() : null;
            if (promptText != null)
            {
                promptText.text = string.Empty;
                promptText.enabled = false;
            }

            TutorialPromptTrigger[] triggers = FindObjectsByType<TutorialPromptTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            FieldInfo idleField = typeof(TutorialPromptTrigger).GetField("idleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo enterField = typeof(TutorialPromptTrigger).GetField("enterMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo textField = typeof(TutorialPromptTrigger).GetField("promptText", BindingFlags.NonPublic | BindingFlags.Instance);

            for (int i = 0; i < triggers.Length; i++)
            {
                if (idleField != null)
                {
                    idleField.SetValue(triggers[i], string.Empty);
                }

                if (enterField != null)
                {
                    enterField.SetValue(triggers[i], "Вытянуть ману");
                }

                if (textField != null && promptText != null)
                {
                    textField.SetValue(triggers[i], promptText);
                }
            }
        }

        private static void DestroyIfExists(string objectName)
        {
            GameObject obj = GameObject.Find(objectName);
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        private static void EnsureCube(Transform parent, string name, Vector3 worldPosition, Vector3 localScale)
        {
            GameObject cube = GameObject.Find(name);
            if (cube == null)
            {
                cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = name;
            }

            cube.transform.SetParent(parent);
            cube.transform.position = worldPosition;
            cube.transform.localScale = localScale;

            Collider collider = cube.GetComponent<Collider>();
            if (collider == null)
            {
                collider = cube.AddComponent<BoxCollider>();
            }

            collider.enabled = true;
            collider.isTrigger = false;
        }

        private static bool ShouldTorchCastSoftShadows(string torchName)
        {
            return torchName == "Room3_Torch";
        }

        private static void EnsureTorch(Transform parent, string name, Vector3 worldPosition)
        {
            GameObject torch = GameObject.Find(name);
            if (torch == null)
            {
                torch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                torch.name = name;
            }

            torch.transform.SetParent(parent);
            torch.transform.position = worldPosition;
            torch.transform.localScale = new Vector3(0.12f, 0.45f, 0.12f);

            Transform lightTransform = torch.transform.Find(name + "_Light");
            if (lightTransform == null)
            {
                GameObject lightObj = new GameObject(name + "_Light");
                lightObj.transform.SetParent(torch.transform);
                lightObj.transform.localPosition = new Vector3(0f, 0.7f, 0f);
                lightTransform = lightObj.transform;
            }

            Light l = lightTransform.GetComponent<Light>();
            if (l == null)
            {
                l = lightTransform.gameObject.AddComponent<Light>();
            }

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

            Collider collider = torch.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
                collider.isTrigger = false;
            }
        }

        private static void EnsureSolidEnvironmentColliders()
        {
            GameObject levelRoot = GameObject.Find("LevelRoot");
            if (levelRoot == null)
            {
                return;
            }

            Collider[] colliders = levelRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider c = colliders[i];
                if (c == null)
                {
                    continue;
                }

                if (c.gameObject.name.Contains("PromptTrigger") || c.gameObject.name.Contains("ExitZone"))
                {
                    continue;
                }

                c.enabled = true;
                c.isTrigger = false;
            }
        }
    }
}
