using System;
using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "New Item Recipe", menuName = "Inventory System/Items/New Recipe")]

    public class ItemRecipe : ScriptableObject
    {
        [Serializable]
        public class ItemEntry
        {
            public int itemID;
            public int amount;
        }
        public ItemEntry[] items;
    }
}