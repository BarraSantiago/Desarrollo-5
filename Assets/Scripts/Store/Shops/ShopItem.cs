using System;
using System.Globalization;
using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Store.Shops
{
    public class ShopItem : MonoBehaviour
    {
        public Action<int> OnSelectItem;
        public int itemID;
        public Image itemImage;
        public TMP_Text price;
        public TMP_Text itemName;
        public Button buyButton;

        public void SetItem(ItemObject itemData, float itemCostMultiplier, bool isCraftingShop)
        {
            itemID = itemData.data.id;
            itemImage.sprite = itemData.uiDisplay;
            itemName.text = itemData.data.name;
            price.text = isCraftingShop ? itemData.data.recipe.showPrice.ToString(CultureInfo.InvariantCulture) :
                ((int)(itemData.data.listPrice.originalPrice * itemCostMultiplier)).ToString(CultureInfo.InvariantCulture);
            buyButton.onClick.AddListener(ShowItem);
            itemImage.preserveAspect = true;
        }

        private void ShowItem()
        {
            OnSelectItem?.Invoke(itemID);
        }
    }
}