using System.Linq;
using InventorySystem;
using UnityEngine;

namespace Interactable
{
    public class OpenMenus : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject[] menus;
        [SerializeField] private GameObject playerInventory;
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
            playerInventory.SetActive(true);
            playerInventory.GetComponent<InventoryObject>().UpdateInventory();
            playerInventory.transform.position = inventoryNewTransform.position;
            return true;
        }

        public void Close()
        {
            foreach (var menu in menus)
            {
                menu?.SetActive(false);
            }
            playerInventory.transform.position = inventoryOriginalTransform.position;
        }
    }
}