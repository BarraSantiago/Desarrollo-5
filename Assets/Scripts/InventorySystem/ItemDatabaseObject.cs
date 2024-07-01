using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Items/Database")]
    public class ItemDatabaseObject : ScriptableObject
    {
        public ItemObject[] ItemObjects;

        public void OnValidate()
        {
            for (int i = 0; i < ItemObjects.Length; i++)
            {
                ItemObjects[i].data.id = i;
            }
        }

        public void AddItem(ItemObject itemObject)
        {
            // Resize the array
            System.Array.Resize(ref ItemObjects, ItemObjects.Length + 1);

            // Add the new item to the end of the array
            ItemObjects[^1] = itemObject;
            // Check if data is null and if so, initialize it
            itemObject.data ??= new Item();
            // Update the ID of the new item
            itemObject.data.id = ItemObjects.Length - 1;
        }
    }
}
