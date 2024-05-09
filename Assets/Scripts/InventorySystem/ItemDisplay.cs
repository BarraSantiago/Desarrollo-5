using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventorySystem
{
    public class ItemDisplay : MonoBehaviour
    {
        public ItemObject item;
        public static Action OnItemUpdate;
        
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text price;
        [SerializeField] private TMP_Text listPrice;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_InputField inputField;

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
            if(int.TryParse(inputField.text, out int result))
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

        
    }
}