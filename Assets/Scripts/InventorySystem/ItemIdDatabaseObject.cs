using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "New Item ID Database", menuName = "Inventory System/Items/IdDatabase")]
    public class ItemIdDatabaseObject : ScriptableObject
    {
        public ItemObject[] ItemObjects;
        
    }
}
