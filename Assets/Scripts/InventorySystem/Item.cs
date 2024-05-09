using Store;

namespace InventorySystem
{
    [System.Serializable]
    public class Item
    {
        public string name;
        public int id = -1;
        public ItemBuff[] buffs;
        public ListPrice listPrice;
        public int price;
        public Item()
        {
            name = "";
            id = -1;
            listPrice = new ListPrice(price);
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
