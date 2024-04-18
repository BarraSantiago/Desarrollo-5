using System;
using TMPro;
using UnityEngine;

namespace Store
{
    public class DisplayItem : MonoBehaviour
    {
        public bool BeingViewed { get; set; }
        public bool Bought { get; set; }
        [SerializeField] public int price;
        [SerializeField] public int ItemId;
        [SerializeField] public TMP_Text showPrice;

        private void Awake()
        {
            showPrice.text = "$" + price.ToString();
        }
    }
}