using System;
using UnityEngine;

namespace Store
{
    public class ItemDisplayer : MonoBehaviour
    {
        [SerializeField] private InventoryObject inventory;
        [SerializeField] private ItemDatabaseObject database;
        [SerializeField] private Transform[] objPostition;

        public DisplayItem[] Items;

        public void Initialize()
        {
            StoreManager.EndCycle += Deinitialize;
            inventory.OnItemAdded += OnAddItem;
            Client.ItemGrabbed += RemoveItem;
            
            Items = new DisplayItem[inventory.GetSlots.Length];
        }

        private void Deinitialize()
        {
            StoreManager.EndCycle -= Deinitialize;
            inventory.OnItemAdded -= OnAddItem;
        }


        public void Test()
        {
            for (int i = 0; i < inventory.GetSlots.Length; i++)
            {
                inventory.AddItem(database.ItemObjects[0].data, 1);
                
                Items[i] = new DisplayItem
                {
                    Object = Instantiate(database.ItemObjects[inventory.GetSlots[i].item.id].characterDisplay,
                        objPostition[i]),
                    ItemObject = inventory.GetSlots[i].GetItemObject(),
                    id = i
                };
                
                Items[i].ItemObject.price = 50;
            }
        }

        public void RemoveItem(int id)
        {
            Items[id] = null;
            inventory.GetSlots[id].RemoveItem();
        }

        private void OnAddItem(int slotId)
        {
            if(inventory.GetSlots[slotId].item.id < 0) return;
            if(Items[slotId] != null) Destroy(Items[slotId].Object);

            if (inventory.GetSlots[slotId].amount == 0)
            {
                Items[slotId] = null;
                return;
            }
            
            Items[slotId] = new DisplayItem
            {
                Object = Instantiate(database.ItemObjects[inventory.GetSlots[slotId].item.id].characterDisplay,
                    objPostition[slotId]),
                ItemObject = inventory.GetSlots[slotId].GetItemObject(),
                id = slotId
            };

            Items[slotId].Initialize(Items[slotId].ItemObject.price);
        }
    }
}