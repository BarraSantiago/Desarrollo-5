using System;
using UnityEngine;

namespace InventorySystem
{
    [Serializable]
    public class InventorySlot
    {
        public ItemType[] AllowedItems = new ItemType[0];

        [NonSerialized] public UserInterface parent;
        [NonSerialized] public GameObject slotDisplay;
    
        [NonSerialized] public Action<InventorySlot> onAfterUpdated;
        [NonSerialized] public Action<InventorySlot> onBeforeUpdated;

        public Item item;
        public int amount;

        public ItemObject GetItemObject()
        {
            if (!parent || !parent.inventory || !parent.inventory.database || item.id < 0 || item.id >= parent.inventory.database.ItemObjects.Length)
            {
                return null;
            }
            return parent.inventory.database.ItemObjects[item.id];
            
        }

        public InventorySlot() => UpdateSlot(new Item(), 0);

        public InventorySlot(Item item, int amount) => UpdateSlot(item, amount);

        public void RemoveItem() => UpdateSlot(new Item(), 0);

        public void AddAmount(int value) => UpdateSlot(item, amount += value);


        public void UpdateSlot(Item itemValue, int amountValue)
        {
            onBeforeUpdated?.Invoke(this);
            item = itemValue;
            amount = amountValue;
            if (amount <= 0)
            {
                item = new Item();
                amount = 0;
            }
            onAfterUpdated?.Invoke(this);
        }

        public bool CanPlaceInSlot(ItemObject itemObject)
        {
            if (AllowedItems.Length <= 0 || itemObject == null || itemObject.data.id < 0)
                return true;
            for (int i = 0; i < AllowedItems.Length; i++)
            {
                if (itemObject.type == AllowedItems[i])
                    return true;
            }

            return false;
        }
    }
}