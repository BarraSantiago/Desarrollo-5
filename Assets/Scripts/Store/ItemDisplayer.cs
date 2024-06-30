using InventorySystem;
using TMPro;
using UnityEngine;

namespace Store
{
    public class ItemDisplayer : MonoBehaviour
    {
        [SerializeField] private Transform[] objPostition;
        [SerializeField] private TextMeshPro[] texts;

        public DisplayItem[] items;

        private ItemDatabaseObject _database;
        private InventoryObject _storeInventory;

        public void Initialize(ItemDatabaseObject database, InventoryObject storeInventory)
        {
            this._storeInventory = storeInventory;
            this._database = database;

            this._storeInventory.OnItemAdded += OnAddItem;
            InventoryObject.OnItemSwapInventory += OnAddItem;
            StoreManager.OnEndCycle += Deinitialize;
            Client.ItemGrabbed += RemoveItem;
            DisplayItem.OnItemUpdate += UpdateSlot;

            for (int i = 0; i < items.Length; i++)
            {
                items[i].itemPosition = objPostition[i];
            }

            UpdateInventory();
        }

        private void Deinitialize()
        {
            StoreManager.OnEndCycle -= Deinitialize;
        }

        private void OnDestroy()
        {
            InventoryObject.OnItemSwapInventory -= OnAddItem;
            _storeInventory.OnItemAdded -= OnAddItem;
            Client.ItemGrabbed -= RemoveItem;
            DisplayItem.OnItemUpdate -= UpdateSlot;
        }

        private void RemoveItem(int id, int amount)
        {
            if (!items[id]) return;

            items[id].amount = 0;
            if (items[id].amount <= 0) items[id].CleanDisplay();
            UpdateInventory();
        }

        private void OnAddItem(int slotId)
        {
            if (!ValidItem(slotId)) return;

            CreateDisplayItem(slotId);
            UpdateInventory();
        }

        private bool ValidItem(int slotId)
        {
            if (slotId >= _storeInventory.GetSlots.Length || slotId < 0) return false;

            if (_storeInventory.GetSlots[slotId].item.id < 0)
            {
                UpdateInventory();
                return false;
            }

            return _storeInventory.GetSlots[slotId].amount != 0;
        }

        private void CreateDisplayItem(int slotId)
        {
            items[slotId].id = slotId;
            items[slotId].showPrice = texts[slotId];
            items[slotId].itemPosition = objPostition[slotId];
            items[slotId].displayObject.GetComponent<GroundItem>().enabled = false;
            items[slotId].Initialize(_storeInventory.GetSlots[slotId].GetItemObject());
        }

        private void UpdateSlot()
        {
            foreach (var item in items)
            {
                item?.Initialize(item.item);
            }
        }

        private void UpdateInventory()
        {
            foreach (var item in items)
            {
                item.inventory.UpdateInventory();
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