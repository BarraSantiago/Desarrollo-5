using TMPro;
using UnityEngine;

namespace Store
{
    public class DisplayItem : MonoBehaviour
    {
        public bool BeingViewed { get; set; }
        public bool Bought { get; set; }
        public TMP_Text showPrice;
        public GameObject Object;
        public ItemObject ItemObject;
        public int id;
        
        public void Initialize(int price)
        {
            GameObject _gameObject = Instantiate(new GameObject(), gameObject.transform);
            showPrice = _gameObject.AddComponent<TextMeshPro>();
            showPrice.text = "$" + price.ToString();
        }
    }
}