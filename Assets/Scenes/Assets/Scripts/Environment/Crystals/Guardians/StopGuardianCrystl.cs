using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Crystals
{
    internal class StopGuardianCrystl : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float activeDuration = 5f;
        [SerializeField] private float cooldownDuration = 3f;
        [SerializeField] private Light pointLight;

        [Header("Light Colors")]
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color cooldownColor = Color.red;
        [SerializeField] private float blinkInterval = 1f;

        public bool state;

        private float _activeTimer;
        private float _cooldownTimer;
        private bool _isOnCooldown;
        private float _blinkTimer;

        private void TurnOffLight()
        {
            if (pointLight != null)
                pointLight.enabled = false;
        }

        private void Start()
        {
            TurnOffLight();
        }

        private void Update()
        {
            if (state)
            {
                _activeTimer -= Time.deltaTime;
                if (_activeTimer <= 0f)
                    Deactivate();

                UpdateLightActive();
            }
            else if (_isOnCooldown)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0f)
                {
                    _isOnCooldown = false;
                    TurnOffLight();
                }

                UpdateLightCooldown();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (state || _isOnCooldown) return;

            if (other.CompareTag("Player") && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                state = true;
                _activeTimer = activeDuration;
                if (pointLight != null)
                {
                    pointLight.enabled = true;
                    pointLight.color = activeColor;
                }
            }
        }

        private void Deactivate()
        {
            state = false;
            _isOnCooldown = true;
            _cooldownTimer = cooldownDuration;
        }

        private void UpdateLightActive()
        {
            if (pointLight == null) return;

            _blinkTimer += Time.deltaTime;
            if (_blinkTimer >= blinkInterval)
            {
                _blinkTimer = 0f;
                pointLight.enabled = !pointLight.enabled;

                if (pointLight.enabled)
                    pointLight.color = activeColor;
            }
        }

        private void UpdateLightCooldown()
        {
            if (pointLight == null) return;

            pointLight.enabled = true;
            pointLight.color = cooldownColor;
        }
    }
}