using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
    public class InventoryObject : ScriptableObject
    {
        public Action<int> OnItemAdded;
        public static Action<int> OnItemSwapInventory;
        public string savePath;
        public ItemDatabaseObject database;

        public InterfaceType type;

        //public int MAX_ITEMS;
        [SerializeField] private Inventory Container = new Inventory();
        public InventorySlot[] GetSlots => Container.Slots;

        public void UpdateInventory()
        {
            foreach (var slot in GetSlots)
            {
                slot.UpdateSlot(slot.item, slot.amount);
            }
        }

        public bool AddItem(Item item, int amount)
        {
            ItemObject itemObject = database.ItemObjects[item.id];
            if (itemObject.stackable)
            {
                List<InventorySlot> slots = FindAllItemsOnInventory(item);
                foreach (InventorySlot slot in slots)
                {
                    int totalAmount = slot.amount + amount;
                    if (totalAmount <= itemObject.maxStack)
                    {
                        slot.AddAmount(amount);
                        return true;
                    }
                    else
                    {
                        int excessAmount = totalAmount - itemObject.maxStack;
                        slot.AddAmount(itemObject.maxStack - slot.amount); // Fill the existing slot to max stack
                        amount = excessAmount; // Update the remaining amount
                    }
                }

                // If there is still amount left, try to add it to a new slot
                if (amount <= 0) return false;
                InventorySlot emptySlot = GetEmptySlot();

                if (emptySlot == null) return false;
                emptySlot.UpdateSlot(item, amount);

                return true;
            }
            else
            {
                // If the item is not stackable, add it to a new slot
                InventorySlot emptySlot = GetEmptySlot();
                if (emptySlot == null) return false;
                emptySlot.UpdateSlot(item, amount);
                return true;
            }
        }

        public int EmptySlotCount
        {
            get { return GetSlots.Count(slot => slot.item.id <= -1); }
        }

        public List<InventorySlot> FindAllItemsOnInventory(Item item)
        {
            return GetSlots.Where(slot => slot.item.id == item.id).ToList();
        }

        public InventorySlot FindItemOnInventory(Item item)
        {
            return GetSlots.FirstOrDefault(t => t.item.id == item.id);
        }

        public bool IsItemInInventory(ItemObject item)
        {
            return GetSlots.Any(t => t.item.id == item.data.id);
        }

        private InventorySlot GetEmptySlot()
        {
            for (int i = 0; i < GetSlots.Length; i++)
            {
                if (GetSlots[i].item.id > -1) continue;
                
                OnItemAdded?.Invoke(i);
                return GetSlots[i];
            }

            return null;
        }

        public void SwapItems(InventorySlot originalSlot, InventorySlot targetSlot)
        {
            if (!targetSlot.CanPlaceInSlot(originalSlot.GetItemObject()) ||
                !originalSlot.CanPlaceInSlot(targetSlot.GetItemObject())) return;

            if (targetSlot.GetItemObject() == null ||
                targetSlot.GetItemObject().data.id != originalSlot.GetItemObject().data.id)
            {
                InventorySlot temp = new InventorySlot(targetSlot.item, targetSlot.amount);
                targetSlot.UpdateSlot(originalSlot.item, originalSlot.amount);
                originalSlot.UpdateSlot(temp.item, temp.amount);
            }
            else if (originalSlot.GetItemObject().data.id == targetSlot.GetItemObject().data.id &&
                     database.ItemObjects[originalSlot.GetItemObject().data.id].stackable)
            {
                int targetSlotRemainingSpace = targetSlot.GetItemObject().maxStack - targetSlot.amount;
                if (originalSlot.amount <= targetSlotRemainingSpace)
                {
                    targetSlot.AddAmount(originalSlot.amount);
                    originalSlot.RemoveItem();
                }
                else
                {
                    originalSlot.AddAmount(-targetSlotRemainingSpace);
                    if (originalSlot.amount == 0) originalSlot.RemoveItem();
                    targetSlot.AddAmount(targetSlotRemainingSpace);
                }
            }

            OnItemAdded?.Invoke(Array.IndexOf(GetSlots, originalSlot));
            OnItemAdded?.Invoke(Array.IndexOf(GetSlots, targetSlot));
            if (targetSlot.parent.inventory != this)
            {
                OnItemSwapInventory?.Invoke(Array.IndexOf(targetSlot.parent.inventory.GetSlots, targetSlot));
            }
        }

        [ContextMenu("Save")]
        public void Save()
        {
            #region Optional Save

            //string saveData = JsonUtility.ToJson(Container, true);
            //BinaryFormatter bf = new BinaryFormatter();
            //FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
            //bf.Serialize(file, saveData);
            //file.Close();

            #endregion

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create,
                FileAccess.Write);
            formatter.Serialize(stream, Container);
            stream.Close();
        }

        [ContextMenu("Load")]
        public void Load()
        {
            if (!File.Exists(string.Concat(Application.persistentDataPath, savePath))) return;

            #region Optional Load

            //BinaryFormatter bf = new BinaryFormatter();
            //FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
            //JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), Container);
            //file.Close();

            #endregion

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open,
                FileAccess.Read);
            Inventory newContainer = (Inventory)formatter.Deserialize(stream);
            for (int i = 0; i < GetSlots.Length; i++)
            {
                GetSlots[i].UpdateSlot(newContainer.Slots[i].item, newContainer.Slots[i].amount);
            }

            stream.Close();
        }

        
        [ContextMenu("Clear")]
        public void Clear()
        {
            Container.Clear();
        }

        public int GetItemCount(Item item)
        {
            var slots = FindAllItemsOnInventory(item);
           
            return slots.Sum(slot => slot.amount);
        }

        public void RemoveItem(Item currentEntryData, int amount)
        {
            List<InventorySlot> slots = FindAllItemsOnInventory(currentEntryData);
            foreach (InventorySlot slot in slots)
            {
                if (slot.amount <= amount)
                {
                    amount -= slot.amount;
                    slot.RemoveItem();
                    if(amount > 0) RemoveItem(currentEntryData, amount);
                }
                else
                {
                    slot.AddAmount(-amount);
                    break;
                }
            }
        }
    }
}