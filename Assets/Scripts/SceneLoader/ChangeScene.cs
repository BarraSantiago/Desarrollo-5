using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneLoader
{
    public class ChangeScene : MonoBehaviour
    {
        [SerializeField] private string sceneName;

        public void LoadScene()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}