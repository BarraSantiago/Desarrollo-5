using System;
using System.Collections;
using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace Menu
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text versionText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button confirmationButton;
        [SerializeField] private GameObject confirmationWindow;
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

            continueButton.onClick.RemoveAllListeners();
            newGameButton.onClick.RemoveAllListeners();
            confirmationButton.onClick.RemoveAllListeners();

            if (SaveFileManager.HasSavedFiles())
            {
                continueButton.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene(SceneName);
                    SaveFileManager.ResetGame = false;
                });
                newGameButton.onClick.AddListener(() => { confirmationWindow.SetActive(true); });
                confirmationButton.onClick.AddListener(ResetGame);
            }
            else
            {
                newGameButton.onClick.AddListener(() =>
                {
                    InventoryObject[] allInventories = Resources.FindObjectsOfTypeAll<InventoryObject>();
                
                    foreach (InventoryObject inventory in allInventories)
                    {
                        inventory.RemoveAllItems();
                    }
                    SaveFileManager.ResetGame = true;
                    ResetGame();
                    SceneManager.LoadScene(SceneName);
                });
            }
        }

        public void ResetGame()
        {
            try
            {
                Debug.Log("Resetting game data...");
                PlayerPrefs.SetInt("PopularityXp", 0);
                PlayerPrefs.SetInt("PopularityLevel", 0);
                PlayerPrefs.SetInt("ShopLevel", 1);
                PlayerPrefs.SetInt("HasInteracted", 0);
                PlayerPrefs.SetInt("PlayerMoney", 250);
                PlayerPrefs.SetInt("TutorialAccessed", 0);
                PlayerPrefs.SetInt("TutorialAccessed", 0);
                PlayerPrefs.Save();
                SaveFileManager.ResetGame = true;
                InventoryObject[] allInventories = Resources.FindObjectsOfTypeAll<InventoryObject>();
                
                foreach (InventoryObject inventory in allInventories)
                {
                    inventory.RemoveAllItems();
                }

                SaveFileManager.DeleteAllSaves();

                LoadScene();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                LoadScene();
                throw;
            }
        }

        public void LoadScene()
        {
            Debug.Log("Loading scene...");
            StartCoroutine(LoadSceneAsync(SceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            yield return new WaitForSeconds(0.1f);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
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