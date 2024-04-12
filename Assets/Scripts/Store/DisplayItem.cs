using UnityEngine;

namespace Store
{
    public class DisplayItem : MonoBehaviour
    {
        public bool BeingViewed { get; set; }
        public bool Bought { get; set; }
        [SerializeField] public int price;
        [SerializeField] public int ItemId;
    }
}