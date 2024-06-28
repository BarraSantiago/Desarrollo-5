using UnityEngine;

namespace Interactable
{
    public class OpenMenu : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject menu;
        
        public bool Interact()
        {
            menu?.SetActive(!menu.activeSelf);
            return menu;
        }
    }
}