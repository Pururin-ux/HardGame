using System;
using UnityEngine;
using UnityEngine.UI;
using DungeonPrototype.Core;

namespace Game
{
    public class PlayerHPSlider : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private float animationSpeed = 5f;

        private float _currentDisplayHealth;
        private float _targetHealth;
        private float _maxHealth;

        private void Awake()
        {
            if (healthSlider == null)
            {
                healthSlider = GetComponent<Slider>();

                if (healthSlider == null)
                {
                    Debug.LogError("PlayerHPSlider: No Slider component found!");
                    return;
                }
            }
        }

        private void OnEnable()
        {
            GameEvents.PlayerHealthChanged += OnPlayerHealthChanged;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHealthChanged -= OnPlayerHealthChanged;
        }

        private void Update()
        {
            if (healthSlider == null) return;

            if (Math.Abs(_currentDisplayHealth - _targetHealth) > 0.01f)
            {
                _currentDisplayHealth = Mathf.Lerp(_currentDisplayHealth, _targetHealth, Time.deltaTime * animationSpeed);
                healthSlider.value = _currentDisplayHealth / _maxHealth;
            }
        }

        private void OnPlayerHealthChanged(float currentHealth, float maxHealth)
        {
            _targetHealth = currentHealth;
            _maxHealth = maxHealth;

            // Initialize display value if this is the first update
            if (_currentDisplayHealth == 0 && _targetHealth > 0)
            {
                _currentDisplayHealth = _targetHealth;
                healthSlider.value = _currentDisplayHealth / _maxHealth;
            }
        }
    }
}