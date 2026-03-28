using DungeonPrototype.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DungeonPrototype.Environment
{
    [RequireComponent(typeof(Collider))]
    public class StartLocationPortal : MonoBehaviour
    {
        [SerializeField] private string targetScene = "DungeonPrototype_Prototype";
        [SerializeField] private bool requireInteractKey = true;

        private bool _playerInside;

        private void Awake()
        {
            Collider c = GetComponent<Collider>();
            if (c != null)
            {
                c.isTrigger = true;
            }
        }

        private void Update()
        {
            if (!_playerInside)
            {
                return;
            }

            if (requireInteractKey && !IsInteractPressed())
            {
                return;
            }

            LoadTargetScene();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerHealth>() != null)
            {
                _playerInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerHealth>() != null)
            {
                _playerInside = false;
            }
        }

        private void LoadTargetScene()
        {
            if (!string.IsNullOrWhiteSpace(targetScene) && Application.CanStreamedLevelBeLoaded(targetScene))
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
                SceneManager.LoadScene(targetScene);
                return;
            }

            Debug.LogError("Target scene is not in Build Settings: " + targetScene);
        }

        private static bool IsInteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }
    }
}
