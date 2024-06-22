using System;
using InventorySystem;
using UnityEngine;
using UnityEngine.UI;

namespace Store.Shops
{
    public class ShopItem : MonoBehaviour
    {
        public static Action<int> OnSelectItem;
        public int itemID;
        public int price;
        public Image itemImage;
        public Button buyButton;

        public void SetItem(ItemObject itemData)
        {
            itemID = itemData.data.id;
            itemImage.sprite = itemData.uiDisplay;
            price = itemData.data.listPrice.originalPrice * 2;
            buyButton.onClick.AddListener(ShowItem);
        }

        private void ShowItem()
        {
            OnSelectItem?.Invoke(itemID);
        }
    }
}