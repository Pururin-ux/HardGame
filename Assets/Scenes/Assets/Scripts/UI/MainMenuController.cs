using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonPrototype.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string[] gameplayScenes = { "DungeonPrototype_Start", "DungeonPrototype_Prototype", "SampleScene" };

        public void StartNewGame()
        {
            for (int i = 0; i < gameplayScenes.Length; i++)
            {
                string sceneName = gameplayScenes[i];
                if (!string.IsNullOrWhiteSpace(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
                {
                    SceneManager.LoadScene(sceneName);
                    return;
                }
            }

            Debug.LogError("No gameplay scene is available in Build Settings. Add DungeonPrototype_Prototype or SampleScene.");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
