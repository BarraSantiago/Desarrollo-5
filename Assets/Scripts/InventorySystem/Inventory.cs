using System;
using System.Linq;

namespace InventorySystem
{
    [System.Serializable]
    public class Inventory
    {
        public InventorySlot[] Slots = new InventorySlot[24];

        public void Clear()
        {
            foreach (InventorySlot slot in Slots)
            {
                slot.item = new Item();
                slot.amount = 0;
            }
        }

        public bool ContainsItem(ItemObject itemObject)
        {
            return Array.Find(Slots, i => i.item.id == itemObject.data.id) != null;
        }


        public bool ContainsItem(int id)
        {
            return Slots.FirstOrDefault(i => i.item.id == id) != null;
        }
    }
}