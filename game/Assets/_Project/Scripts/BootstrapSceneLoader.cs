using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoralCritter
{
    /// <summary>
    /// Minimal bootstrap behavior used by the prototype scaffold.
    /// </summary>
    public class BootstrapSceneLoader : MonoBehaviour
    {
        [SerializeField] private string firstScene = "IslandPrototype";

        private void Start()
        {
            if (!string.IsNullOrWhiteSpace(firstScene))
            {
                SceneManager.LoadScene(firstScene, LoadSceneMode.Single);
            }
        }
    }
}
