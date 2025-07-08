using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using InventorySystem;
using player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

namespace Store.Shops
{
    public class Shop : MonoBehaviour, IInitializable
    {
        #region Serialized Fields

        [Header("Shop setup")] 
        [SerializeField] private ItemDatabaseObject completeDatabase;
        [SerializeField] private ItemIdDatabaseObject[] databases;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Player player;
        [SerializeField] private TMP_Text upgradeText;
        [SerializeField] private bool isCraftingShop;
        [SerializeField] private string ShopLevelKey = "ShopLevel";
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private GameObject errorPopup;

        [Header("Item setup")] 
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private Image selectedItemImage;
        [SerializeField] private Transform shopItemsParent;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button increaseAmountButton;
        [SerializeField] private Button decreaseAmountButton;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text craftChanceText;
        [SerializeField] private ShopRecipe[] shopRecipes;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text availableText;
        [SerializeField] private float itemCostMultiplier = 0.8f;
        [SerializeField] private int ForceShopLevel = 0;
        
        [Header("Item crafting setup")] 
        [SerializeField] private GameObject messageWindow;
        [SerializeField] private GameObject SuccessWindow;
        [SerializeField] private GameObject FailWindow;
        [SerializeField] private Image FailImage;
        [SerializeField] private Image SuccessImage;

        #endregion

        #region Properties

        private List<ShopItem> _shopItems = new List<ShopItem>();
        private int _shopLevel;
        private int _shopMaxLevel;
        private int _shopUpgradeCost;
        private int _shopUpgradeCostMultiplier;

        private int CurrentCost
        {
            get => _currentCost;
            set
            {
                _currentCost = value;
                UpdateTextColor();
                costText.text = _currentCost.ToString();
            }
        }

        private int _currentAmount = 1;
        private float _craftChance = 0;

        private float CraftChanceMultiplier
        {
            get => _craftChanceMultiplier;
            set
            {
                _craftChanceMultiplier = value;
                UpdateTextColor();
                craftChanceText.text = (_craftChance * _craftChanceMultiplier).ToString("0.##") + "%";
            }
        }

        private ItemObject _selectedItem;
        private int _currentCost;
        private float _craftChanceMultiplier = 1;
        private const int MaxAmount = 999999;
        private const int MinAmount = 0;

        #endregion



        private void Awake()
        {
            _shopMaxLevel = databases.Length + 1;
            _shopLevel = ForceShopLevel > 0 ? ForceShopLevel : PlayerPrefs.GetInt(ShopLevelKey, 1);

            CheckLevel();
        }

        private void Start()
        {
            _shopUpgradeCost = 150;
            _shopUpgradeCostMultiplier = 3;
            _shopUpgradeCost *= _shopUpgradeCostMultiplier * _shopLevel;
            if (amountText) amountText.text = _currentAmount.ToString();
            if (upgradeText) upgradeText.text = _shopUpgradeCost.ToString();


            ListItems();

            UpdateAvailability(player.Money);

            selectedItemImage.preserveAspect = true;
            SelectItem(databases[0].ItemObjects[0].data.id);
        }

        private void OnEnable()
        {
            UpdateAvailability(player.Money);
        }

        public void ResetAmount()
        {
            _currentAmount = 1;
            if (amountText) amountText.text = _currentAmount.ToString();
            if (_selectedItem)
                CurrentCost = (int)(_selectedItem.data.listPrice.originalPrice * itemCostMultiplier * _currentAmount);
        }

        private void SelectItem(int itemId)
        {
            _selectedItem = completeDatabase.ItemObjects[itemId];
            selectedItemImage.sprite = _selectedItem.uiDisplay;
            ResetAmount();


            int recipeItemsLength = _selectedItem.data.recipe?.items?.Length ?? 0;

            if (_selectedItem.data.craftable && _selectedItem.data.recipe)
            {
                for (int i = 0; i < shopRecipes.Length; i++)
                {
                    shopRecipes[i].gameObject.SetActive(i < recipeItemsLength);
                }

                for (int i = 0; i < _selectedItem.data.recipe.items!.Length; i++)
                {
                    ItemObject itemRecipe = completeDatabase.ItemObjects[_selectedItem.data.recipe.items[i].itemID];
                    shopRecipes[i].SetRecipe(itemRecipe, _selectedItem.data.recipe.items[i].amount,
                        player.inventory.GetItemCount(itemRecipe.data));
                }

                _craftChance = completeDatabase.ItemObjects[_selectedItem.data.id].data.recipe.craftChance;
                craftChanceText.text =
                    (_craftChance * _craftChanceMultiplier).ToString(CultureInfo.CurrentCulture) + "%";
                CurrentCost = 0;
                inputField.text = 0.ToString();
                CraftChanceMultiplier = 1;
            }
            else
            {
                CurrentCost = (int)(_selectedItem.data.listPrice.originalPrice * itemCostMultiplier * _currentAmount);
            }
        }

        private void BuyItem()
        {
            string itemAction = isCraftingShop ? "craft" : "buy";
            if (player.Money < CurrentCost)
            {
                AudioManager.instance.Play("Error");
                errorPopup.SetActive(true);
                errorText.text = $"You don't have enough money to {itemAction} {_selectedItem.data.name}!";
                return;
            }
            
            if(player.inventory.IsFull(_selectedItem.data, _currentAmount))
            {
                AudioManager.instance.Play("Error");
                errorPopup.SetActive(true);
                errorText.text = $"You don't have enough space in your inventory to {itemAction} {_selectedItem.data.name}!";
                return;
            }

            if (_selectedItem.data.craftable)
            {
                if (!CraftItem()) return;
            }
            else
            {
                player.Money -= CurrentCost;
                player.inventory.AddItem(_selectedItem.data, _currentAmount);
            }

            UpdateAvailability(player.Money);
        }

        private bool CraftItem()
        {
            if (!CheckRecipe()) return false;
            foreach (ItemRecipe.ItemEntry itemEntry in _selectedItem.data.recipe.items)
            {
                ItemObject currentEntry = completeDatabase.ItemObjects[itemEntry.itemID];
                player.inventory.RemoveItem(currentEntry.data, itemEntry.amount);
            }

            messageWindow.SetActive(true);

            if (Random.Range(0, 100) < _craftChance * CraftChanceMultiplier)
            {
                player.inventory.AddItem(_selectedItem.data, _currentAmount);
                SuccessWindow.SetActive(true);
                SuccessImage.sprite = _selectedItem.uiDisplay;
                AudioManager.instance.Play("CraftingSuccess");
            }
            else
            {
                FailWindow.SetActive(true);
                FailImage.sprite = _selectedItem.uiDisplay;
                AudioManager.instance.Play("CraftingFail");
            }

            return true;
        }

        private bool CheckRecipe()
        {
            return !(from itemEntry in _selectedItem.data.recipe.items
                let currentEntry = completeDatabase.ItemObjects[itemEntry.itemID]
                where player.inventory.GetItemCount(currentEntry.data) < itemEntry.amount
                select itemEntry).Any();
        }

        private void ChangeCraftChanceMod()
        {
            if (int.TryParse(inputField.text, out int newPrice))
            {
                newPrice = newPrice switch
                {
                    < MinAmount => MinAmount,
                    > MaxAmount => MaxAmount,
                    _ => newPrice
                };
                inputField.text = newPrice.ToString();

                CurrentCost = newPrice;
                CraftChanceMultiplier = ((float)newPrice / _selectedItem.data.listPrice.originalPrice) + 1;

                if (!(CraftChanceMultiplier * _selectedItem.data.recipe.craftChance > 100)) return;

                SetMaxCraftChance();
            }
            else
            {
                inputField.text = 0.ToString();
                CraftChanceMultiplier = 1;
                CurrentCost = 0;
            }
        }

        private void SetMaxCraftChance()
        {
            CraftChanceMultiplier = 100 / _selectedItem.data.recipe.craftChance;
            inputField.text = (_selectedItem.data.listPrice.originalPrice * CraftChanceMultiplier).ToString("0");
            CurrentCost = (int)(_selectedItem.data.listPrice.originalPrice * CraftChanceMultiplier);
        }


        private void UpdateAvailability(int money)
        {
            UpdateTextColor();

            CheckLevel();

            CheckCraftingMaterials();
        }

        private void CheckCraftingMaterials(int slotIndex = -1)
        {
            if (!_selectedItem || !_selectedItem.data.craftable) return;

            for (int i = 0; i < _selectedItem.data.recipe.items.Length; i++)
            {
                ItemObject currentEntry = completeDatabase.ItemObjects[_selectedItem.data.recipe.items[i].itemID];
                shopRecipes[i].UpdateAbailability(_selectedItem.data.recipe.items[i].amount,
                    player.inventory.GetItemCount(currentEntry.data));
            }
        }

        private void UpdateTextColor()
        {
            costText.color = player.Money < CurrentCost ? Color.red : Color.green;
            if (upgradeText && _shopLevel < _shopMaxLevel)
            {
                upgradeText.color = player.Money < _shopUpgradeCost ? Color.red : Color.green;
            }
        }

        private void CheckLevel()
        {
            // Check if shop level is limited by popularity level
            int maxAllowableLevel = 1; // Base level

            if (PopularityManager.Level >= 1)
                maxAllowableLevel = 2; // Allow first upgrade
            if (PopularityManager.Level >= 2)
                maxAllowableLevel = 3; // Allow second upgrade
            if (PopularityManager.Level >= 4)
                maxAllowableLevel = _shopMaxLevel; // Allow all upgrades

            if (_shopLevel < _shopMaxLevel - 1 && _shopLevel < maxAllowableLevel)
            {
                if (upgradeButton) upgradeButton.interactable = true;
                return;
            }

            if (upgradeButton) upgradeButton.interactable = false;

            // Show appropriate message based on whether popularity level or max level is limiting factor
            if (_shopLevel >= maxAllowableLevel && _shopLevel < _shopMaxLevel - 1)
            {
                string levelText = maxAllowableLevel switch
                {
                    1 => "Silver",
                    2 => "Gold",
                    3 => "Emerald",
                    _ => "Max Level"
                };
                if (availableText) availableText.text = $"Prestige Level: {levelText}";
                if (availableText) availableText.color = Color.yellow;
            }
            else
            {
                if (availableText) availableText.text = "Max Level";
                if (upgradeText) upgradeText.text = "Max Level";
                if (availableText) availableText.color = Color.gray;
                if (upgradeText) upgradeText.color = Color.gray;
            }
        }

        private void UpgradeShop()
        {
            if (_shopLevel >= _shopMaxLevel)
            {
                upgradeButton.interactable = false;
                return;
            }

            if (player.Money < _shopUpgradeCost)
            {
                return;
            }

            player.Money -= _shopUpgradeCost;
            _shopUpgradeCost *= _shopUpgradeCostMultiplier;
            upgradeText.text = _shopUpgradeCost.ToString();
            ListDatabaseItems(databases[_shopLevel]);
            _shopLevel++;
            PlayerPrefs.SetInt(ShopLevelKey, _shopLevel);
            UpdateAvailability(player.Money);
        }

        private void ListItems()
        {
            for (int i = 0; i < _shopLevel; i++)
            {
                if (databases.Length <= i) continue;
                ListDatabaseItems(databases[i]);
            }
        }

        private void ListDatabaseItems(ItemIdDatabaseObject database)
        {
            foreach (ItemObject itemObject in database.ItemObjects)
            {
                GameObject shopItem = Instantiate(shopItemPrefab, shopItemsParent);
                ShopItem newItem = shopItem.GetComponent<ShopItem>();
                newItem.SetItem(itemObject, itemCostMultiplier);
                newItem.OnSelectItem += SelectItem;
                _shopItems.Add(newItem);
            }
        }

        private void DecreaseAmount()
        {
            if (_currentAmount <= 1) return;
            _currentAmount--;
            amountText.text = _currentAmount.ToString();
            CurrentCost = (int)(_selectedItem.data.listPrice.originalPrice * itemCostMultiplier * _currentAmount);
        }

        private void IncreaseAmount()
        {
            _currentAmount++;
            amountText.text = _currentAmount.ToString();
            CurrentCost = (int)(_selectedItem.data.listPrice.originalPrice * itemCostMultiplier * _currentAmount);
        }

        public void Initialize()
        {
            foreach (ShopItem shopItem in _shopItems)
            {
                shopItem.OnSelectItem += SelectItem;
            }

            Player.OnMoneyUpdate += UpdateAvailability;

            player.inventory.OnItemAdded += CheckCraftingMaterials;
            upgradeButton?.onClick.AddListener(UpgradeShop);
            buyButton.onClick.AddListener(BuyItem);
            increaseAmountButton?.onClick.AddListener(IncreaseAmount);
            decreaseAmountButton?.onClick.AddListener(DecreaseAmount);

            if (!isCraftingShop) return;

            inputField.onEndEdit.AddListener(delegate { ChangeCraftChanceMod(); });
            inputField.onDeselect.AddListener(delegate { ChangeCraftChanceMod(); });
        }

        public void Deinitialize()
        {
            foreach (ShopItem shopItem in _shopItems)
            {
                shopItem.OnSelectItem = null;
            }

            Player.OnMoneyUpdate -= UpdateAvailability;
            player.inventory.OnItemAdded -= CheckCraftingMaterials;

            upgradeButton?.onClick.RemoveListener(UpgradeShop);
            buyButton.onClick.RemoveListener(BuyItem);
            increaseAmountButton?.onClick.RemoveListener(IncreaseAmount);
            decreaseAmountButton?.onClick.RemoveListener(DecreaseAmount);

            if (!isCraftingShop) return;
            inputField.onEndEdit.RemoveListener(delegate { ChangeCraftChanceMod(); });
            inputField.onDeselect.RemoveListener(delegate { ChangeCraftChanceMod(); });
        }
    }
}