#if UNITY_EDITOR
using System;
using DungeonPrototype.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DungeonPrototype.EditorTools
{
    public static class MainMenuSceneBuilder
    {
        private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("Tools/Dungeon Prototype/Create Main Menu Scene")]
        public static void CreateMainMenuScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";

            CreateCamera();
            CreateEventSystem();

            GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Font defaultFont = GetDefaultFont();

            GameObject bg = CreateUIObject("Background", canvasGo.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.06f, 0.08f, 0.13f, 1f);

            GameObject topGlow = CreateUIObject("TopGlow", canvasGo.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1200f, 320f), new Vector2(0f, -60f));
            Image topGlowImage = topGlow.AddComponent<Image>();
            topGlowImage.color = new Color(0.24f, 0.38f, 0.62f, 0.32f);

            GameObject bottomShade = CreateUIObject("BottomShade", canvasGo.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1400f, 280f), new Vector2(0f, 40f));
            Image bottomShadeImage = bottomShade.AddComponent<Image>();
            bottomShadeImage.color = new Color(0.02f, 0.03f, 0.05f, 0.45f);

            GameObject panel = CreateUIObject("MenuPanel", canvasGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(640f, 500f), new Vector2(0f, 10f));
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.10f, 0.14f, 0.22f, 0.95f);

            GameObject panelAccent = CreateUIObject("PanelAccent", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(560f, 8f), new Vector2(0f, -16f));
            Image panelAccentImage = panelAccent.AddComponent<Image>();
            panelAccentImage.color = new Color(0.35f, 0.56f, 0.9f, 0.9f);

            GameObject title = CreateUIObject("Title", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(560f, 90f), new Vector2(0f, -68f));
            Text titleText = title.AddComponent<Text>();
            titleText.font = defaultFont;
            titleText.fontSize = 58;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(0.95f, 0.97f, 1f, 1f);
            titleText.text = "Hard Game";

            GameObject subtitle = CreateUIObject("Subtitle", panel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(560f, 42f), new Vector2(0f, -124f));
            Text subtitleText = subtitle.AddComponent<Text>();
            subtitleText.font = defaultFont;
            subtitleText.fontSize = 22;
            subtitleText.alignment = TextAnchor.MiddleCenter;
            subtitleText.color = new Color(0.64f, 0.76f, 0.93f, 0.95f);
            subtitleText.text = "Дракон, кристаллы и древний культ";

            MainMenuController controller = panel.AddComponent<MainMenuController>();

            Button newGameButton = CreateButton(panel.transform, defaultFont, "NewGameButton", "Новая игра", new Vector2(0f, -245f));
            Button exitButton = CreateButton(panel.transform, defaultFont, "ExitButton", "Выход", new Vector2(0f, -340f));

            UnityEventTools.AddPersistentListener(newGameButton.onClick, controller.StartNewGame);
            UnityEventTools.AddPersistentListener(exitButton.onClick, controller.QuitGame);

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, MenuScenePath);
            EnsureBuildScenes(
                MenuScenePath,
                "Assets/Scenes/DungeonPrototype_Start.unity",
                "Assets/Scenes/DungeonPrototype_Prototype.unity",
                "Assets/Scenes/SampleScene.unity");

            Selection.activeGameObject = panel;
            EditorGUIUtility.PingObject(panel);
            Debug.Log("MainMenu scene created. Open Build Settings to verify scene order.");
        }

        private static void CreateCamera()
        {
            GameObject cameraGo = new GameObject("Main Camera");
            Camera cam = cameraGo.AddComponent<Camera>();
            cameraGo.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.07f, 1f);
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));

            Type inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                eventSystem.AddComponent(inputSystemModuleType);
            }
            else
            {
                eventSystem.AddComponent<StandaloneInputModule>();
            }
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                return font;
            }

            // Fallback for older editor versions.
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static Button CreateButton(Transform parent, Font font, string name, string label, Vector2 anchoredPos)
        {
            GameObject buttonGo = CreateUIObject(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(390f, 72f), anchoredPos);
            Image image = buttonGo.AddComponent<Image>();
            image.color = new Color(0.19f, 0.31f, 0.52f, 1f);

            Button button = buttonGo.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.19f, 0.31f, 0.52f, 1f);
            colors.highlightedColor = new Color(0.27f, 0.43f, 0.7f, 1f);
            colors.pressedColor = new Color(0.13f, 0.21f, 0.35f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            GameObject textGo = CreateUIObject("Text", buttonGo.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Text text = textGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;

            return button;
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
