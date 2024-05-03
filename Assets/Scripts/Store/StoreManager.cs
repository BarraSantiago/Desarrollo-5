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
        [SerializeField] private ItemDisplayer itemDisplayer;
        
        [Header("Waiting Line Setup")]
        [SerializeField] private Transform waitingLineStart;
        [SerializeField] private int posAmount;
        [SerializeField] private float distanceBetweenPos;
        
        [Header("Misc Setup")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Player player;

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
            itemDisplayer.Initialize();

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
            player.Money += money;
            uiManager.SpawnFlyingText(money);
            uiManager.UpdateMoneyText(player.Money);
        }

        private void UpdateMoneyText()
        {
            uiManager.UpdateMoneyText(player.Money);
        }

        private void ItemBought(DisplayItem displayItem)
        {
            int id = displayItem.ItemObject.data.id;
            
            displayItem.Bought = true;
            itemDisplayer.RemoveItem(displayItem.id);
            
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
            return itemDisplayer.Items.Any(displayItem => !displayItem.BeingViewed && !displayItem.Bought);
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

            DisplayItem[] avaliableItems =
                Array.FindAll(itemDisplayer.Items, displayItem => !displayItem.BeingViewed && !displayItem.Bought);

            int randomItem = Random.Range(0, avaliableItems.Length);

            client.desiredItem = avaliableItems[randomItem];

            avaliableItems[randomItem].BeingViewed = true;
        }
    }
}