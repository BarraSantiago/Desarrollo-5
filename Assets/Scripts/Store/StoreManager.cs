using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Store
{
    public class StoreManager : MonoBehaviour
    {
        public static Action EndCycle;
        #region Serialized Fields
        
        [Header("Client Setup")]
        [SerializeField] private GameObject clientPrefab;
        [SerializeField] private Client[] clients;
        
        [Header("Items Setup")]
        [SerializeField] private InventoryObject inventory;
        [SerializeField] public ItemDatabaseObject itemDatabase;
        
        [Header("Waiting Line Setup")]
        [SerializeField] private Transform waitingLineStart;
        [SerializeField] private int posAmount;
        [SerializeField] private float distanceBetweenPos;
        
        [Header("Misc Setup")]
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private Player _player;

        #endregion

        #region private variables
        
        private WaitingLine _waitingLine;
        private Dictionary<int, Item> _items;
        private int _dailyClients = 2; // TODO update this, should variate depending on popularity
        private int _clientsSent = 0;
        private readonly float _popularity = 0.5f;
        private readonly int _maxClients = 15;
        private readonly int _minClients = 5;
        private int _buyersInShop; // amount of clients currently in the shop
        private float _timeBetweenClients;
        private float _clientTimer = 3f;
        private float _cicleMaxTime;
        private float _cilceTimer;

        #endregion
        
        private void Start()
        {
            Client.ItemBought += ItemBought;
            Client.MoneyAdded += SpawnText;
            Client.StartLine += AddToQueue;
            Client.LeaveLine += RemoveFromQueue;

            UpdateMoneyText();

            _waitingLine = new WaitingLine(waitingLineStart,posAmount, distanceBetweenPos);
        }

        private void RemoveFromQueue()
        {
            _waitingLine.AdvanceQueue();
        }

        private void AddToQueue(Client agent)
        {
            if (!_waitingLine.AddToQueue(agent))
            {
                //No empty spaces in queue
            };
        }

        private void SpawnText(int money)
        {
            _player.Money += money;
            _uiManager.SpawnFlyingText(money);
            _uiManager.UpdateMoneyText(_player.Money);
        }

        private void UpdateMoneyText()
        {
            _uiManager.UpdateMoneyText(_player.Money);
        }

        private void ItemBought(int id)
        {
            //inventory.GetSlots[id].GetItemObject().gameObject.SetActive(false);
            inventory.GetSlots[id].GetItemObject().displayItem.Bought = true;
            inventory.GetSlots[id].RemoveItem();
            itemDatabase.ItemObjects[id].data.listPrice.wasSold = true;
            itemDatabase.ItemObjects[id].data.listPrice.amountSoldLastDay++;
        }

        public void StartCicle()
        {
            _waitingLine.Initialize();
            
            float clientsVariation = Random.Range(_popularity * 0.8f, _popularity * 1.2f);
            _dailyClients = (int)math.lerp(_minClients, _maxClients, clientsVariation);

            // Updates list price of items depending on offer/sold last cycle
            foreach (var item in itemDatabase.ItemObjects)
            {
                item.data.listPrice.UpdatePrice();
            }

            StartCoroutine(SendClient());
        }

        /// <summary>
        /// Should be called when cicle ends
        /// </summary>
        private void CompleteDayCycle()
        {
            EndCycle?.Invoke();
        }
        
        private bool AvailableItem()
        {
            return inventory.GetSlots.Any(slot => !slot.GetItemObject().displayItem.BeingViewed && !slot.GetItemObject().displayItem.Bought);
        }

        private IEnumerator SendClient()
        {
            foreach (Client client in clients)
            {
                if (_clientsSent >= _dailyClients) continue;
                if (client.inShop) continue;

                ChooseItem(client);
                client.EnterStore();

                _clientsSent++;
                yield return new WaitForSeconds(_clientTimer);
            }
        }
        
        /// <summary>
        /// Chooses a random item for the client
        /// </summary>
        /// <param name="client"> Subject to recieve a random item </param>
        private void ChooseItem(Client client)
        {
            if (!AvailableItem()) return;

            InventorySlot[] avaliableItems =
                Array.FindAll(inventory.GetSlots, slot => !slot.GetItemObject().displayItem.BeingViewed && !slot.GetItemObject().displayItem.Bought);

            int randomItem = Random.Range(0, avaliableItems.Length);

            client.desiredItem = avaliableItems[randomItem].GetItemObject();

            avaliableItems[randomItem].GetItemObject().displayItem.BeingViewed = true;
        }
    }
}