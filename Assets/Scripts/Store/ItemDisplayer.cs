using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Store
{
    public class ItemDisplayer : MonoBehaviour
    {
        [SerializeField] private Transform[] objPostition;
        [SerializeField] private TextMeshPro[] texts;

        [FormerlySerializedAs("items")] public DisplayItem[] displayItems;

        public void Initialize(InventoryObject[] storeInventories)
        {
            StoreManager.OnEndCycle += Deinitialize;
            Client.ItemGrabbed += RemoveItem;

            for (int i = 0; i < displayItems.Length; i++)
            {
                displayItems[i].inventory = storeInventories[i];
                displayItems[i].itemPosition = objPostition[i];
                displayItems[i].showPrice = texts[i];
            }

            UpdateInventory();
        }

        private void Deinitialize()
        {
            StoreManager.OnEndCycle -= Deinitialize;
        }

        private void OnDestroy()
        {
            Client.ItemGrabbed -= RemoveItem;
        }

        private void RemoveItem(int id, int amount)
        {
            if (!displayItems[id]) return;

            displayItems[id].amount = 0;
            if (displayItems[id].amount <= 0) displayItems[id].CleanDisplay();
            UpdateInventory();
        }
        
        private void UpdateInventory()
        {
            foreach (var displayItem in displayItems)
            {
                displayItem.Initialize();
            }
        }
    }
}