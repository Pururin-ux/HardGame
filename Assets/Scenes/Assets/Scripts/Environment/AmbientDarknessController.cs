using UnityEngine;
using UnityEngine.Rendering;

namespace DungeonPrototype.Environment
{
    public class AmbientDarknessController : MonoBehaviour
    {
        [SerializeField] private Color ambientColor = Color.black;
        [SerializeField] private float ambientIntensity = 0f;
        [SerializeField] private float reflectionIntensity = 0f;

        private void OnEnable()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.reflectionIntensity = reflectionIntensity;
        }
    }
}
