using System;
using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Store.Shops
{
    public class ShopItem : MonoBehaviour
    {
        public static Action<int, int> OnBuyItem;
        public int itemID;
        public int databaseID;
        public int price;
        public Image itemImage;
        public Button buyButton;
        public TMP_Text priceText;

        public void SetItem(ItemObject itemData, int databaseId )
        {
            databaseID = databaseId;
            itemID = itemData.data.id;
            itemImage.sprite = itemData.uiDisplay;
            priceText.text = itemData.data.listPrice.originalPrice.ToString();
            price = itemData.data.listPrice.originalPrice * 2;
            buyButton.onClick.AddListener(BuyItem);
        }
        
        public void BuyItem()
        {
            OnBuyItem?.Invoke(databaseID, itemID);
        }
    }
}