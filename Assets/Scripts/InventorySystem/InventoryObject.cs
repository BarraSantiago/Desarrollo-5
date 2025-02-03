using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace InventorySystem
{
    [Serializable]
    public class SerializableInventorySlot
    {
        public int SlotIndex;
        public int ItemId;
        public int Amount;
    }

    [Serializable]
    public class SerializableInventory
    {
        public SerializableInventorySlot[] Slots;
    }

    [CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
    public class InventoryObject : ScriptableObject
    {
        [SerializeField] private Inventory Container = new Inventory();

        public Action<int> OnItemAdded;
        public static Action<int> OnItemSwapInventory;
        public string savePath;
        public ItemDatabaseObject database;
        public InterfaceType type;
        public InventorySlot[] GetSlots => Container.Slots;

        public void UpdateInventory()
        {
            foreach (InventorySlot slot in GetSlots)
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
                        OnItemAdded?.Invoke(Array.IndexOf(GetSlots, slot));
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
                OnItemAdded?.Invoke(Array.IndexOf(GetSlots, emptySlot));

                return true;
            }
            else
            {
                // If the item is not stackable, add it to a new slot
                InventorySlot emptySlot = GetEmptySlot();
                if (emptySlot == null) return false;
                emptySlot.UpdateSlot(item, amount);
                OnItemAdded?.Invoke(Array.IndexOf(GetSlots, emptySlot));
                return true;
            }
        }

        /// <summary>
        /// Sorts the inventory slots in place using the swap function.
        /// Sorting is done by:
        ///   1. Bringing slots whose GetItemObject().type matches selectedItemType to the front.
        ///   2. Then ordering by item.listPrice.CurrentPrice.
        ///   3. Then by item.id.
        ///   4. Then by amount.
        /// Empty slots (with item.id less than 0) will be moved to the end.
        /// </summary>
        /// <param name="selectedItemType">The item type that should come first.</param>
        public void SortInventory(ItemType selectedItemType)
        {
            // Get the array of slots from our container.
            InventorySlot[] slots = GetSlots;
            int n = slots.Length;

            // Use a simple selection sort.
            for (int i = 0; i < n - 1; i++)
            {
                // Find the slot with the smallest value (according to our comparer) among slots[i...n-1]
                int minIndex = i;
                for (int j = i + 1; j < n; j++)
                {
                    if (CompareSlots(slots[j], slots[minIndex], selectedItemType) < 0)
                    {
                        minIndex = j;
                    }
                }

                // If a slot out-of-order is found, swap its contents with the current slot.
                if (minIndex != i)
                {
                    // SwapItems will move the items between these two slots.
                    SwapItems(slots[i], slots[minIndex]);
                }
            }
        }

        /// <summary>
        /// Compares two slots using the following criteria:
        /// 1. Non-empty slots come before empty slots.
        /// 2. Slots whose item type matches selectedItemType come first.
        /// 3. Then by listPrice.CurrentPrice (lowest first).
        /// 4. Then by item.id.
        /// 5. Then by amount.
        /// </summary>
        /// <param name="a">First slot.</param>
        /// <param name="b">Second slot.</param>
        /// <param name="selectedItemType">The item type that should be prioritized.</param>
        /// <returns>
        /// A negative value if slot a should come before slot b,
        /// zero if they are equal,
        /// and a positive value if slot a should come after slot b.
        /// </returns>
        private int CompareSlots(InventorySlot a, InventorySlot b, ItemType selectedItemType)
        {
            // First, determine whether either slot is empty.
            bool aEmpty = a.item == null || a.GetItemObject() == null || a.item.id < 0;
            bool bEmpty = b.item == null || b.GetItemObject() == null || b.item.id < 0;

            if (aEmpty && bEmpty)
                return 0;
            if (aEmpty)
                return 1; // a is empty: push it to the end.
            if (bEmpty)
                return -1; // b is empty: push it to the end.

            // Primary criterion: Compare by whether the slot’s type matches the selected type.
            // (In the LINQ code, OrderBy(slot => slot.GetItemObject().type != selectedItemType) causes
            //  slots with a matching type (false) to come before those that do not (true).)
            bool aNonMatch = a.GetItemObject().type != selectedItemType;
            bool bNonMatch = b.GetItemObject().type != selectedItemType;
            if (aNonMatch != bNonMatch)
                return aNonMatch ? 1 : -1;

            // Secondary criterion: Compare by listPrice.CurrentPrice.
            int cmp = a.item.listPrice.CurrentPrice.CompareTo(b.item.listPrice.CurrentPrice);
            if (cmp != 0)
                return cmp;

            // Tertiary criterion: Compare by item.id.
            cmp = a.item.id.CompareTo(b.item.id);
            if (cmp != 0)
                return cmp;

            // Finally, compare by amount.
            return a.amount.CompareTo(b.amount);
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
            SerializableInventory serializableInventory = new SerializableInventory
            {
                Slots = GetSlots.Select((slot, index) => new SerializableInventorySlot
                {
                    SlotIndex = index,
                    ItemId = slot.item.id,
                    Amount = slot.amount
                }).ToArray()
            };

            IFormatter formatter = new BinaryFormatter();
            Stream stream = null;

            try
            {
                stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create,
                    FileAccess.Write);
                formatter.Serialize(stream, serializableInventory);
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to serialize inventory data: " + e.Message);
            }
            finally
            {
                stream?.Close();
            }
        }

        [ContextMenu("Load")]
        public void Load()
        {
            string filePath = string.Concat(Application.persistentDataPath, savePath);
            if (!File.Exists(filePath)) return;

            Stream stream = null;
            try
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                if (stream.Length == 0) return; // Check if the file is empty

                IFormatter formatter = new BinaryFormatter();
                SerializableInventory newContainer = (SerializableInventory)formatter.Deserialize(stream);
                foreach (SerializableInventorySlot slot in newContainer.Slots)
                {
                    if (slot is null || slot.ItemId == -1) continue;
                    GetSlots[slot.SlotIndex].UpdateSlot(database.ItemObjects[slot.ItemId].data, slot.Amount);
                }
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to deserialize inventory data: " + e.Message);
            }
            finally
            {
                stream?.Close();
            }
        }


        [ContextMenu("Clear")]
        public void Clear()
        {
            Container.Clear();
        }

        public void RemoveAllItems()
        {
            foreach (InventorySlot slot in GetSlots)
            {
                slot.RemoveItem();
            }
        }

        public int GetItemCount(Item item)
        {
            List<InventorySlot> slots = FindAllItemsOnInventory(item);

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
                    if (amount > 0) RemoveItem(currentEntryData, amount);
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