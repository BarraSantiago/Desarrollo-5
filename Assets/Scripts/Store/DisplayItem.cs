using InventorySystem;
using TMPro;
using UnityEngine;

namespace Store
{
    public class DisplayItem : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] public InventoryObject inventory;
        public bool BeingViewed { get; set; }
        public bool Bought { get; set; }
        public TMP_Text showPrice;
        public TMP_Text amountText;
        public TMP_Text totalPriceText;
        public GameObject displayObject;
        private ItemObject _item;
        public ItemObject Item
        {
            get => _item;
            private set
            {
                if (_item)
                {
                    _item.data.OnPriceChange -= ChangePrice;
                }

                _item = value;

                if (_item)
                {
                    _item.data.OnPriceChange += ChangePrice;
                }
            }
        }

        private void ChangePrice()
        {
            showPrice.text = "$" + Item.price;
            totalPriceText.text = "$" + (Item.price * amount);
        }

        public Transform itemPosition;
        public int id;
        public int amount;

        private const int MaxAmount = 999999;
        private const int MinAmount = 1;

        private void Start()
        {
            inputField.onEndEdit.AddListener(delegate { ChangeItemPrice(); });
            inputField.onDeselect.AddListener(delegate { ChangeItemPrice(); });
            inputField.onSelect.AddListener(delegate { OnSelectInput(); });
            
            foreach (var slot in inventory.GetSlots)
            {
                slot.onAfterUpdated += UpdateSlot;
            }
            
            inventory.UpdateInventory();
            
            if (inventory.GetSlots[0].GetItemObject())
            {
                Initialize(inventory.GetSlots[0].GetItemObject());
            }
            else
            {
                CleanDisplay();
            }
        }

        private void OnEnable()
        {
            inventory.UpdateInventory();
        }

        public void Initialize()
        {
            foreach (var slot in inventory.GetSlots)
            {
                slot.onAfterUpdated += UpdateSlot;
            }
            inventory.UpdateInventory();
            if (inventory.GetSlots[0].GetItemObject())
            {
                Initialize(inventory.GetSlots[0].GetItemObject());
            }
            else
            {
                CleanDisplay();
            }
        }
        
        public void Initialize(ItemObject newItem)
        {
            Item = newItem;
            UpdateSlot(inventory.GetSlots[0]);
        }

        private void UpdateSlot(InventorySlot slot)
        {
            if(displayObject) Destroy(displayObject);
            if (!slot.GetItemObject() || slot.amount == 0)
            {
                CleanDisplay();
                return;
            }
            amount = slot.amount;
            Item = slot.GetItemObject();
            CreateDisplayItem(Item);
            showPrice.text = "$" + Item.price;
            inputField.text = "$" + Item.price;
            totalPriceText.text = "$" + (Item.price * amount);
            amountText.text = amount.ToString();
        }

        public void CleanDisplay()
        {
            if(displayObject) Destroy(displayObject);
            Item = null;
            amount = 0;
            showPrice.text = "";
            inputField.text = "";
            totalPriceText.text = "";
            amountText.text = "";
        }

        private void CreateDisplayItem(ItemObject itemObject)
        {
            displayObject = Instantiate(itemObject.characterDisplay, itemPosition);
            displayObject.GetComponent<GroundItem>().enabled = false;
            displayObject.GetComponent<BoxCollider>().enabled = false;
        }

        public void OnSelectInput()
        {
            inputField.text = inputField.text.Replace("$", "");
        }
        
        
        public void ChangeItemPrice()
        {
            if (!int.TryParse(inputField.text, out int result))
            {
                OnSelectInput();
                if (!int.TryParse(inputField.text, out result))
                {
                    return;
                }
            }
            
            inputField.text = result switch
            {
                < MinAmount => MinAmount.ToString(),
                > MaxAmount => MaxAmount.ToString(),
                _ => "$" + inputField.text
            };
            
            Item.price = result;
            inputField.text = "$" + Item.price;
            showPrice.text = "$" + Item.price;
            totalPriceText.text = "$" + (Item.price * amount);
        }
    }
}
