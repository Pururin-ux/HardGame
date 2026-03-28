using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonPrototype.Gameplay
{
    /// <summary>
    /// Applies runtime fixes for previously built tutorial scenes so Tools rebuild is not required.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public class LinearTutorialRuntimePatcher : MonoBehaviour
    {
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
            ApplyPromptFixes();
            EnsureSolidEnvironmentColliders();
            _applied = true;
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

            EnsureCube(levelRoot.transform, "Transition_Floor_Room1_To_Room2", new Vector3(7.75f, -0.1f, 0f), new Vector3(1f, 0.2f, 4.5f));
            EnsureCube(levelRoot.transform, "Transition_Ceiling_Room1_To_Room2", new Vector3(7.75f, 4.2f, 0f), new Vector3(1f, 0.2f, 4.5f));
            EnsureCube(levelRoot.transform, "Transition_Floor_Room2_To_Room3", new Vector3(21f, -0.1f, 0f), new Vector3(2f, 0.2f, 4.5f));
            EnsureCube(levelRoot.transform, "Transition_Ceiling_Room2_To_Room3", new Vector3(21f, 4.2f, 0f), new Vector3(2f, 0.2f, 4.5f));

            EnsureCube(room1.transform, "Room1_Ceiling", new Vector3(0f, 4.2f, 0f), new Vector3(15f, 0.2f, 15f));
            EnsureCube(room2.transform, "Room2_Ceiling", new Vector3(14f, 4.2f, 0f), new Vector3(12f, 0.2f, 4.5f));
            EnsureCube(room3.transform, "Room3_Ceiling", new Vector3(30f, 4.2f, 0f), new Vector3(16f, 0.2f, 12f));

            EnsureTorch(room1.transform, "Room1_Torch_A", new Vector3(-6.5f, 1.8f, 6.4f));
            EnsureTorch(room1.transform, "Room1_Torch_B", new Vector3(-6.5f, 1.8f, -6.4f));
            EnsureTorch(room2.transform, "Room2_Torch", new Vector3(19.2f, 1.8f, 1.7f));
            EnsureTorch(room3.transform, "Room3_Torch", new Vector3(22.8f, 1.8f, -5f));
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
            l.range = 7.5f;
            l.shadows = LightShadows.Soft;

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
