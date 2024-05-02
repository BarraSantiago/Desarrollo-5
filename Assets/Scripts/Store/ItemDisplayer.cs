using UnityEngine;

namespace Store
{
    public class ItemDisplayer : MonoBehaviour
    {
        [SerializeField] private InventoryObject inventory;
        [SerializeField] private ItemDatabaseObject database;
        [SerializeField] private Transform[] objPostition;

        public void Test()
        {
            for (int i = 0; i < inventory.GetSlots.Length; i++)
            {
                inventory.AddItem(database.ItemObjects[0].data, 1);
                GameObject _gameObject = Instantiate(database.ItemObjects[inventory.GetSlots[i].item.id].characterDisplay,
                    objPostition[i]);
                inventory.GetSlots[i].GetItemObject().price = 50;
                inventory.GetSlots[i].GetItemObject().currentInstance = _gameObject;
                inventory.GetSlots[i].GetItemObject().PlaceItem();
                _gameObject.transform.position = Vector3.zero;
                
            }
        }
    }
}