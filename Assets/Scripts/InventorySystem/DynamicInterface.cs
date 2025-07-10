using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace InventorySystem
{
    public class DynamicInterface : UserInterface
    {
        public GameObject inventoryPrefab;
        [FormerlySerializedAs("X_START")] public int xStart;
        [FormerlySerializedAs("Y_START")] public int yStart;
        [FormerlySerializedAs("X_SPACE_BETWEEN_ITEM")] public int xSpaceBetweenItem;
        [FormerlySerializedAs("NUMBER_OF_COLUMN")] public int numberOfColumn;
        [FormerlySerializedAs("Y_SPACE_BETWEEN_ITEMS")] public int ySpaceBetweenItems;
        [SerializeField] private Transform slotsParent;
        [SerializeField] private ItemInfo itemInfo;
        [SerializeField] private ItemDatabaseObject itemDatabase;
        [SerializeField] private Button[] sortButton;

        protected override void CreateSlots()
        {
            slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
            for (int i = 0; i < inventory.GetSlots.Length; i++)
            {
                GameObject obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
                if (slotsParent)
                {
                    obj.transform.SetParent(slotsParent);
                }
                else
                {
                    obj.GetComponent<RectTransform>().localPosition = GetPosition(i);
                }

                AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
                AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
                AddEvent(obj, EventTriggerType.PointerClick, delegate(BaseEventData data)
                {
                    InventorySlot slot = slotsOnInterface[obj];
                    if (slot.item == null) return; // Null check for slot.item

                    int itemId = slot.item.id;
                    ItemObject matchedItem = null;

                    foreach (ItemObject item in itemDatabase.ItemObjects)
                    {
                        if (item.data.id != itemId) continue;
                        matchedItem = item;
                        break;
                    }

                    if (!matchedItem) return;
                    itemInfo.UpdateItemInfo(matchedItem.uiDisplay, matchedItem.name, matchedItem.description);
                });
                AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
                AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
                AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });
                inventory.GetSlots[i].slotDisplay = obj;
                slotsOnInterface.Add(obj, inventory.GetSlots[i]);
            }

            if (sortButton == null || sortButton.Length <= 0) return;
            
            
                sortButton[0].onClick.AddListener(delegate { UpdateSlots((ItemType)0); });
                sortButton[1].onClick.AddListener(delegate { UpdateSlots((ItemType)3); });
                sortButton[2].onClick.AddListener(delegate { UpdateSlots((ItemType)2); });
                sortButton[3].onClick.AddListener(delegate { UpdateSlots((ItemType)1); });
            
        }

        private void UpdateSlots(ItemType type)
        {
            inventory.SortInventory(type);
            foreach (InventorySlot slot in slotsOnInterface.Values)
            {
                OnSlotUpdate(slot);
            }

        }


        private Vector3 GetPosition(int i)
        {
            return new Vector3(xStart + (xSpaceBetweenItem * (i % numberOfColumn)),
                yStart + (-ySpaceBetweenItems * (i / numberOfColumn)), 0f);
        }
    }
}