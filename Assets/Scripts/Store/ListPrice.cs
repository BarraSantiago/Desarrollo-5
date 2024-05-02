using UnityEngine;

namespace Store
{
    public class ListPrice
    {
        [SerializeField] public int originalPrice;
        
        public int CurrentPrice { get; private set; }
        public static readonly float PriceModifier = 0.12f;
        public int amountSoldLastDay;
        public int TotalSold { get; private set; }
        public bool wasSold;
        
        
        
        /// <summary>
        /// Whenever a store cicle ends, all list prices should be updated to reflect on the offer of the object
        /// </summary>
        public void UpdatePrice()
        {
            int newPrice = (int)(originalPrice * (1 - PriceModifier * amountSoldLastDay));
            CurrentPrice = (int)(newPrice > CurrentPrice * 0.5f ? newPrice : CurrentPrice * 0.5f);

            TotalSold += amountSoldLastDay;
            amountSoldLastDay = 0;
        }
    }
}