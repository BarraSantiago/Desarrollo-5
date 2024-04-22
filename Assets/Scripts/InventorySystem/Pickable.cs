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
            _inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!CompareTag("Player")) return;
            
            if (_inventory.AddItem(itemID))
            {
                Debug.Log("Item: " + itemID + " picked.");
                gameObject.SetActive(false);
            }
        }
    }
}