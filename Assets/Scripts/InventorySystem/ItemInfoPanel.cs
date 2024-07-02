using player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventorySystem
{
    public class ItemInfoPanel : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text listPrice;
        [SerializeField] private TMP_Text description;
        [SerializeField] private Button useButton;

        public ItemObject item;
        public InventorySlot slot;
        
        private EventTrigger _eventTrigger;

        public void Initialize()
        {
            icon.sprite = item.uiDisplay;
            icon.preserveAspect = true;
            itemName.text = item.name;
            listPrice.text = item.data.listPrice.CurrentPrice.ToString();
            description.text = item.description;

            _eventTrigger = GetComponent<EventTrigger>();
            if (_eventTrigger == null)
            {
                _eventTrigger = gameObject.AddComponent<EventTrigger>();
            }

            AddPointerExitEvent();
            if(item.type == ItemType.Potion) useButton.onClick.AddListener(UseItem);
        }

        private void AddPointerExitEvent()
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((eventData) => { Close(); });

            _eventTrigger.triggers.Add(entry);
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
            // TODO drink potion sound
            slot.UpdateSlot(slot.item, slot.amount - 1);
            if(slot.amount <= 0) Close();
        }
    }
}