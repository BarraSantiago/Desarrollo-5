using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using InventorySystem;
using player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

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
        [SerializeField] private float itemCostMultiplier = 0.8f;

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
        private const int MinAmount = 1;
        
        #endregion


        private void Start()
        {
            _shopLevel = 1;
            _shopMaxLevel = databases.Length;
            _shopUpgradeCost = 150;
            _shopUpgradeCostMultiplier = 5;
            _shopUpgradeCost *= _shopUpgradeCostMultiplier * _shopLevel;
            if (amountText) amountText.text = _currentAmount.ToString();
            if (upgradeText) upgradeText.text = _shopUpgradeCost.ToString();
            
            
            ListItems();

            UpdateAvailability(player.money);

            selectedItemImage.preserveAspect = true;
            SelectItem(databases[0].ItemObjects[0].data.id);
        }
        
        private void OnEnable()
        {
            UpdateAvailability(player.money);
        }

        private void SelectItem(int itemId)
        {
            _selectedItem = completeDatabase.ItemObjects[itemId];
            selectedItemImage.sprite = _selectedItem.uiDisplay;


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
                        player.inventory.GetItemCount(_selectedItem.data));
                }

                _craftChance = completeDatabase.ItemObjects[_selectedItem.data.id].data.recipe.craftChance;
                craftChanceText.text = (_craftChance * _craftChanceMultiplier).ToString(CultureInfo.CurrentCulture) + "%";
                CurrentCost = _selectedItem.data.listPrice.originalPrice / 3;
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
            if (player.money < CurrentCost)
            {
                return;
            }

            if (_selectedItem.data.craftable)
            {
                if (!CraftItem()) return;
            }
            else
            {
                player.money -= CurrentCost;
                player.inventory.AddItem(_selectedItem.data, _currentAmount);
            }
            
            UpdateAvailability(player.money);
        }

        private bool CraftItem()
        {
            if (!CheckRecipe()) return false;
            foreach (var itemEntry in _selectedItem.data.recipe.items)
            {
                ItemObject currentEntry = completeDatabase.ItemObjects[itemEntry.itemID];
                player.inventory.RemoveItem(currentEntry.data, itemEntry.amount);
            }
            
            if (Random.Range(0, 100) < _craftChance * CraftChanceMultiplier)
            {
                player.inventory.AddItem(_selectedItem.data, _currentAmount);
                AudioManager.instance.Play("CraftingSuccess");
            }
            else
            {
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
                inputField.text = newPrice switch
                {
                    < MinAmount => MinAmount.ToString(),
                    > MaxAmount => MaxAmount.ToString(),
                    _ => inputField.text
                };
                
                CurrentCost = _selectedItem.data.listPrice.originalPrice / 3 + newPrice;
                CraftChanceMultiplier = ((float)newPrice / _selectedItem.data.listPrice.originalPrice) + 1;

                if (!(CraftChanceMultiplier * _selectedItem.data.recipe.craftChance > 100)) return;

                SetMaxCraftChance();
            }
            else
            {
                inputField.text = 0.ToString();
                CraftChanceMultiplier = 1;
                CurrentCost = _selectedItem.data.listPrice.originalPrice / 3;
            }
        }

        private void SetMaxCraftChance()
        {
            CraftChanceMultiplier = 100 / _selectedItem.data.recipe.craftChance;
            inputField.text = (_selectedItem.data.listPrice.originalPrice * CraftChanceMultiplier).ToString("0.##");
            CurrentCost = _selectedItem.data.listPrice.originalPrice / 3 + (int)(_selectedItem.data.listPrice.originalPrice * CraftChanceMultiplier);
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
            costText.color = player.money < CurrentCost ? Color.red : Color.green;
            if (upgradeText) upgradeText.color = player.money < _shopUpgradeCost ? Color.red : Color.green;
        }

        private void CheckLevel()
        {
            if (_shopLevel < _shopMaxLevel) return;

            if (upgradeButton) upgradeButton.interactable = false;
            if (upgradeText) upgradeText.text = "Max Level";
            if (upgradeText) upgradeText.color = Color.gray;
        }

        private void UpgradeShop()
        {
            if (_shopLevel >= _shopMaxLevel)
            {
                upgradeButton.interactable = false;
                return;
            }

            if (player.money < _shopUpgradeCost)
            {
                return;
            }

            player.money -= _shopUpgradeCost;
            _shopUpgradeCost *= _shopUpgradeCostMultiplier;
            upgradeText.text = _shopUpgradeCost.ToString();
            _shopLevel++;
        }

        private void ListItems()
        {
            for (int i = 0; i < _shopLevel; i++)
            {
                var itemDatabase = databases[i];
                foreach (var itemObject in itemDatabase.ItemObjects)
                {
                    var shopItem = Instantiate(shopItemPrefab, shopItemsParent);
                    ShopItem newItem = shopItem.GetComponent<ShopItem>();
                    newItem.SetItem(itemObject, itemCostMultiplier);
                    _shopItems.Add(newItem);
                }
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
            foreach (var shopItem in _shopItems)
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
            foreach (var shopItem in _shopItems)
            {
                shopItem.OnSelectItem -= SelectItem;
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