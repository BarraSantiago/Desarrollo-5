using Interactable;
using player;
using UnityEngine;

namespace InventorySystem
{
    public class GroundItem : MonoBehaviour, IInteractable
    {
        public ItemObject item;
        public int amount = 1;
        public bool droppedByPlayer = false;
        public float droppedTime;
        public bool State { get; }
        public bool Interact()
        {
            Player player = FindObjectOfType<Player>();
            
            if (player is null || !player.inventory.AddItem(new Item(item), amount)) return false;
            
            Destroy(gameObject);
            return true;
        }

        public void Close()
        {
            throw new System.NotImplementedException();
        }
    }
}
