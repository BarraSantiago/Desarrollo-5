using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private Toggle fullscreenToggle;
        public Dropdown resDropdown;
        private const string FullscreenKey = "fullscreen";
        private const string ResWidthKey = "width";
        private const string ResHeightKey = "height";
        private Resolution[] resolutions;
        private Resolution fullscreen = default;
        private Resolution window = default;


        private void Start()
        {
            Init();
        }


        public void SetResolution(int resolutionId)
        {
            Resolution res = resolutions[resolutionId];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt(ResWidthKey, res.width);
            PlayerPrefs.SetInt(ResHeightKey, res.height);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            if (isFullscreen)
            {
                PlayerPrefs.SetInt(FullscreenKey, 1);
                Screen.SetResolution(fullscreen.width, fullscreen.height, true);
                PlayerPrefs.SetInt(ResWidthKey, fullscreen.width);
                PlayerPrefs.SetInt(ResHeightKey, fullscreen.height);
            }
            else
            {
                PlayerPrefs.SetInt(FullscreenKey, 0);
                Screen.SetResolution(window.width, window.height, false);
                PlayerPrefs.SetInt(ResWidthKey, window.width);
                PlayerPrefs.SetInt(ResHeightKey, window.height);
            }
        }

        public void SetQuality(int qualityId)
        {
            QualitySettings.SetQualityLevel(qualityId);
        }

        private void Init()
        {
            fullscreen.width = 1920;
            fullscreen.height = 1080;
            window.width = 1280;
            window.height = 720;

            InitSettingsValues();
        }
        
        void InitSettingsValues()
        {
            if (PlayerPrefs.HasKey(FullscreenKey))
            {
                Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey) == 1;
            }
            else
            {
                PlayerPrefs.SetInt(FullscreenKey, Screen.fullScreen ? 1 : 0);
            }

            fullscreenToggle.isOn = Screen.fullScreen;
            if (PlayerPrefs.HasKey(ResWidthKey) && PlayerPrefs.HasKey(ResHeightKey))
            {
                Screen.SetResolution(PlayerPrefs.GetInt(ResWidthKey), PlayerPrefs.GetInt(ResHeightKey),
                    Screen.fullScreen);
            }
            else
            {
                PlayerPrefs.SetInt(ResWidthKey, Screen.fullScreen ? fullscreen.width : window.width);
                PlayerPrefs.SetInt(ResHeightKey, Screen.fullScreen ? fullscreen.height : window.height);
            }
        }
    }
}