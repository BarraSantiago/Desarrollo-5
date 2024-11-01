﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

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

        protected override void CreateSlots()
        {
            slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
            for (int i = 0; i < inventory.GetSlots.Length; i++)
            {
                var obj = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
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
                AddEvent(obj, EventTriggerType.PointerClick, delegate(BaseEventData data) { OnRightClick(obj, data); });
                AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
                AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
                AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });
                inventory.GetSlots[i].slotDisplay = obj;
                slotsOnInterface.Add(obj, inventory.GetSlots[i]);
            }
        }

        private Vector3 GetPosition(int i)
        {
            return new Vector3(xStart + (xSpaceBetweenItem * (i % numberOfColumn)),
                yStart + (-ySpaceBetweenItems * (i / numberOfColumn)), 0f);
        }
    }
}