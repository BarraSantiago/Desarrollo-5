using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace Menu
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text versionText = null;
        [SerializeField] private Button continueButton = null;
        [SerializeField] private Button newGameButton = null;
        [SerializeField] private Button confirmationButton = null;
        [SerializeField] private GameObject confirmationWindow = null;
        private const string SceneName = "LoadingScene";

        public void Start()
        {
            if (versionText) versionText.text = "Ver " + Application.version;
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            continueButton.interactable = SaveFileManager.HasSavedFiles();
            confirmationWindow.SetActive(false);
            if (SaveFileManager.HasSavedFiles())
            {
                continueButton.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene(SceneName);
                });
                newGameButton.onClick.AddListener(() =>
                {
                    confirmationWindow.SetActive(true);
                });
                confirmationButton.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt("PopularityXp", 0);
                    PlayerPrefs.SetInt("PopularityLevel", 0);
                    PlayerPrefs.SetInt("ShopLevel", 1);
                    PlayerPrefs.SetInt("HasInteracted", 0);
                    PlayerPrefs.SetInt("PlayerMoney", 250);
                    PlayerPrefs.SetInt("TutorialAccessed", 0);
                    PlayerPrefs.Save();
                    SaveFileManager.DeleteAllSaves();
                    SceneManager.LoadScene(SceneName);
                });
            }
            else
            {
                newGameButton.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene(SceneName);
                });
            }
        }

        /// <summary>
        /// Quits the application
        /// </summary>
        public void QuitApplication()
        {
            Application.Quit();
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

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}