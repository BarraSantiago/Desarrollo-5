using System.Linq;
using InventorySystem;
using UnityEngine;

namespace Interactable
{
    public class OpenMenus : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject[] menus;
        [SerializeField] private GameObject playerInventoryUi;
        [SerializeField] private InventoryObject playerInventory;
        [SerializeField] private Transform inventoryOriginalTransform;
        [SerializeField] private Transform inventoryNewTransform;
        
        public bool State
        {
            get
            {
                return menus.Any(menu => menu.activeSelf);
            }
        }

        public bool Interact()
        {
            foreach (var menu in menus)
            {
                menu?.SetActive(!menu.activeSelf);
            }
            playerInventoryUi.SetActive(true);
            playerInventoryUi.transform.position = inventoryNewTransform.position;
            playerInventory.UpdateInventory();
            return true;
        }

        public void Close()
        {
            foreach (var menu in menus)
            {
                menu?.SetActive(false);
            }
            playerInventoryUi.SetActive(false);
            playerInventoryUi.transform.position = inventoryOriginalTransform.position;
        }
    }
}