using InventorySystem;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "ItemList")]
    public class ItemList : ScriptableObject
    {
        public Item[] items;
    }
}