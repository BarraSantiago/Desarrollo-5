using System;
using Store;

namespace InventorySystem
{
    [System.Serializable]
    public class Item
    {
        public Action OnPriceChange;
        public string name;
        public int id = -1;
        public ListPrice listPrice;
        public bool craftable;
        public ItemRecipe recipe;
        private int _price;

        public int Price
        {
            get => _price;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _price = value;
                OnPriceChange?.Invoke();
            }
        }

        public Item()
        {
            name = "";
            id = -1;
            listPrice = new ListPrice(Price);
        }

        public Item(ItemObject item)
        {
            name = item.name;
            id = item.data.id;
        }
    }
}