using TMPro;
using UnityEngine;

namespace Store
{
    public class DisplayItem : MonoBehaviour
    {
        public bool BeingViewed { get; set; }
        public bool Bought { get; set; }
        [SerializeField] private InventoryObject inventory;
        [SerializeField] private ItemDatabaseObject database;
        [SerializeField] private Transform objPostition;
        [SerializeField] public int price;
        [SerializeField] public TMP_Text showPrice;

        private void Awake()
        {
            showPrice.text = "$" + price.ToString();
        }

        public void Test()
        {
            inventory.AddItem(database.ItemObjects[0].data, 1);
            GameObject _gameObject = Instantiate(database.ItemObjects[inventory.GetSlots[0].item.Id].characterDisplay, objPostition);
            
            _gameObject.transform.position = Vector3.zero;

        }
    }
}