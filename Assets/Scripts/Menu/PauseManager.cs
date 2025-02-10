using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    /// <summary>
    /// PauseManager class manages the pause state of the game.
    /// It provides methods to pause and resume the game, change the mouse state, load scenes, and quit the application.
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        private const float NormalTimeScale = 1;
        private const float PauseTimeScale = 0;
        private const int MenuSceneIndex = 0;

        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private UnityEngine.InputSystem.PlayerInput playerInput;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Toggle fullscreenToggle;

        private const string FullscreenKey = "fullscreen";

        private void Awake()
        {
            resumeButton.onClick.AddListener(ResumeGame);
            menuButton.onClick.AddListener(LoadMenu);
            quitButton.onClick.AddListener(Quit);
            fullscreenToggle.onValueChanged.AddListener(FullscreenToggle);
            resumeButton.Select();

            FullscreenToggle(PlayerPrefs.GetInt(FullscreenKey, 1) == 1);
            
            fullscreenToggle.isOn = Screen.fullScreen;
        }

        /// <summary>
        /// Resumes the game and reactivates the gameplay inputs.
        /// </summary>
        private void ResumeGame()
        {
            ChangeMouseState(false);
            playerInput.enabled = true;
            ResumeTime();
            ChangeState();
        }

        /// <summary>
        /// Changes the state of the mouse. Locks/unlocks, visible/invisible.
        /// </summary>
        /// <param name="mouseVisible"> mouse visibility state </param>
        private static void ChangeMouseState(bool mouseVisible)
        {
            Cursor.lockState = mouseVisible ? CursorLockMode.None : CursorLockMode.Confined;
        }

        /// <summary>
        /// Loads the menu scene
        /// </summary>
        private void LoadMenu()
        {
            ResumeTime();

            SceneManager.LoadScene(MenuSceneIndex);
        }

        /// <summary>
        /// Quits the application
        /// </summary>
        private void Quit()
        {
            Application.Quit();
        }


        /// <summary>
        /// In case of game ending by win or lose event, stops time and disables this menu.
        /// </summary>
        private void OnGameEnd()
        {
            StopTime();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Activates or deactivates the pause.
        /// </summary>
        public void OnPause()
        {
            if (pauseMenu.activeSelf)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        /// <summary>
        /// Stops the game and deactivates gameplay inputs.
        /// </summary>
        private void PauseGame()
        {
            ChangeMouseState(true);
            StopTime();
            ChangeState();
            resumeButton.Select();
        }

        /// <summary>
        /// Enables or disables the player's input and the pause menu
        /// </summary>
        private void ChangeState()
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
        }

        /// <summary>
        /// Modifies time scale to PauseTimeScale
        /// </summary>
        private void StopTime()
        {
            playerInput.enabled = false;
            Time.timeScale = PauseTimeScale;
        }

        /// <summary>
        /// Modifies time scale to NormalTimeScale
        /// </summary>
        private void ResumeTime()
        {
            Time.timeScale = NormalTimeScale;
        }

        private void FullscreenToggle(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        }
    }
}