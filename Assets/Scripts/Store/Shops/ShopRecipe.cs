using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Store.Shops
{
    public class ShopRecipe : MonoBehaviour
    {
        [SerializeField] public Image materialImage;
        [SerializeField] public Image stateImage;
        [SerializeField] public TMP_Text required;
        [SerializeField] public TMP_Text owned;

        
        public void SetRecipe(ItemObject item, int requiredAmount, int ownedAmount)
        {
            materialImage.sprite = item.uiDisplay;
            required.text = requiredAmount.ToString();
            owned.text = ownedAmount.ToString();
            stateImage.color = requiredAmount <= ownedAmount ? Color.green : Color.red;
        }
    }
}