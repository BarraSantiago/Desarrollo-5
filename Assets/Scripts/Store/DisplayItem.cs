using InventorySystem;
using TMPro;
using UnityEngine;

namespace Store
{
    public class DisplayItem
    {
        public bool BeingViewed { get; set; }
        public bool Bought { get; set; }
        public TMP_Text showPrice;
        public GameObject Object;
        public ItemObject ItemObject;
        public int id;
        public int amount;

        public void Initialize(int price)
        {
            showPrice.text = "$" + price.ToString();
        }
    }
}