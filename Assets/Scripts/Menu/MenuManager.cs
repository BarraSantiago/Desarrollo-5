using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text versionText = null;
        private const string SceneName = "LoadingScene";
        
        public void Start()
        {
            if (versionText != null) versionText.text = "Ver " + Application.version;
        }
        /// <summary>
        /// Quits the application
        /// </summary>
        public void QuitApplication()
        {
            Application.Quit();
        }

        /// <summary>
        /// Loads the scene saved in SceneName.
        /// </summary>
        public void LoadSceneByName()
        {
            SceneManager.LoadScene(SceneName);
        }
        
        /// <summary>
        /// Loads the required scene by the Id introduced.
        /// </summary>
        public void LoadSceneById(int id)
        {
            SceneManager.LoadScene(id);
        }

        public void OpenMenu(GameObject menu)
        {
            menu.SetActive(!menu.activeSelf);
        }
    }
}