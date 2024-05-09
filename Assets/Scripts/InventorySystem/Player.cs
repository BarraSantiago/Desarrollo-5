using UnityEngine;
using UnityEngine.SceneManagement;

namespace InventorySystem
{
    public class Player : MonoBehaviour
    {
        public InventoryObject inventory;
        public InventoryObject equipment;
        public int Money;


        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            inventory.UpdateInventory();
        }
        public void OnTriggerEnter(Collider other)
        {
            var item = other.GetComponent<GroundItem>();
            if (item)
            {
                if (inventory.AddItem(new Item(item.item), item.amount))
                    Destroy(other.gameObject);
            }
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                inventory.Save();
                equipment?.Save();
            }
            else if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                inventory.Load();
                equipment?.Load();
            }
        }


        public void OnApplicationQuit()
        {
            inventory.Clear();
            equipment?.Clear();
        }
    }
}