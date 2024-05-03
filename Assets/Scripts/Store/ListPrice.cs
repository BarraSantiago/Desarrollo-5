using System;

namespace Store
{
    [System.Serializable]
    public class ListPrice
    {
        public int originalPrice;
        public int CurrentPrice { get; private set; }
        [NonSerialized] public static readonly float PriceModifier = 0.12f;
        [NonSerialized] public int amountSoldLastDay;
        [NonSerialized] public bool wasSold;
        public int TotalSold { get; private set; }

        public ListPrice(int originalPrice)
        {
            this.originalPrice = originalPrice;
            CurrentPrice = originalPrice;
        }

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