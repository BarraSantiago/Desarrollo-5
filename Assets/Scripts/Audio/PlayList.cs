using UnityEngine;

namespace Audio
{
    public class PlayList : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] playList;

        private int _currentSongIndex = 0;
        private bool _wasPlaying;

        private void Start()
        {
            audioSource.clip = playList[_currentSongIndex];
            audioSource.Play();
        }

        private void Update()
        {
            if (!audioSource.isPlaying && _wasPlaying)
            {
                _currentSongIndex++;
                if (_currentSongIndex >= playList.Length)
                {
                    _currentSongIndex = 0;
                }

                audioSource.clip = playList[_currentSongIndex];
                audioSource.Play();
            }

            _wasPlaying = audioSource.isPlaying;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                _wasPlaying = audioSource.isPlaying;
            }
        }
    }
}