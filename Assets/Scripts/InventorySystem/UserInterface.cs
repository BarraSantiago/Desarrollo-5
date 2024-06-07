using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

        public InventoryObject inventory;
        public Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
        public static Action<GameObject, int> OnDropItem;

        private InventoryObject _previousInventory;

        public void Awake()
        {
            CreateSlots();
            foreach (var slot in inventory.GetSlots)
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
            foreach (var key in slotsOnInterface.Keys.ToList())
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
                Debug.LogWarning("No EventTrigger component found!");
                return;
            }

            var eventTrigger = new EventTrigger.Entry { eventID = type };
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

            Canvas canvas = FindObjectOfType<Canvas>();

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 itemDisplaySize = itemDisplayPrefab.GetComponent<RectTransform>().sizeDelta;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null,
                out var localMousePosition);

            localMousePosition.x = Mathf.Clamp(localMousePosition.x,
                -canvasRect.sizeDelta.x / 2 + itemDisplaySize.x / 2,
                canvasRect.sizeDelta.x / 2 - itemDisplaySize.x / 2);
            localMousePosition.y = Mathf.Clamp(localMousePosition.y,
                -canvasRect.sizeDelta.y / 2 + itemDisplaySize.y / 2,
                canvasRect.sizeDelta.y / 2 - itemDisplaySize.y / 2);

            Vector3 worldMousePosition = canvasRect.TransformPoint(localMousePosition);

            GameObject itemDisplay = Instantiate(itemDisplayPrefab, worldMousePosition, Quaternion.identity,
                canvas.transform);
            ItemDisplay display = itemDisplay.GetComponent<ItemDisplay>();
            itemDisplay.transform.SetAsLastSibling();
            display.item = slotsOnInterface[obj].GetItemObject();
            display.Initialize();
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
            MouseData.TempItemBeingDragged = CreateTempItem(obj);
        }

        private GameObject CreateTempItem(GameObject obj)
        {
            GameObject tempItem = null;
            if (slotsOnInterface[obj].item.id < 0) return tempItem;

            tempItem = new GameObject();
            var rt = tempItem.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50, 50);
            tempItem.transform.SetParent(transform.parent.parent);
            var img = tempItem.AddComponent<Image>();
            img.sprite = slotsOnInterface[obj].GetItemObject().uiDisplay;
            img.raycastTarget = false;

            return tempItem;
        }

        protected void OnDragEnd(GameObject obj)
        {
            Destroy(MouseData.TempItemBeingDragged);

            if (MouseData.InterfaceMouseIsOver == null)
            {
                OnDropItem?.Invoke(slotsOnInterface[obj].GetItemObject().characterDisplay,
                    slotsOnInterface[obj].amount);
                slotsOnInterface[obj].RemoveItem();
                InventoryObject.OnItemSwapInventory?.Invoke(0);
                return;
            }

            if (!MouseData.SlotHoveredOver) return;

            InventorySlot mouseHoverSlotData =
                MouseData.InterfaceMouseIsOver.slotsOnInterface[MouseData.SlotHoveredOver];
            inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
        }

        protected void OnDrag(GameObject obj)
        {
            if (MouseData.TempItemBeingDragged != null)
                MouseData.TempItemBeingDragged.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }
}