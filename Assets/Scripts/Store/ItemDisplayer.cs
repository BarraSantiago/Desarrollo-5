using UnityEngine;

namespace Store
{
    public class ItemDisplayer : MonoBehaviour
    {
        [SerializeField] private InventoryObject inventory;
        [SerializeField] private ItemDatabaseObject database;
        [SerializeField] private Transform[] objPostition;

        public DisplayItem[] Items;

        public void Initialize()
        {
            Items = new DisplayItem[inventory.GetSlots.Length];
        }
        
        public void Test()
        {
            for (int i = 0; i < inventory.GetSlots.Length; i++)
            {
                inventory.AddItem(database.ItemObjects[0].data, 1);
                Items[i] = gameObject.AddComponent<DisplayItem>();
                Items[i].Object = Instantiate(database.ItemObjects[inventory.GetSlots[i].item.id].characterDisplay,
                    objPostition[i]);
                Items[i].ItemObject = inventory.GetSlots[i].GetItemObject();
                Items[i].ItemObject.price = 50;
                Items[i].Initialize(Items[i].ItemObject.price);
                Items[i].id = i;
            }
        }

        public void RemoveItem(int id)
        {
            Items[id] = null;
            inventory.GetSlots[id].RemoveItem();
        }
    }
}