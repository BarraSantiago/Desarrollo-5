using UnityEngine;

namespace InventorySystem
{
    public class Player : MonoBehaviour
    {
        public InventoryObject inventory;
        public InventoryObject equipment;
        public int Money;


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