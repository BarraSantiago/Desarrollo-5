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
        private float lastVolume;
        private void Start()
        {
            lastVolume = PlayerPrefs.GetFloat(MixerVolume, 0);
            mainMixer.SetFloat(MixerVolume, lastVolume);
            volumeSlider.value = Mathf.Pow(10, lastVolume / 20f); // Convert dB to linear value
            volumeSlider.onValueChanged.AddListener(SetVolume);
            volumeToggle.onValueChanged.AddListener(ToggleVolume);
            lastVolume = volumeSlider.value;
        }

        private void ToggleVolume(bool arg0)
        {
            mainMixer.SetFloat(MixerVolume, !arg0 ? lastVolume : -80);
        }

        /// <summary>
        /// Changes the game's volume
        /// </summary>
        /// <param name="volume"> new volume </param>
        public void SetVolume(float volume)
        {
            if(volume <= 0.0001f) volume = 0.0001f;
            float dB = 20f * Mathf.Log10(volume); // Convert linear value to dB
            mainMixer.SetFloat(MixerVolume, dB);
            lastVolume = dB;
            PlayerPrefs.SetFloat(MixerVolume, lastVolume);
        }
    }
}