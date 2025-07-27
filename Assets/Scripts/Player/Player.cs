using System;
using InventorySystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace player
{
    public class Player : MonoBehaviour
    {
        public static Action<int> OnMoneyUpdate;
        public Action<int> OnMoneyReduced;
        public InventoryObject inventory;
        public float pickUpCooldown = 2f;
        private int _money = 200;
        private const string PickUpSoundKey = "PickUp";
        private const string MoneyKey = "PlayerMoney";

        public int Money
        {
            get => _money;
            set
            {
                if (value < _money) OnMoneyReduced?.Invoke(_money - value);
                if (_money == value) return;
                _money = value;
                OnMoneyUpdate?.Invoke(_money);
            }
        }


        private void Start()
        {
            //inventory.Load();
            LoadMoney();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            UserInterface.OnDropItem += DropItem;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UserInterface.OnDropItem -= DropItem;
        }
        
        private void DropItem(GameObject obj, int amount)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            Vector3 dropPosition = hit.point + Vector3.up * 2f;
            GameObject droppedItem = Instantiate(obj, dropPosition, Quaternion.identity);

            Rigidbody rb = droppedItem.AddComponent<Rigidbody>();
            rb.mass = 1f;
            droppedItem.AddComponent<BoxCollider>();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            inventory.UpdateInventory();
        }

        public void SaveMoney()
        {
            PlayerPrefs.SetInt(MoneyKey, _money);
            PlayerPrefs.Save();
        }

        private void LoadMoney()
        {
            if (!PlayerPrefs.HasKey(MoneyKey)) return;
            
            _money = PlayerPrefs.GetInt(MoneyKey);
            OnMoneyUpdate?.Invoke(_money);
        }
    }
}