using UnityEngine;

namespace Tutorial
{
    public class CheckClose : MonoBehaviour
    {
        private bool _hasBeenAccessed;
        private const string HasSeenTutorial = "HasSeenTutorial";
        private void Awake()
        {
            _hasBeenAccessed = PlayerPrefs.GetInt(HasSeenTutorial, 0) == 1;
            if (!_hasBeenAccessed)
            {
                _hasBeenAccessed = true;
                PlayerPrefs.SetInt(HasSeenTutorial, 1);
                PlayerPrefs.Save();
            }
            else
            {
                Close();
            }
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }
    }
}