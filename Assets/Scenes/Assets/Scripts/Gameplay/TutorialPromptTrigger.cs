using DungeonPrototype.Player;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonPrototype.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class TutorialPromptTrigger : MonoBehaviour
    {
        [SerializeField] private Text promptText;
        [SerializeField] private string enterMessage = "Вытянуть ману";
        [SerializeField] private string idleMessage = string.Empty;

        private void Awake()
        {
            Collider c = GetComponent<Collider>();
            if (c != null)
            {
                c.isTrigger = true;
            }

            SetPrompt(idleMessage);
            if (promptText != null)
            {
                promptText.enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerHealth>() == null)
            {
                return;
            }

            SetPrompt(enterMessage);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerHealth>() == null)
            {
                return;
            }

            SetPrompt(idleMessage);
            if (promptText != null)
            {
                promptText.enabled = false;
            }
        }

        private void SetPrompt(string text)
        {
            if (promptText == null)
            {
                return;
            }

            promptText.text = text;
            promptText.enabled = !string.IsNullOrWhiteSpace(text);
        }
    }
}
