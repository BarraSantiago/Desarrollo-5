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
        public ItemBuff[] buffs;
        public ListPrice listPrice;
        private int _price;
        public int Price{
            get => _price;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _price = value;
                OnPriceChange?.Invoke();
            }
        }
        public bool craftable;
        public ItemRecipe recipe;
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
            buffs = new ItemBuff[item.data.buffs.Length];
            for (int i = 0; i < buffs.Length; i++)
            {
                buffs[i] = new ItemBuff(item.data.buffs[i].Min, item.data.buffs[i].Max)
                {
                    stat = item.data.buffs[i].stat
                };
            }
        }
    
    }
}
