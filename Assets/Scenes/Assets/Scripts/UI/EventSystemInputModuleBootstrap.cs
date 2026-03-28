using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace DungeonPrototype.UI
{
    public static class EventSystemInputModuleBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyOnStartup()
        {
            FixAllEventSystems();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            FixAllEventSystems();
        }

        private static void FixAllEventSystems()
        {
            EventSystem[] eventSystems = Object.FindObjectsOfType<EventSystem>(true);
            for (int i = 0; i < eventSystems.Length; i++)
            {
                FixEventSystem(eventSystems[i]);
            }
        }

        private static void FixEventSystem(EventSystem eventSystem)
        {
            if (eventSystem == null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (oldModule != null)
            {
                Object.Destroy(oldModule);
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }
    }
}
