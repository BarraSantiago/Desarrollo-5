using System.Collections.Generic;
using InventorySystem;
using player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Store.Shops
{
    public class Shop : MonoBehaviour
    {
        [SerializeField] private ItemDatabaseObject[] databases;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Player player;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private Transform shopItemsParent;

        private List<ShopItem> _shopItems = new List<ShopItem>();
        private int _shopLevel;
        private int _shopMaxLevel;
        private int _shopUpgradeCost;
        private int _shopUpgradeCostMultiplier;
        private int _itemCostMultiplier;

        private void Start()
        {
            ShopItem.OnBuyItem += BuyItem;
            _shopLevel = 1;
            _shopMaxLevel = databases.Length;
            _shopUpgradeCost = 150;
            _shopUpgradeCostMultiplier = 5;
            _shopUpgradeCost *= _shopUpgradeCostMultiplier * _shopLevel;
            costText.text = _shopUpgradeCost.ToString();

            upgradeButton.onClick.AddListener(UpgradeShop);
            ListItems();

            UpdateAvailability(player.money);
            Player.OnMoneyUpdate += UpdateAvailability;
            ShopItem.OnBuyItem += BuyItem;
        }

        private void BuyItem(int databaseId, int itemId)
        {
            var item = databases[databaseId].ItemObjects[itemId];
            if (player.money < item.data.listPrice.originalPrice)
            {
                return;
            }

            player.money -= item.data.listPrice.originalPrice;
            player.inventory.AddItem(item.data, 1);
        }

        public void UpdateAvailability(int money)
        {
            costText.color = money < _shopUpgradeCost ? Color.red : Color.green;
            foreach (var item in _shopItems)
            {
                item.priceText.color = money < item.price ? Color.red : Color.green;
            }
        }

        private void CheckLevel()
        {
            if (_shopLevel < _shopMaxLevel) return;
            upgradeButton.interactable = false;
            costText.text = "Max Level";
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
            costText.text = _shopUpgradeCost.ToString();
            _shopLevel++;
        }

        private void ListItems()
        {
            for (int i = 0; i < _shopLevel; i++)
            {
                var itemDatabase = databases[i];
                for (int j = 0; j < itemDatabase.ItemObjects.Length; j++)
                {
                    var shopItem = Instantiate(shopItemPrefab, shopItemsParent);
                    ShopItem newItem = shopItem.GetComponent<ShopItem>();
                    newItem.SetItem(itemDatabase.ItemObjects[j], i);
                    _shopItems.Add(newItem);
                }
            }
        }
    }
}