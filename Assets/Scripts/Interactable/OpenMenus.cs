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
        [SerializeField] private bool isSellingPoint;
        private GameObject InventoryGO;

        private void Awake()
        {
            InventoryGO = menus[0].transform.GetChild(0).gameObject;
        }

        public bool State
        {
            get { return menus.Any(menu => menu.activeSelf); }
        }

        public bool Interact()
        {
            foreach (GameObject menu in menus)
            {
                menu?.SetActive(true);
            }

            if (isSellingPoint) InventoryGO.transform.SetParent(playerInventoryUi.transform);
            playerInventoryUi.SetActive(true);
            //playerInventoryUi.transform.position = inventoryNewTransform.position;
            playerInventory.UpdateInventory();

            return true;
        }

        public void Close()
        {
            foreach (GameObject menu in menus)
            {
                menu?.SetActive(false);
            }

            if (isSellingPoint) InventoryGO.transform.SetParent(menus[0].transform);
            playerInventoryUi.SetActive(false);
            //playerInventoryUi.transform.position = inventoryOriginalTransform.position;
        }
    }
}