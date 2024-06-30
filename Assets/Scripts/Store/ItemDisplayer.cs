using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Store
{
    public class ItemDisplayer : MonoBehaviour
    {
        [SerializeField] private Transform[] objPostition;
        [SerializeField] private TextMeshPro[] texts;

        [FormerlySerializedAs("items")] public DisplayItem[] displayItems;

        private ItemDatabaseObject _database;
        private InventoryObject[] _storeInventories;

        public void Initialize(ItemDatabaseObject database, InventoryObject[] storeInventories)
        {
            this._storeInventories = storeInventories;
            this._database = database;

            StoreManager.OnEndCycle += Deinitialize;
            Client.ItemGrabbed += RemoveItem;

            for (int i = 0; i < displayItems.Length; i++)
            {
                displayItems[i].inventory = storeInventories[i];
                displayItems[i].itemPosition = objPostition[i];
                displayItems[i].showPrice = texts[i];
            }

            UpdateInventory();
        }

        private void Deinitialize()
        {
            StoreManager.OnEndCycle -= Deinitialize;
        }

        private void OnDestroy()
        {
            Client.ItemGrabbed -= RemoveItem;
        }

        private void RemoveItem(int id, int amount)
        {
            if (!displayItems[id]) return;

            displayItems[id].amount = 0;
            if (displayItems[id].amount <= 0) displayItems[id].CleanDisplay();
            UpdateInventory();
        }

        private void CreateDisplayItem(int slotId)
        {
            displayItems[slotId].id = slotId;
            displayItems[slotId].showPrice = texts[slotId];
            displayItems[slotId].itemPosition = objPostition[slotId];
            displayItems[slotId].displayObject.GetComponent<GroundItem>().enabled = false;
        }

        private void UpdateSlot()
        {
            foreach (var item in displayItems)
            {
                item?.Initialize(item.item);
            }
        }

        private void UpdateInventory()
        {
            foreach (var displayItem in displayItems)
            {
                displayItem.Initialize();
            }
            /*
            foreach (var item in items)
            {
                for (int i = 0; i < item.inventory.GetSlots.Length; i++)
                {
                    switch (_storeInventory.GetSlots[i].amount)
                    {
                        case 0:
                        {
                            if (items[i]) Destroy(items[i].displayObject);
                            texts[i].text = string.Empty;
                            break;
                        }
                        case > 0 when !items[i]:
                            OnAddItem(i);
                            break;
                        case > 0:
                        {
                            if (items[i].item != _storeInventory.GetSlots[i].GetItemObject())
                            {
                                RemoveItem(i, items[i].amount);
                                OnAddItem(i);
                            }

                            break;
                        }
                    }
                }
            }*/
        }
    }
}