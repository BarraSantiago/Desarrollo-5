using System;
using UnityEngine;

namespace Interactable
{
    public class OpenMenu : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject menu;

        public Action OnInteract { get; set; }
        public bool HasBeenAccessed { get; private set; }
        public bool State => menu.activeSelf;

        private void Awake()
        {
            HasBeenAccessed = PlayerPrefs.GetInt("HasInteracted", 0) == 1;
        }

        public bool Interact()
        {
            menu?.SetActive(!menu.activeSelf);
            OnInteract?.Invoke();
            if (!HasBeenAccessed)
            {
                HasBeenAccessed = true;
                PlayerPrefs.SetInt("HasInteracted", 1);
                PlayerPrefs.Save();
            }
            return menu;
        }

        public void Close()
        {
            menu?.SetActive(false);
        }
    }
}