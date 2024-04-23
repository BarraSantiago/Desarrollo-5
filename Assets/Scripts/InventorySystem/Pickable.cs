using UnityEngine;

namespace InventorySystem
{
    public abstract class Pickable : MonoBehaviour
    {
        [SerializeField] public Sprite icon;
        [SerializeField] private int itemID;

        private Inventory _inventory;

        private void Awake()
        {
            //TODO Find a better way to get the inventory
            _inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            
            if (_inventory.AddItem(itemID))
            {
                gameObject.SetActive(false);
            }
        }
    }
}