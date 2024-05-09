using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Linq;
using InventorySystem;

[RequireComponent(typeof(EventTrigger))]
public abstract class UserInterface : MonoBehaviour
{
    public InventoryObject inventory;
    public Dictionary<GameObject, InventorySlot> slotsOnInterface = new Dictionary<GameObject, InventorySlot>();
    
    [SerializeField] private GameObject itemDisplayPrefab;
    private InventoryObject _previousInventory;

    public void OnEnable()
    {
        CreateSlots();
        for (int i = 0; i < inventory.GetSlots.Length; i++)
        {
            inventory.GetSlots[i].parent = this;
            inventory.GetSlots[i].onAfterUpdated += OnSlotUpdate;
        }

        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
    }

    public abstract void CreateSlots();

    public void UpdateInventoryLinks()
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
        MouseData.slotHoveredOver = obj;
    }
    
    public void OnRightClick(GameObject obj, BaseEventData data)
    {
        if (data is PointerEventData { button: PointerEventData.InputButton.Right })
        {
            // TODO add right click functionality
            if (slotsOnInterface[obj].item.id < 0) return;
            
            Canvas canvas = FindObjectOfType<Canvas>();

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 itemDisplaySize = itemDisplayPrefab.GetComponent<RectTransform>().sizeDelta;

            Vector2 localMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out localMousePosition);

            localMousePosition.x = Mathf.Clamp(localMousePosition.x, -canvasRect.sizeDelta.x / 2 + itemDisplaySize.x / 2, canvasRect.sizeDelta.x / 2 - itemDisplaySize.x / 2);
            localMousePosition.y = Mathf.Clamp(localMousePosition.y, -canvasRect.sizeDelta.y / 2 + itemDisplaySize.y / 2, canvasRect.sizeDelta.y / 2 - itemDisplaySize.y / 2);

            Vector3 worldMousePosition = canvasRect.TransformPoint(localMousePosition);

            GameObject itemDisplay = Instantiate(itemDisplayPrefab, worldMousePosition, Quaternion.identity, canvas.transform);
            ItemDisplay display = itemDisplay.GetComponent<ItemDisplay>();
            display.item = slotsOnInterface[obj].GetItemObject();
            display.Initialize();
        }
    }

    public void OnEnterInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = obj.GetComponent<UserInterface>();
    }

    public void OnExitInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = null;
    }

    public void OnExit(GameObject obj)
    {
        MouseData.slotHoveredOver = null;
    }

    public void OnDragStart(GameObject obj)
    {
        MouseData.tempItemBeingDragged = CreateTempItem(obj);
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

    public void OnDragEnd(GameObject obj)
    {
        Destroy(MouseData.tempItemBeingDragged);

        if (MouseData.interfaceMouseIsOver == null)
        {
            //TODO drop item on ground
            slotsOnInterface[obj].RemoveItem();
            InventoryObject.OnItemSwapInventory?.Invoke(0);
            return;
        }

        if (MouseData.slotHoveredOver)
        {
            InventorySlot mouseHoverSlotData =
                MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver];
            inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
        }
    }

    public void OnDrag(GameObject obj)
    {
        if (MouseData.tempItemBeingDragged != null)
            MouseData.tempItemBeingDragged.GetComponent<RectTransform>().position = Input.mousePosition;
    }
}