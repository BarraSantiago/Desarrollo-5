using System.Collections.Generic;
using UnityEngine;

namespace Store
{
    [CreateAssetMenu(fileName = "ListPrices")]
    public class ListPrices : ScriptableObject
    {
        [SerializeField] public List<ListPrice> prices;
    }
}