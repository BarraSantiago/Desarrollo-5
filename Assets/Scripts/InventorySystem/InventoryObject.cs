﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;



[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
public class InventoryObject : ScriptableObject
{
    public Action<int> OnItemAdded;
    public static Action<int> OnItemSwapInventory;
    public string savePath;
    public ItemDatabaseObject database;
    public InterfaceType type;
    //public int MAX_ITEMS;
    [SerializeField]
    private Inventory Container = new Inventory();
    public InventorySlot[] GetSlots => Container.Slots;

    public bool AddItem(Item item, int amount)
    {
        if (EmptySlotCount <= 0)
            return false;
        InventorySlot slot = FindItemOnInventory(item);
        if (!database.ItemObjects[item.id].stackable || slot == null)
        {
            GetEmptySlot().UpdateSlot(item, amount);
            return true;
        }
        slot.AddAmount(amount);
        return true;
    }

    public int EmptySlotCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < GetSlots.Length; i++)
            {
                if (GetSlots[i].item.id <= -1)
                {
                    counter++;
                }
            }
            return counter;
        }
    }

    public InventorySlot FindItemOnInventory(Item item)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].item.id == item.id)
            {
                OnItemAdded?.Invoke(i);
                return GetSlots[i];
            }
        }
        return null;
    }
    
    
    public bool IsItemInInventory(ItemObject item)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].item.id == item.data.id)
            {
                return true;
            }
        }
        return false;
    }
    
    
    public InventorySlot GetEmptySlot()
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if (GetSlots[i].item.id <= -1)
            {
                OnItemAdded?.Invoke(i);
                return GetSlots[i];
            }
        }
        return null;
    }

    public void SwapItems(InventorySlot originalSlot, InventorySlot targetSlot)
    {
        if (originalSlot == targetSlot)
            return;
        if (targetSlot.CanPlaceInSlot(originalSlot.GetItemObject()) && originalSlot.CanPlaceInSlot(targetSlot.GetItemObject()))
        {
            InventorySlot temp = new InventorySlot(targetSlot.item, targetSlot.amount);
            targetSlot.UpdateSlot(originalSlot.item, originalSlot.amount);
            originalSlot.UpdateSlot(temp.item, temp.amount);
            
            OnItemAdded?.Invoke(Array.IndexOf(GetSlots, originalSlot));
            OnItemAdded?.Invoke(Array.IndexOf(GetSlots, targetSlot));
            
            if(targetSlot.parent.inventory != this) OnItemSwapInventory?.Invoke(Array.IndexOf(targetSlot.parent.inventory.GetSlots, targetSlot));
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
        Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, Container);
        stream.Close();
    }

    [ContextMenu("Load")]
    public void Load()
    {
        if (File.Exists(string.Concat(Application.persistentDataPath, savePath)))
        {
            #region Optional Load
            //BinaryFormatter bf = new BinaryFormatter();
            //FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
            //JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), Container);
            //file.Close();
            #endregion

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
            Inventory newContainer = (Inventory)formatter.Deserialize(stream);
            for (int i = 0; i < GetSlots.Length; i++)
            {
                GetSlots[i].UpdateSlot(newContainer.Slots[i].item, newContainer.Slots[i].amount);
            }
            stream.Close();
        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        Container.Clear();
    }

}
