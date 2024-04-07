using System.Collections.Generic;
using UnityEngine;

namespace Store
{
    [CreateAssetMenu(fileName = "Items")]
    public class Items : ScriptableObject
    {
        [SerializeField] public List<Item> items = null;
    }
}