using System;
using InventorySystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace player
{
    public class Player : MonoBehaviour
    {
        public static Action<int> OnMoneyUpdate;
        public InventoryObject inventory;
        public float pickUpCooldown = 3f;
        private int _money;
        private const string PickUpSoundKey = "PickUp";

        public int money
        {
            get => _money;
            set
            {
                if (_money == value) return;
                _money = value;
                OnMoneyUpdate?.Invoke(_money);
            }
        }


        private void Start()
        {
            //inventory.Load();
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

        public void OnTriggerEnter(Collider other)
        {
            var item = other.GetComponent<GroundItem>();

            if (!item) return;

            if (item.droppedByPlayer && Time.time - item.droppedTime < pickUpCooldown) return;

            if (!inventory.AddItem(new Item(item.item), item.amount)) return;
            
            AudioManager.instance.Play(PickUpSoundKey);
            Destroy(other.gameObject);
            inventory.UpdateInventory();
        }


        public void OnApplicationQuit()
        {
            inventory.Save();
        }


        private void DropItem(GameObject obj, int amount)
        {
            GameObject droppedItem = Instantiate(obj, transform.position, Quaternion.identity);
            GroundItem groundItem = droppedItem.GetComponent<GroundItem>();
            groundItem.droppedByPlayer = true;
            groundItem.droppedTime = Time.time;
            groundItem.amount = amount;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            inventory.UpdateInventory();
        }
    }
}