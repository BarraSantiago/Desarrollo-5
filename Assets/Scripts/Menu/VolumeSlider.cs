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

        private const string MixerVolume = "Volume";

        private void Start()
        {
            mainMixer.GetFloat(MixerVolume, out float currentVolume);
            volumeSlider.value = Mathf.Pow(10, currentVolume / 20f); // Convert dB to linear value
            volumeSlider.onValueChanged.AddListener(SetVolume);
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
        }
    }
}