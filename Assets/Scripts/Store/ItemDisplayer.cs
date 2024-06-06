using InventorySystem;
using TMPro;
using UnityEngine;

namespace Store
{
    public class ItemDisplayer : MonoBehaviour
    {
        [SerializeField] private Transform[] objPostition;
        [SerializeField] private TextMeshPro[] texts;

        public DisplayItem[] Items;

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
            ItemDisplay.OnItemUpdate += UpdateSlot;

            Items = new DisplayItem[storeInventory.GetSlots.Length];

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
            ItemDisplay.OnItemUpdate -= UpdateSlot;
        }

        private void RemoveItem(int id, int amount)
        {
            if (Items[id] == null) return;

            Items[id].amount -= amount;
            if (Items[id].amount <= 0) _storeInventory.GetSlots[id].RemoveItem();
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

            if (Items[slotId] != null) Destroy(Items[slotId].Object);

            if (_storeInventory.GetSlots[slotId].amount != 0) return true;

            Items[slotId] = null;
            return false;
        }

        private void CreateDisplayItem(int slotId)
        {
            Items[slotId] = new DisplayItem
            {
                Object = Instantiate(_database.ItemObjects[_storeInventory.GetSlots[slotId].item.id].characterDisplay,
                    objPostition[slotId]),
                ItemObject = _storeInventory.GetSlots[slotId].GetItemObject(),
                id = slotId,
                showPrice = texts[slotId],
                amount = _storeInventory.GetSlots[slotId].amount
            };

            Items[slotId].Initialize(Items[slotId].ItemObject.price);
        }

        private void UpdateSlot()
        {
            foreach (var item in Items)
            {
                item?.Initialize(item.ItemObject.price);
            }
        }

        private void UpdateInventory()
        {
            _storeInventory.UpdateInventory();

            for (int i = 0; i < Items.Length; i++)
            {
                switch (_storeInventory.GetSlots[i].amount)
                {
                    case 0:
                    {
                        if (Items[i] != null) Destroy(Items[i].Object);
                        Items[i] = null;
                        texts[i].text = string.Empty;
                        break;
                    }
                    case > 0 when Items[i] == null:
                        OnAddItem(i);
                        break;
                    case > 0:
                    {
                        if (Items[i].ItemObject != _storeInventory.GetSlots[i].GetItemObject())
                        {
                            RemoveItem(i, Items[i].amount);
                            OnAddItem(i);
                        }

                        break;
                    }
                }
            }
        }
    }
}