using UnityEngine;
using DungeonPrototype.Core;
using DungeonPrototype.Environment;

namespace DungeonPrototype.Mana
{
    public class ManaCrystal : MonoBehaviour
    {
        [Header("Mana")]
        [SerializeField] private float maxMana = 30f;

        [Header("Threat")]
        [SerializeField] private float humNoiseRadius = 12f;
        [SerializeField] private float humThreat = 0.3f;
        [SerializeField] private float depletedThreat = 1f;

        [Header("Visual")]
        [SerializeField] private Renderer crystalRenderer;
        [SerializeField] private Light crystalPointLight;
        [SerializeField] private Color activeEmissionColor = Color.cyan;
        [SerializeField] private Color drainingEmissionColor = Color.white;
        [SerializeField] private Color depletedEmissionColor = new Color(0.03f, 0.03f, 0.03f, 1f);

        [Header("Depletion")]
        [SerializeField] private bool disableCollidersOnDepleted = true;
        [SerializeField] private bool removeFromCrystalLayerOnDepleted = true;

        public float CurrentMana { get; private set; }
        public float MaxMana => maxMana;
        public bool IsDepleted => CurrentMana <= 0.01f;

        private bool _isDraining;
        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            CurrentMana = maxMana;
            _mpb = new MaterialPropertyBlock();

            // Ensure runtime light fading exists even in scenes created before builder updates.
            if (GetComponent<LightManager>() == null)
            {
                gameObject.AddComponent<LightManager>();
            }

            if (crystalPointLight == null)
            {
                crystalPointLight = GetComponentInChildren<Light>(true);
            }

            UpdateEmission(false);
            UpdateCrystalLight();
        }

        public void BeginDrain(float intensity = 1f)
        {
            if (IsDepleted)
            {
                return;
            }

            _isDraining = true;
            UpdateEmission(true);
            GameEvents.RaiseCrystalDrainStarted(this, Mathf.Clamp01(intensity));
        }

        public float Drain(float amount)
        {
            if (!_isDraining || IsDepleted || amount <= 0f)
            {
                return 0f;
            }

            float before = CurrentMana;
            CurrentMana = Mathf.Max(0f, CurrentMana - amount);
            float drained = before - CurrentMana;

            if (drained > 0f)
            {
                float normalizedDrained = 1f - (CurrentMana / maxMana);
                GameEvents.RaiseCrystalDrainProgress(this, normalizedDrained, drained);
                GameEvents.RaiseNoise(transform.position, humNoiseRadius, humThreat);

                if (IsDepleted)
                {
                    _isDraining = false;
                    UpdateEmission(false);
                    UpdateCrystalLight();
                    SetInactiveAfterDepletion();
                    GameEvents.RaiseCrystalDepleted(this);
                    GameEvents.RaiseCrystalDrainEnded(this, maxMana, true);
                    GameEvents.RaiseNoise(transform.position, humNoiseRadius * 1.5f, depletedThreat);
                }
            }

            return drained;
        }

        public void EndDrain(float totalDrained)
        {
            if (!_isDraining)
            {
                return;
            }

            _isDraining = false;
            UpdateEmission(false);

            bool depleted = IsDepleted;
            GameEvents.RaiseCrystalDrainEnded(this, totalDrained, depleted);

            if (!depleted && totalDrained > 0f)
            {
                GameEvents.RaiseNoise(transform.position, humNoiseRadius, humThreat * 0.8f);
            }
        }

        private void UpdateEmission(bool isDraining)
        {
            if (crystalRenderer == null)
            {
                UpdateCrystalLight();
                return;
            }

            if (IsDepleted)
            {
                crystalRenderer.GetPropertyBlock(_mpb);
                _mpb.SetColor("_EmissionColor", depletedEmissionColor);
                crystalRenderer.SetPropertyBlock(_mpb);
                UpdateCrystalLight();
                return;
            }

            float manaRatio = Mathf.Clamp01(CurrentMana / maxMana);
            Color baseColor = Color.Lerp(Color.black, activeEmissionColor, manaRatio);
            Color finalEmission = isDraining ? Color.Lerp(baseColor, drainingEmissionColor, 0.7f) : baseColor;

            crystalRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor("_EmissionColor", finalEmission);
            crystalRenderer.SetPropertyBlock(_mpb);
            UpdateCrystalLight();
        }

        private void UpdateCrystalLight()
        {
            if (crystalPointLight == null)
            {
                return;
            }

            if (IsDepleted)
            {
                if (GetComponent<LightManager>() != null || GetComponent<AdvancedLightFading>() != null)
                {
                    return;
                }

                crystalPointLight.enabled = false;
                return;
            }

            crystalPointLight.enabled = true;
            float manaRatio = Mathf.Clamp01(CurrentMana / Mathf.Max(0.001f, maxMana));
            crystalPointLight.intensity = Mathf.Lerp(0.6f, 2f, manaRatio);
        }

        private void SetInactiveAfterDepletion()
        {
            if (disableCollidersOnDepleted)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = false;
                }
            }

            if (removeFromCrystalLayerOnDepleted)
            {
                gameObject.layer = 0;
            }
        }
    }
}
