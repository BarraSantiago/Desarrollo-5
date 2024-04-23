using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventorySystem
{
    public class Inventory : MonoBehaviour
    {
        public void Initialize(int slotAmount, Dictionary<int, Item> _items, Slot[] slots)
        {
            this.slots = new Slot[slotAmount];
            this._items = _items;
            this.slots = slots;
        }

        public struct Slot
        {
            public bool occupied;
            public int itemID;
            public int amount;
            public Item item;
        }

        public static Action<int, int> ItemAdded;

        private Slot[] slots;
        private Dictionary<int, Item> _items;
        
        /// <summary>
        /// Checks if there is an available slot in the inventory.
        /// </summary>
        /// <returns> Returns true if there is any slot free. </returns>
        public bool IsSlotAvailable()
        {
            return slots.Any(element => element.occupied == false);
        }

        /// <summary>
        /// Adds item to the inventory.
        /// </summary>
        /// <param name="itemId"> Item to be added. </param>
        /// <returns> True = item successfully added to inventory. False = no space left to add item. </returns>
        public bool AddItem(int itemId)
        {
            if (slots.Any(id => id.itemID == itemId))
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i].itemID == itemId)
                    {
                        slots[i].amount++;
                        ItemAdded?.Invoke(itemId, 1);
                        return true;
                    }
                }
            }

            if (!IsSlotAvailable()) return false;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].occupied) continue;

                slots[i].occupied = true;
                slots[i].itemID = itemId;
                slots[i].amount = 1;
                slots[i].item = _items[itemId];
                ItemAdded?.Invoke(itemId, slots[i].amount);
            }

            return true;
        }
    }
}