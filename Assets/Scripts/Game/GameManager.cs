using System.Collections.Generic;
using InventorySystem;
using UI;
using UnityEngine;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private GameObject _inventoryUIHodler;
        [SerializeField] private GameObject _itemSlotPrefab;
        [SerializeField] private ItemList _itemList;
        [SerializeField] private int inventorySize;

        private Dictionary<int, Item> _items;
        private InventoryUI _inventoryUI;
        private Inventory _inventory;
        private Inventory.Slot[] slots;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _items = new Dictionary<int, Item>();
            
            for (int i = 0; i < _itemList.items.Length; i++)
            {
                _items.Add(i, _itemList.items[i]);
            }

            LoadInventory();
            
            _inventoryUI = _inventoryUIHodler.AddComponent<InventoryUI>();

            _inventoryUI.Initialize(_items, _itemSlotPrefab);

            
        }

        private void LoadInventory()
        {
            //TODO load inventory here
            slots = new Inventory.Slot[inventorySize];
            
            _inventory = _player.Inventory;
            
            _inventory.Initialize(inventorySize, _items, slots);
        }
    }
}