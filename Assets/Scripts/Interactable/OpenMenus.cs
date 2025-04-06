using System.Linq;
using InventorySystem;
using UnityEngine;

namespace Interactable
{
    public class OpenMenus : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject[] menus;
        [SerializeField] private GameObject playerInventoryUi;
        [SerializeField] private GameObject itemInfoUi;
        [SerializeField] private InventoryObject playerInventory;
        [SerializeField] private bool isSellingPoint;
        private GameObject InventoryGO;

        private const string AudioKey = "OpenMenu";

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

            AudioManager.instance.Play(AudioKey);
            if (isSellingPoint)
            {
                InventoryGO.transform.SetParent(playerInventoryUi.transform);
                playerInventoryUi.SetActive(true);
                itemInfoUi.SetActive(false);
                playerInventory.UpdateInventory();
            }

            return true;
        }

        public void Close()
        {
            foreach (GameObject menu in menus)
            {
                menu?.SetActive(false);
            }

            if (isSellingPoint)
            {
                InventoryGO.transform.SetParent(menus[0].transform);
                itemInfoUi.SetActive(true);
                playerInventoryUi.SetActive(false);
            }
        }
    }
}