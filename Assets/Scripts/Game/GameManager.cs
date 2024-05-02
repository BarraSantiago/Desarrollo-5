using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private GameObject _inventoryUIHodler;
        [SerializeField] private GameObject _itemSlotPrefab;
        [SerializeField] private ItemDatabaseObject _itemList;
        [SerializeField] private int inventorySize;

        private Inventory _inventory;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            LoadInventory();
        }

        private void LoadInventory()
        {
            //TODO load inventory here
        }
    }
}