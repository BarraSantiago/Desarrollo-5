﻿using TMPro;
using UnityEngine;

namespace Store
{
    public class ItemDisplayer : MonoBehaviour
    {
        [SerializeField] private Transform[] objPostition;
        [SerializeField] private TextMeshPro[] texts;

        public DisplayItem[] Items;

        private ItemDatabaseObject database;
        private InventoryObject storeInventory;

        public void Initialize(ItemDatabaseObject database, InventoryObject storeInventory)
        {
            this.storeInventory = storeInventory;
            this.database = database;

            this.storeInventory.OnItemAdded += OnAddItem;
            InventoryObject.OnItemSwapInventory += OnAddItem;
            StoreManager.EndCycle += Deinitialize;
            Client.ItemGrabbed += RemoveItem;

            Items = new DisplayItem[storeInventory.GetSlots.Length];

            UpdateInventory();
        }

        private void Deinitialize()
        {
            StoreManager.EndCycle -= Deinitialize;
            InventoryObject.OnItemSwapInventory += OnAddItem;
            storeInventory.OnItemAdded -= OnAddItem;
        }

        private void RemoveItem(int id)
        {
            Items[id] = null;
            storeInventory.GetSlots[id].RemoveItem();
            texts[id].text = string.Empty;
        }

        private void OnAddItem(int slotId)
        {
            if (!ValidItem(slotId)) return;

            CreateDisplayItem(slotId);
        }

        private bool ValidItem(int slotId)
        {
            if (slotId >= storeInventory.GetSlots.Length || slotId < 0) return false;

            if (storeInventory.GetSlots[slotId].item.id < 0)
            {
                UpdateInventory();
                return false;
            }

            if (Items[slotId] != null) Destroy(Items[slotId].Object);

            if (storeInventory.GetSlots[slotId].amount == 0)
            {
                Items[slotId] = null;
                return false;
            }

            return true;
        }

        private void CreateDisplayItem(int slotId)
        {
            Items[slotId] = new DisplayItem
            {
                Object = Instantiate(database.ItemObjects[storeInventory.GetSlots[slotId].item.id].characterDisplay,
                    objPostition[slotId]),
                ItemObject = storeInventory.GetSlots[slotId].GetItemObject(),
                id = slotId,
                showPrice = texts[slotId]
            };

            Items[slotId].Initialize(Items[slotId].ItemObject.price);

            UpdateInventory();
        }

        private void UpdateInventory()
        {
            foreach (var slot in storeInventory.GetSlots)
            {
                slot.UpdateSlot(slot.item, slot.amount);
            }

            for (int i = 0; i < Items.Length; i++)
            {
                if (storeInventory.GetSlots[i].amount == 0)
                {
                    if (Items[i] != null) Destroy(Items[i].Object);
                    Items[i] = null;
                    texts[i].text = string.Empty;
                }
                else if (storeInventory.GetSlots[i].amount > 0)
                {
                    if (Items[i] == null)
                    {
                        OnAddItem(i);
                    }
                    else if (Items[i].ItemObject != storeInventory.GetSlots[i].GetItemObject())
                    {
                        RemoveItem(i);
                        OnAddItem(i);
                    }
                }
            }
        }
    }
}