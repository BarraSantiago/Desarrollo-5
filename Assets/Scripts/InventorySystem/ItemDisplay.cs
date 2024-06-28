using System;
using player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventorySystem
{
    public class ItemDisplay : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text price;
        [SerializeField] private TMP_Text listPrice;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button useButton;

        public static Action OnItemUpdate;
        public ItemObject item;

        private EventTrigger eventTrigger;

        public void Initialize()
        {
            icon.sprite = item.uiDisplay;
            itemName.text = item.name;
            price.text = item.price.ToString();
            listPrice.text = item.data.listPrice.CurrentPrice.ToString();
            description.text = item.description;

            eventTrigger = GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = gameObject.AddComponent<EventTrigger>();
            }

            AddPointerExitEvent();
            if(item.type == ItemType.Potion) useButton.onClick.AddListener(UseItem);
        }

        private void AddPointerExitEvent()
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((eventData) => { Close(); });

            eventTrigger.triggers.Add(entry);
        }

        public void ChangeItemPrice()
        {
            if (int.TryParse(inputField.text, out int result))
            {
                item.price = result;
                OnItemUpdate?.Invoke();
            }
            else
            {
                // TODO fail to change value
            }
        }

        private void Close()
        {
            // TODO make object pooling
            Destroy(gameObject);
        }

        private void UseItem()
        {
            foreach (var buff in item.data.buffs)
            {
                PlayerStats.OnBuffReceived?.Invoke(buff);
            }
        }
    }
}