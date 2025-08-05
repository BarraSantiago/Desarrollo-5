using InventorySystem;
using TMPro;
using UnityEngine;

namespace Store
{
    public class DisplayItem : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] public InventoryObject inventory;
        [SerializeField] public GameObject inventoryParent;
        [SerializeField] private GameObject showEmpty;

        public Transform itemPosition;
        public int id;
        public int amount;
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
                    _item.data.OnPriceChange -= UpdatePrice;
                }

                _item = value;

                if (_item)
                {
                    _item.data.OnPriceChange += UpdatePrice;
                }
            }
        }

        private const int MaxAmount = 99999;
        private const int MinAmount = 1;

        private void Start()
        {
            inputField.characterLimit = 5;

            inputField.onEndEdit.AddListener(delegate { ChangeItemPrice(); });
            inputField.onDeselect.AddListener(delegate { ChangeItemPrice(); });
            inputField.onSelect.AddListener(delegate { OnSelectInput(); });

            inventoryParent.SetActive(true);

            if (inventory.GetSlots[0].GetItemObject())
            {
                Initialize(inventory.GetSlots[0].GetItemObject());
            }
            else
            {
                CleanDisplay();
            }

            inventory.UpdateInventory();
            inventoryParent.SetActive(false);
        }

        private void OnEnable()
        {
            inventory.UpdateInventory();
        }

        public void Initialize()
        {
            inventoryParent.SetActive(true);

            foreach (InventorySlot slot in inventory.GetSlots)
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

            inventoryParent.SetActive(false);
        }

        public void Initialize(ItemObject newItem)
        {
            UpdateSlot(inventory.GetSlots[0]);
        }

        private void UpdateSlot(InventorySlot slot)
        {
            if (displayObject) Destroy(displayObject);
            if (!slot.GetItemObject() || slot.amount == 0)
            {
                UpdateShowEmpty();
                CleanDisplay();
                return;
            }

            amount = slot.amount;
            Item = slot.GetItemObject();
            CreateDisplayItem(Item);
            UpdatePrice();
            amountText.text = amount.ToString();
            UpdateShowEmpty();
        }

        public void CleanDisplay()
        {
            if (displayObject) Destroy(displayObject);
            Item = null;
            amount = 0;
            showPrice.text = "";
            inputField.text = "";
            totalPriceText.text = "";
            amountText.text = "";
            BeingViewed = false;
            Bought = false;
            UpdateShowEmpty();
        }

        private void CreateDisplayItem(ItemObject itemObject)
        {
            displayObject = Instantiate(itemObject.characterDisplay, itemPosition);
            displayObject.GetComponent<BoxCollider>().enabled = false;
        }

        private void OnSelectInput()
        {
            inputField.text = inputField.text.Replace("$", "");
        }


        private void ChangeItemPrice()
        {
            string inputText = inputField.text.Replace("$", "");

            if (!int.TryParse(inputText, out int result))
            {
                result = MinAmount;
            }

            // Handle negative numbers and enforce maximum 5 digits (99999)
            if (result <= 0)
            {
                result = MinAmount;
            }
            else if (result > MaxAmount)
            {
                result = MaxAmount;
            }

            inputField.text = result.ToString();
            Item.Price = result;
            UpdatePrice();
        }

        private void UpdatePrice()
        {
            inputField.text = Item.Price.ToString();
            showPrice.text = Item.Price.ToString();
            totalPriceText.text = (Item.Price * amount).ToString();
        }

        private void UpdateShowEmpty()
        {
            if (!showEmpty) return;

            showEmpty.SetActive(!_item || amount == 0);
        }
    }
}