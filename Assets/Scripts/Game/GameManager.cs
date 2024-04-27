using System.Collections.Generic;
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
        private Inventory _inventory;

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
            
           

            
        }

        private void LoadInventory()
        {
            //TODO load inventory here
        }
    }
}