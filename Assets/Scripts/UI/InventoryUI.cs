using System.Collections.Generic;
using InventorySystem;
using UnityEngine;

namespace UI
{
    public class InventoryUI : MonoBehaviour
    {
        public void Initialize(Dictionary<int, Item> _items)
        {
            this._items = _items;
        }

        [SerializeField] private GameObject ItemSlotPrefab;
        private Dictionary<int, Item> _items;

        private void Start()
        {
            Inventory.ItemAdded += AddItem;
        }

        private void OnDestroy()
        {
            Inventory.ItemAdded -= AddItem;
        }

        private void AddItem(int itemId, int amount)
        {
            GameObject itemAdded = Instantiate(ItemSlotPrefab, gameObject.transform);
            ItemUI itemUI = itemAdded.GetComponent<ItemUI>();
            itemUI.icon.sprite = _items[itemId].icon;
            itemUI.amount.text = amount.ToString();
        }
    }
}