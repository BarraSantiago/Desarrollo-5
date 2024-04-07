using UnityEngine;

namespace Store
{
    public class DisplayItem : MonoBehaviour
    {
        public bool BeingViewed { get; set; }
        [SerializeField] public int price;
        [SerializeField] public int ItemId;
    }
}