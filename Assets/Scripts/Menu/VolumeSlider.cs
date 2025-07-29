using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Menu
{
    /// <summary>
    /// The VolumeSlider class manages the volume control.
    /// </summary>
    public class VolumeSlider : MonoBehaviour
    {
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle volumeToggle;

        private const string MixerVolume = "Volume";
        private float _lastVolume;
        private bool _isMuted;
        
        private void Start()
        {
            _lastVolume = PlayerPrefs.GetFloat(MixerVolume, 0);
            _isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
            volumeToggle.isOn = _isMuted;
            
            if (_isMuted)
            {
                mainMixer.SetFloat(MixerVolume, -80);
            }
            else
            {
                mainMixer.SetFloat(MixerVolume, _lastVolume);
            }
            
            volumeSlider.value = Mathf.Pow(10, _lastVolume / 20f);
            volumeSlider.onValueChanged.AddListener(SetVolume);
            volumeToggle.onValueChanged.AddListener(ToggleVolume);
            _lastVolume = volumeSlider.value;
        }

        private void ToggleVolume(bool arg0)
        {
            mainMixer.SetFloat(MixerVolume, !arg0 ? _lastVolume : -80);
            _isMuted = arg0;
            PlayerPrefs.SetInt("IsMuted", _isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Changes the game's volume
        /// </summary>
        /// <param name="volume"> new volume </param>
        public void SetVolume(float volume)
        {
            if(volume <= 0.0001f) volume = 0.0001f;
            float dB = 20f * Mathf.Log10(volume);
            mainMixer.SetFloat(MixerVolume, dB);
            _lastVolume = dB;
            PlayerPrefs.SetFloat(MixerVolume, _lastVolume);
        }
    }
}