using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DungeonPrototype.Environment
{
    /// <summary>
    /// Configures global fog and URP post-processing volume.
    /// </summary>
    public class FogAndPostProcessingSetup : MonoBehaviour
    {
        [Header("Fog Settings")]
        [SerializeField] private Color fogColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        [SerializeField] private float fogDensity = 0.05f;
        [SerializeField] private FogMode fogMode = FogMode.Exponential;

        [Header("Post-Processing")]
        [SerializeField] private float baseChromaticAberration = 0.2f;
        [SerializeField] private float maxChromaticAberration = 3.0f;
        [SerializeField] private float bloomIntensity = 3.0f;
        [SerializeField] private float bloomThreshold = 0.8f;
        [SerializeField] private float bloomScatter = 0.85f;
        [SerializeField] private float vignetteIntensity = 0.52f;
        [SerializeField] private float vignetteSmoothness = 0.7f;
        [SerializeField] private Color colorFilter = new Color(0.78f, 0.9f, 1f, 1f);
        [SerializeField] private float postExposure = -0.25f;
        [SerializeField] private float saturation = -20f;
        [SerializeField] private float contrast = 18f;

        private Volume _globalVolume;
        private VolumeProfile _runtimeProfile;
        private ChromaticAberration _chromaticAberration;
        private Vignette _vignette;
        private Bloom _bloom;
        private ColorAdjustments _colorAdjustments;

        private void OnEnable()
        {
            SetupGlobalFog();
            SetupPostProcessing();
        }

        private void SetupGlobalFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogMode = fogMode;
        }

        private void SetupPostProcessing()
        {
            _globalVolume = GetComponent<Volume>();
            if (_globalVolume == null)
            {
                _globalVolume = gameObject.AddComponent<Volume>();
            }

            _globalVolume.isGlobal = true;
            _globalVolume.priority = 0f;

            if (_globalVolume.profile == null)
            {
                _runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                _globalVolume.profile = _runtimeProfile;
            }
            else
            {
                _runtimeProfile = _globalVolume.profile;
            }

            if (!_runtimeProfile.TryGet<ChromaticAberration>(out _chromaticAberration))
            {
                _chromaticAberration = _runtimeProfile.Add<ChromaticAberration>(true);
            }

            if (!_runtimeProfile.TryGet<Vignette>(out _vignette))
            {
                _vignette = _runtimeProfile.Add<Vignette>(true);
            }

            if (!_runtimeProfile.TryGet<Bloom>(out _bloom))
            {
                _bloom = _runtimeProfile.Add<Bloom>(true);
            }

            if (!_runtimeProfile.TryGet<ColorAdjustments>(out _colorAdjustments))
            {
                _colorAdjustments = _runtimeProfile.Add<ColorAdjustments>(true);
            }

            _chromaticAberration.active = true;
            _chromaticAberration.intensity.Override(Mathf.Clamp(baseChromaticAberration, 0f, 1f));

            _vignette.active = true;
            _vignette.intensity.Override(Mathf.Clamp01(vignetteIntensity));
            _vignette.smoothness.Override(Mathf.Clamp01(vignetteSmoothness));
            _vignette.rounded.Override(false);

            _bloom.active = true;
            _bloom.intensity.Override(Mathf.Max(0f, bloomIntensity));
            _bloom.threshold.Override(Mathf.Max(0f, bloomThreshold));
            _bloom.scatter.Override(Mathf.Clamp01(bloomScatter));

            _colorAdjustments.active = true;
            _colorAdjustments.colorFilter.Override(colorFilter);
            _colorAdjustments.postExposure.Override(postExposure);
            _colorAdjustments.saturation.Override(saturation);
            _colorAdjustments.contrast.Override(contrast);
        }

        public void SetChromaticAberrationIntensity(float intensity)
        {
            if (_runtimeProfile == null)
            {
                SetupPostProcessing();
            }

            if (_runtimeProfile != null && _runtimeProfile.TryGet<ChromaticAberration>(out _chromaticAberration))
            {
                float normalized = Mathf.Clamp01(intensity / Mathf.Max(0.001f, maxChromaticAberration));
                _chromaticAberration.intensity.Override(normalized);
            }
        }

        public void UpdateFogDensity(float density)
        {
            RenderSettings.fogDensity = density;
        }

        public void UpdateFogColor(Color color)
        {
            RenderSettings.fogColor = color;
        }
    }
}
