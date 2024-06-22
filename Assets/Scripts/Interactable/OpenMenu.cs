using UnityEngine;

namespace Interactable
{
    public class OpenMenu : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject menu;
        
        public void Interact()
        {
            menu?.SetActive(!menu.activeSelf);
        }
    }
}