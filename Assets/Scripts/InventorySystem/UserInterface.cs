﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventorySystem
{
    [RequireComponent(typeof(EventTrigger))]
    public abstract class UserInterface : MonoBehaviour
    {
        [SerializeField] private GameObject itemDisplayPrefab;
        [SerializeField] private Canvas canvas;

        public InventoryObject inventory;
        public Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
        public static Action<GameObject, int> OnDropItem;

        private InventoryObject _previousInventory;

        public void Awake()
        {
            CreateSlots();
            foreach (InventorySlot slot in inventory.GetSlots)
            {
                slot.parent = this;
                slot.onAfterUpdated += OnSlotUpdate;
            }

            AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
            AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
        }

        protected abstract void CreateSlots();

        private void UpdateInventoryLinks()
        {
            int i = 0;
            foreach (GameObject key in slotsOnInterface.Keys.ToList())
            {
                slotsOnInterface[key] = inventory.GetSlots[i];
                i++;
            }
        }

        public void OnSlotUpdate(InventorySlot slot)
        {
            if (!slot.slotDisplay || !slot.slotDisplay.transform.GetChild(0))
            {
                return;
            }

            if (slot.item.id <= -1)
            {
                slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().sprite = null;
                slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
                slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
            }
            else
            {
                slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().sprite = slot.GetItemObject().uiDisplay;
                slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
                slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().preserveAspect = true;
                slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text =
                    slot.amount == 1 ? string.Empty : slot.amount.ToString("n0");
            }
        }

        public void Update()
        {
            if (_previousInventory != inventory)
            {
                UpdateInventoryLinks();
            }

            _previousInventory = inventory;
        }

        protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
        {
            EventTrigger trigger = obj.GetComponent<EventTrigger>();
            if (!trigger)
            {
                trigger = obj.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry eventTrigger = new EventTrigger.Entry { eventID = type };
            eventTrigger.callback.AddListener(action);
            trigger.triggers.Add(eventTrigger);
        }

        public void OnEnter(GameObject obj)
        {
            MouseData.SlotHoveredOver = obj;
        }

        public void OnRightClick(GameObject obj, BaseEventData data)
        {
            if (data is not PointerEventData { button: PointerEventData.InputButton.Right }) return;

            if (slotsOnInterface[obj].item.id < 0) return;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();


            Vector2 itemDisplaySize = itemDisplayPrefab.GetComponent<RectTransform>().sizeDelta;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null,
                out Vector2 localMousePosition);

            localMousePosition.x = Mathf.Clamp(localMousePosition.x,
                -canvasRect.sizeDelta.x / 2 + itemDisplaySize.x / 2,
                canvasRect.sizeDelta.x / 2 - itemDisplaySize.x / 2);
            localMousePosition.y = Mathf.Clamp(localMousePosition.y,
                -canvasRect.sizeDelta.y / 2 + itemDisplaySize.y / 2,
                canvasRect.sizeDelta.y / 2 - itemDisplaySize.y / 2);

            Vector3 worldMousePosition = canvasRect.TransformPoint(localMousePosition);
            GameObject itemDisplay = Instantiate(itemDisplayPrefab, worldMousePosition, Quaternion.identity,
                canvas.transform);

            ItemInfoPanel infoPanel = itemDisplay.GetComponent<ItemInfoPanel>();
            infoPanel.slot = slotsOnInterface[obj];
            itemDisplay.transform.SetAsLastSibling();
            infoPanel.item = slotsOnInterface[obj].GetItemObject();
            infoPanel.Initialize();
        }

        private void OnEnterInterface(GameObject obj)
        {
            MouseData.InterfaceMouseIsOver = obj.GetComponent<UserInterface>();
        }

        private void OnExitInterface(GameObject obj)
        {
            MouseData.InterfaceMouseIsOver = null;
        }

        protected void OnExit(GameObject obj)
        {
            MouseData.SlotHoveredOver = null;
        }

        protected void OnDragStart(GameObject obj)
        {
            InventorySlot slot = slotsOnInterface[obj];
            if (slot.item.id >= 0)
            {
                MouseData.TempItemBeingDragged = CreateTempItem(obj);
                MouseData.TempItemBeingDragged.transform.SetParent(UIManager.TopCanvas.transform);
            }
        }

        private GameObject CreateTempItem(GameObject obj)
        {
            GameObject tempItem = null;
            if (slotsOnInterface[obj].item.id < 0) return tempItem;

            tempItem = new GameObject();
            RectTransform rt = tempItem.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(75, 75);

            Image img = tempItem.AddComponent<Image>();
            img.sprite = slotsOnInterface[obj].GetItemObject().uiDisplay;
            img.raycastTarget = false;
            img.preserveAspect = true;
            return tempItem;
        }

        protected void OnDragEnd(GameObject obj)
        {
            if (!obj) return;
            Destroy(MouseData.TempItemBeingDragged);
            if (!MouseData.InterfaceMouseIsOver)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (!Physics.Raycast(ray, out RaycastHit hit)) return;


                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Walkable")) return;

                //DropItem(obj);

                return;
            }

            if (!MouseData.SlotHoveredOver) return;

            InventorySlot mouseHoverSlotData =
                MouseData.InterfaceMouseIsOver.slotsOnInterface[MouseData.SlotHoveredOver];
            inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
        }

        private void DropItem(GameObject obj)
        {
            OnDropItem?.Invoke(slotsOnInterface[obj].GetItemObject().characterDisplay,
                slotsOnInterface[obj].amount);
            slotsOnInterface[obj].RemoveItem();
            InventoryObject.OnItemSwapInventory?.Invoke(0);
        }

        protected void OnDrag(GameObject obj)
        {
            if (MouseData.TempItemBeingDragged != null)
                MouseData.TempItemBeingDragged.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }
}