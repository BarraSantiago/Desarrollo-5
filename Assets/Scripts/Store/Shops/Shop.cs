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

        [Header("Item setup")] 
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private Image selectedItemImage;
        [SerializeField] private Transform shopItemsParent;
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
        private int _currentCost;
        private int _currentAmount = 1;
        private ItemObject _selectedItem;

        #endregion


        private void Start()
        {
            _shopLevel = 1;
            _shopMaxLevel = databases.Length;
            _shopUpgradeCost = 150;
            _shopUpgradeCostMultiplier = 5;
            _shopUpgradeCost *= _shopUpgradeCostMultiplier * _shopLevel;
            if(amountText) amountText.text = _currentAmount.ToString();
            if(upgradeText) upgradeText.text = _shopUpgradeCost.ToString();

            upgradeButton?.onClick.AddListener(UpgradeShop);
            buyButton.onClick.AddListener(BuyItem);
            increaseAmountButton?.onClick.AddListener(IncreaseAmount);
            decreaseAmountButton?.onClick.AddListener(DecreaseAmount);
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
                craftChanceText.text = _selectedItem.data.recipe.craftChance.ToString(CultureInfo.CurrentCulture) + "%";
                _currentCost = _selectedItem.data.listPrice.originalPrice / 2;
            }
            else
            {
                _currentCost = (int)(_selectedItem.data.listPrice.originalPrice * itemCostMultiplier * _currentAmount);
            }

            UpdateTextColor();
            costText.text = _currentCost.ToString();
        }

        private void BuyItem()
        {
            if (player.money < _currentCost)
            {
                return;
            }

            switch (_selectedItem.data.craftable)
            {
                case true when !CheckRecipe():
                    return;
                case true:
                {
                    foreach (var itemEntry in _selectedItem.data.recipe.items)
                    {
                        ItemObject currentEntry = completeDatabase.ItemObjects[itemEntry.itemID];
                        player.inventory.RemoveItem(currentEntry.data, itemEntry.amount);
                    }

                    break;
                }
            }
            
            player.money -= _currentCost;
            player.inventory.AddItem(_selectedItem.data, _currentAmount);
            UpdateAvailability(player.money);
        }

        private bool CheckRecipe()
        {
            return !(from itemEntry in _selectedItem.data.recipe.items
                let currentEntry = completeDatabase.ItemObjects[itemEntry.itemID]
                where player.inventory.GetItemCount(currentEntry.data) < itemEntry.amount
                select itemEntry).Any();
        }

        private void UpdateAvailability(int money)
        {
           UpdateTextColor();

           CheckLevel();

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
            costText.color = player.money < _currentCost ? Color.red : Color.green;
            if(upgradeText) upgradeText.color = player.money < _shopUpgradeCost ? Color.red : Color.green;
        }

        private void CheckLevel()
        {
            if (_shopLevel < _shopMaxLevel) return;
            
            if(upgradeButton) upgradeButton.interactable = false;
            if(upgradeText) upgradeText.text = "Max Level";
            if(upgradeText) upgradeText.color = Color.gray;
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
            _currentCost = (int)(_selectedItem.data.listPrice.originalPrice * itemCostMultiplier * _currentAmount);
            UpdateTextColor();
            costText.text = _currentCost.ToString();
        }

        private void IncreaseAmount()
        {
            _currentAmount++;
            amountText.text = _currentAmount.ToString();
            _currentCost = (int)(_selectedItem.data.listPrice.originalPrice * itemCostMultiplier * _currentAmount);
            UpdateTextColor();
            costText.text = _currentCost.ToString();
        }

        public void Initialize()
        {
            foreach (var shopItem in _shopItems)
            {
                shopItem.OnSelectItem += SelectItem;
            }
            Player.OnMoneyUpdate += UpdateAvailability;
            
        }

        public void Deinitialize()
        {
            foreach (var shopItem in _shopItems)
            {
                shopItem.OnSelectItem -= SelectItem;
            }
            Player.OnMoneyUpdate -= UpdateAvailability;
         }
    }
}