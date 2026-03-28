using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DungeonPrototype.Core;
using DungeonPrototype.Dragon;

namespace DungeonPrototype.Environment
{
    /// <summary>
    /// Drives chromatic aberration intensity based on dragon hunger and darkness.
    /// </summary>
    public class HungerVisualEffects : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FogAndPostProcessingSetup postProcessing;
        [SerializeField] private DragonHungerSystem dragonHunger;

        [Header("Aberration Thresholds")]
        [SerializeField] private float lowHungerThreshold = 0.3f;
        [SerializeField] private float criticalHungerThreshold = 0.7f;
        [SerializeField] private float maxAberrationIntensity = 3.0f;

        private float _currentAberrationIntensity;

        private void OnEnable()
        {
            GameEvents.DragonManaChanged += OnDragonManaChanged;
        }

        private void OnDisable()
        {
            GameEvents.DragonManaChanged -= OnDragonManaChanged;
        }

        private void OnDragonManaChanged(float currentMana, float maxMana, float delta)
        {
            float safeMax = Mathf.Max(0.001f, maxMana);
            float hungerRatio = 1f - Mathf.Clamp01(currentMana / safeMax);
            UpdateAberration(hungerRatio);
        }

        private void UpdateAberration(float hungerRatio)
        {
            float hungerIntensity = 0f;

            if (hungerRatio >= criticalHungerThreshold)
            {
                hungerIntensity = maxAberrationIntensity * (hungerRatio - criticalHungerThreshold) / Mathf.Max(0.001f, 1f - criticalHungerThreshold);
            }
            else if (hungerRatio >= lowHungerThreshold)
            {
                hungerIntensity = (hungerRatio - lowHungerThreshold) / Mathf.Max(0.001f, criticalHungerThreshold - lowHungerThreshold) * maxAberrationIntensity * 0.5f;
            }

            float lightIntensity = GetEnvironmentLightIntensity();
            float combinedAberration = hungerIntensity + (1f - lightIntensity) * maxAberrationIntensity * 0.3f;

            if (postProcessing != null && !Mathf.Approximately(combinedAberration, _currentAberrationIntensity))
            {
                _currentAberrationIntensity = combinedAberration;
                postProcessing.SetChromaticAberrationIntensity(_currentAberrationIntensity);
            }
        }

        private float GetEnvironmentLightIntensity()
        {
            Light[] lights = FindObjectsOfType<Light>();
            if (lights.Length == 0)
            {
                return 0f;
            }

            float totalIntensity = 0f;
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light.enabled)
                {
                    totalIntensity += light.intensity;
                }
            }

            totalIntensity += RenderSettings.ambientLight.grayscale;
            return Mathf.Clamp01(totalIntensity / (lights.Length + 1f));
        }
    }
}
