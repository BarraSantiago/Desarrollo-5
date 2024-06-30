using UnityEngine;

namespace Interactable
{
    public class OpenMenu : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject menu;
        
        public bool State => menu.activeSelf;

        public bool Interact()
        {
            menu?.SetActive(!menu.activeSelf);
            return menu;
        }

        public void Close()
        {
            menu?.SetActive(false);
        }
    }
}