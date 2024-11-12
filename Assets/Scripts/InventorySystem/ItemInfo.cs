using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem
{
    public class ItemInfo : MonoBehaviour
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text itemDescription;
        
        public void UpdateItemInfo(Sprite sprite, string name, string description)
        {
            itemImage.sprite = sprite;
            itemName.text = name;
            itemDescription.text = description;
            var color = itemImage.color;
            color.a = 255;
            itemImage.color = color;
            itemImage.preserveAspect = true;
        }
    }
}