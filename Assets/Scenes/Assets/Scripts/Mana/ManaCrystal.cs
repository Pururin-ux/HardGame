using UnityEngine;
using DungeonPrototype.Core;

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
        [SerializeField] private Color activeEmissionColor = Color.cyan;
        [SerializeField] private Color drainingEmissionColor = Color.white;

        public float CurrentMana { get; private set; }
        public float MaxMana => maxMana;
        public bool IsDepleted => CurrentMana <= 0.01f;

        private bool _isDraining;
        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            CurrentMana = maxMana;
            _mpb = new MaterialPropertyBlock();
            UpdateEmission(false);
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
                return;
            }

            float manaRatio = Mathf.Clamp01(CurrentMana / maxMana);
            Color baseColor = Color.Lerp(Color.black, activeEmissionColor, manaRatio);
            Color finalEmission = isDraining ? Color.Lerp(baseColor, drainingEmissionColor, 0.7f) : baseColor;

            crystalRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor("_EmissionColor", finalEmission);
            crystalRenderer.SetPropertyBlock(_mpb);
        }
    }
}
