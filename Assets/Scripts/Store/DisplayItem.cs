using TMPro;
using UnityEngine;

namespace Store
{
    public class DisplayItem : MonoBehaviour
    {
        public bool BeingViewed { get; set; }
        public bool Bought { get; set; }
        public TMP_Text showPrice;
        public DisplayItem(int price)
        {
            GameObject _gameObject = Instantiate(new GameObject(), gameObject.transform);
            _gameObject.transform.SetParent(gameObject.transform);
            showPrice = _gameObject.AddComponent<TextMeshPro>();
            showPrice.text = "$" + price.ToString();
        }
    }
}