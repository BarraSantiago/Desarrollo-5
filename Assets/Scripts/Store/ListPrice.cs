using UnityEngine;

namespace Store
{
    [CreateAssetMenu(fileName = "ListPrice")]
    public class ListPrice : ScriptableObject
    {
        [SerializeField] public int itemId;
        [SerializeField] private int _originalPrice;
        public int CurrentPrice { get; private set; }
        public static readonly float PriceModifier = 0.12f;
        public int amountSoldLastDay;
        
        private int _totalSold;

        /// <summary>
        /// Whenever a store cicle ends, all list prices should be updated to reflect on the offer of the object
        /// </summary>
        public void UpdatePrice()
        {
            int newPrice = (int)(_originalPrice * (1 - PriceModifier * amountSoldLastDay));
            CurrentPrice = (int)(newPrice > CurrentPrice * 0.5f ? newPrice : CurrentPrice * 0.5f);

            _totalSold += amountSoldLastDay;
            amountSoldLastDay = 0;
        }
    }
}