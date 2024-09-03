using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class LoadNextLevel : MonoBehaviour
    {
        private readonly int sceneId = 2;
        
        private void OnDestroy()
        {
            SceneManager.LoadScene(sceneId);
        }
    }
}