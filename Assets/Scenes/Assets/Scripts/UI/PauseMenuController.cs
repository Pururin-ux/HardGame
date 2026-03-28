using System;
using DungeonPrototype.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DungeonPrototype.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string fallbackGameplayScene = "DungeonPrototype_Prototype";

        [Header("References")]
        [SerializeField] private SimpleFirstPersonController playerController;

        private GameObject _panelRoot;
        private bool _isPaused;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<SimpleFirstPersonController>(FindObjectsInactive.Include);
            }

            EnsurePauseUI();
            SetPaused(false);
        }

        private void Update()
        {
            if (ReadTogglePausePressed())
            {
                SetPaused(!_isPaused);
            }
        }

        private void OnDestroy()
        {
            if (_isPaused)
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
            }
        }

        public void OnResumePressed()
        {
            SetPaused(false);
        }

        public void OnMainMenuPressed()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;

            if (!string.IsNullOrWhiteSpace(mainMenuScene) && Application.CanStreamedLevelBeLoaded(mainMenuScene))
            {
                SceneManager.LoadScene(mainMenuScene);
                return;
            }

            Debug.LogError("MainMenu scene is not in Build Settings.");
        }

        public void OnRestartPressed()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;

            string sceneName = string.IsNullOrWhiteSpace(fallbackGameplayScene) ? SceneManager.GetActiveScene().name : fallbackGameplayScene;
            if (!string.IsNullOrWhiteSpace(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnQuitPressed()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SetPaused(bool paused)
        {
            _isPaused = paused;

            Time.timeScale = paused ? 0f : 1f;
            AudioListener.pause = paused;

            if (playerController != null)
            {
                playerController.enabled = !paused;
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(paused);
            }

            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }

        private void EnsurePauseUI()
        {
            EnsureEventSystemInputCompatibility();

            GameObject canvasGo = new GameObject("PauseCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Font font = GetDefaultFont();

            GameObject dimmer = CreateUIObject("Dimmer", canvasGo.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Image dimmerImage = dimmer.AddComponent<Image>();
            dimmerImage.color = new Color(0f, 0f, 0f, 0.62f);

            _panelRoot = CreateUIObject("PausePanel", canvasGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 510f), Vector2.zero);
            Image panelImage = _panelRoot.AddComponent<Image>();
            panelImage.color = new Color(0.11f, 0.15f, 0.24f, 0.95f);

            GameObject title = CreateUIObject("Title", _panelRoot.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(500f, 84f), new Vector2(0f, -56f));
            Text titleText = title.AddComponent<Text>();
            titleText.font = font;
            titleText.fontSize = 52;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(0.95f, 0.97f, 1f, 1f);
            titleText.text = "Пауза";

            Button resumeButton = CreateButton(_panelRoot.transform, font, "ResumeButton", "Продолжить", new Vector2(0f, -165f));
            Button restartButton = CreateButton(_panelRoot.transform, font, "RestartButton", "Заново", new Vector2(0f, -250f));
            Button menuButton = CreateButton(_panelRoot.transform, font, "MenuButton", "В меню", new Vector2(0f, -335f));
            Button exitButton = CreateButton(_panelRoot.transform, font, "ExitButton", "Выход", new Vector2(0f, -420f));

            resumeButton.onClick.AddListener(OnResumePressed);
            restartButton.onClick.AddListener(OnRestartPressed);
            menuButton.onClick.AddListener(OnMainMenuPressed);
            exitButton.onClick.AddListener(OnQuitPressed);
        }

        private static Button CreateButton(Transform parent, Font font, string name, string label, Vector2 anchoredPos)
        {
            GameObject buttonGo = CreateUIObject(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(360f, 64f), anchoredPos);
            Image image = buttonGo.AddComponent<Image>();
            image.color = new Color(0.18f, 0.29f, 0.49f, 1f);

            Button button = buttonGo.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.18f, 0.29f, 0.49f, 1f);
            colors.highlightedColor = new Color(0.24f, 0.37f, 0.62f, 1f);
            colors.pressedColor = new Color(0.13f, 0.22f, 0.36f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            GameObject textGo = CreateUIObject("Text", buttonGo.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Text text = textGo.AddComponent<Text>();
            text.font = font;
            text.fontSize = 30;
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

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                return font;
            }

            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void EnsureEventSystemInputCompatibility()
        {
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            }

            Type inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                StandaloneInputModule old = eventSystem.GetComponent<StandaloneInputModule>();
                if (old != null)
                {
                    Destroy(old);
                }

                if (eventSystem.GetComponent(inputSystemModuleType) == null)
                {
                    eventSystem.gameObject.AddComponent(inputSystemModuleType);
                }
            }
            else if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
        }

        private static bool ReadTogglePausePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}
