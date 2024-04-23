using System.Collections.Generic;
using InventorySystem;
using UnityEngine;

namespace UI
{
    public struct UiSlot
    {
        public int itemId;
        public int amount;
        public int slotId;
        public ItemUI slot;
    }

    public class InventoryUI : MonoBehaviour
    {
        private GameObject itemSlotPrefab;

        private Dictionary<int, Item> _items;

        private List<UiSlot> _uiItems = new List<UiSlot>();

        private int _maxPerSlot = 5;

        public void Initialize(Dictionary<int, Item> _items, GameObject itemSlotPrefab)
        {
            this._items = _items;
            this.itemSlotPrefab = itemSlotPrefab;
        }

        private void Start()
        {
            Inventory.ItemAdded += AddItem;
        }

        private void OnDestroy()
        {
            Inventory.ItemAdded -= AddItem;
        }

        /// <summary>
        /// Adds item to the visual interface of the inventory.
        /// </summary>
        /// <param name="itemId"> Item to add to the inventory. </param>
        /// <param name="amount"> Amount to add. </param>
        private void AddItem(int itemId, int amount)
        {
            UiSlot uiSlot;
            if (_uiItems.Exists(x => x.itemId == itemId && x.amount < _maxPerSlot))
            {
                int uiSlotIndex = _uiItems.FindIndex(x => x.itemId == itemId && x.amount < _maxPerSlot);

                uiSlot = _uiItems[uiSlotIndex];

                if (uiSlot.amount + amount <= _maxPerSlot)
                {
                    uiSlot.amount += amount;
                    uiSlot.slot.amount.text = uiSlot.amount.ToString();
                    _uiItems[uiSlotIndex] = uiSlot;
                    return;
                }
            }

            GameObject itemAdded = Instantiate(itemSlotPrefab, gameObject.transform);
            ItemUI itemUI = itemAdded.GetComponent<ItemUI>();

            uiSlot = new UiSlot
            {
                itemId = itemId,
                amount = amount,
                slotId = _uiItems.Count,
                slot = itemUI
            };
            itemUI.icon.sprite = _items[itemId].icon;
            itemUI.amount.text = amount.ToString();
            _uiItems.Add(uiSlot);
        }
    }
}