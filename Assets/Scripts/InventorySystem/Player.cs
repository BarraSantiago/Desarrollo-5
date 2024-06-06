using UnityEngine;
using UnityEngine.SceneManagement;

namespace InventorySystem
{
    public class Player : MonoBehaviour
    {
        public InventoryObject inventory;
        public InventoryObject equipment;
        public float pickUpCooldown = 3f;
        public int Money;


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

            Destroy(other.gameObject);
            inventory.UpdateInventory();
        }

        public void OnApplicationQuit()
        {
            inventory.Clear();
            equipment?.Clear();
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