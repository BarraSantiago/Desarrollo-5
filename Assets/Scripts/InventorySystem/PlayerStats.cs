using player;
using UnityEngine;

namespace InventorySystem
{
    public class PlayerStats : MonoBehaviour
    {
    
        public Attribute[] attributes;
        public Attribute Agility => attributes[0];
    
        private InventoryObject _equipment;

        private void Start()
        {
            _equipment = GetComponent<Player>().equipment;
        
            foreach (var t in attributes)
            {
                t.SetParent(this);
            }

            foreach (var t in _equipment.GetSlots)
            {
                t.onBeforeUpdated += OnRemoveItem;
                t.onAfterUpdated += OnEquipItem;
            }
        }
    
        public void AttributeModified(Attribute attribute)
        {
            //Debug.Log(string.Concat(attribute.type, " was updated! Value is now ", attribute.value.ModifiedValue));
        }
    
        public void OnRemoveItem(InventorySlot slot)
        {
            if (!slot.GetItemObject())
                return;
            switch (slot.parent.inventory.type)
            {
                case InterfaceType.Inventory:
                    print("Removed " + slot.GetItemObject() + " on: " + slot.parent.inventory.type + ", Allowed items: " +
                          string.Join(", ", slot.AllowedItems));
                    break;
            
                case InterfaceType.Equipment:
                    //    print("Removed " + slot.GetItemObject() + " on: " + slot.parent.inventory.type + ", Allowed items: " +
                    //          string.Join(", ", slot.AllowedItems));
                    foreach (var itemBuff in slot.item.buffs)
                    {
                        foreach (var atribute in attributes)
                        {
                            if (atribute.type == itemBuff.stat)
                                atribute.value.RemoveModifier(itemBuff);
                        }
                    }
                    break;
            
                case InterfaceType.Chest:
                    print("Removed " + slot.GetItemObject() + " on: " + slot.parent.inventory.type + ", Allowed items: " +
                          string.Join(", ", slot.AllowedItems));
                    break;
                default:
                    break;
            }
        }

        public void OnEquipItem(InventorySlot slot)
        {
            if (!slot.GetItemObject())
                return;
            switch (slot.parent.inventory.type)
            {
                case InterfaceType.Inventory:
                    print("Placed " + slot.GetItemObject() + " on: " + slot.parent.inventory.type + ", Allowed items: " +
                          string.Join(", ", slot.AllowedItems));
                    break;
            
                case InterfaceType.Equipment:
                    // print("Placed " + _slot.GetItemObject() + " on: " + _slot.parent.inventory.type + ", Allowed items: " +
                    //      string.Join(", ", _slot.AllowedItems));
                    foreach (var itemBuff in slot.item.buffs)
                    {
                        foreach (var attribute in attributes)
                        {
                            if (attribute.type == itemBuff.stat)
                                attribute.value.AddModifier(itemBuff);
                        }
                    }
                    break;
            
                case InterfaceType.Chest:
                    print("Placed " + slot.GetItemObject() + " on: " + slot.parent.inventory.type + ", Allowed items: " +
                          string.Join(", ", slot.AllowedItems));
                    break;
            
                default:
                    break;
            }
        }
    }
}
