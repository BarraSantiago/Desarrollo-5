using InventorySystem;
using TMPro;
using UnityEngine;

namespace Store
{
    public class ItemDisplayer : MonoBehaviour
    {
        [SerializeField] private Transform[] objPostition;
        [SerializeField] private TextMeshPro[] texts;

        public static DisplayItem[] DisplayItems;

        public void Initialize(InventoryObject[] storeInventories)
        {
            Client.ItemGrabbed += RemoveItem;

            for (int i = 0; i < DisplayItems.Length; i++)
            {
                DisplayItems[i].inventory = storeInventories[i];
                DisplayItems[i].itemPosition = objPostition[i];
                DisplayItems[i].showPrice = texts[i];
            }

            UpdateInventory();
        }

        private void Deinitialize()
        {
        }

        private void OnDestroy()
        {
            Client.ItemGrabbed -= RemoveItem;
        }

        private void RemoveItem(int id, int amount)
        {
            if (!DisplayItems[id]) return;

            DisplayItems[id].CleanDisplay();
            UpdateInventory();
        }
        
        private void UpdateInventory()
        {
            foreach (var displayItem in DisplayItems)
            {
                displayItem.Initialize();
            }
        }
    }
}