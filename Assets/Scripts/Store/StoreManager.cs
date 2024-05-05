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
        [SerializeField] private Transform clientExit;

        [Header("Items Setup")] 
        [SerializeField] private InventoryObject storeInventory;
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

        #region Private Variables
        
        private List<Client> clients = new List<Client>();
        private WaitingLine _waitingLine;
        private Dictionary<int, Item> _items;
        private int _dailyClients = 2; // TODO update this, should variate depending on popularity
        private int _clientsSent = 0;
        private int _clientsLeft = 0;
        private readonly float _popularity = 0.5f;
        private readonly int _maxClients = 4;
        private readonly int _minClients = 3;
        private int _buyersInShop; // amount of clients currently in the shop
        private float _timeBetweenClients;
        private float _clientTimer = 3f;
        private float _cicleMaxTime;
        private float _cilceTimer;

        #endregion

        private void Start()
        {
            Client.ItemDatabase = itemDatabase;
            Client.exit = clientExit;
            
            Client.ItemBought += ItemBought;
            Client.MoneyAdded += SpawnText;
            Client.StartLine += AddToQueue;
            Client.LeaveLine += RemoveFromQueue;
            Client.LeftStore += CheckEndCicle;

            UpdateMoneyText();
            itemDisplayer.Initialize(itemDatabase, storeInventory);

            _waitingLine = new WaitingLine();
        }

        private void CheckEndCicle()
        {
            _clientsLeft++;
            if (_clientsLeft >= _dailyClients)
            {
                EndDayCycle();
            }
        }

        public void AddItem()
        {
            player.inventory.AddItem(itemDatabase.ItemObjects[0].data, 1);
        }
        
        public void StartDayCicle()
        {
            float clientsVariation = Random.Range(_popularity * 0.8f, _popularity * 1.2f);
            _dailyClients = (int)math.lerp(_minClients, _maxClients, clientsVariation);
            
            _waitingLine.Initialize(waitingLineStart, _dailyClients, distanceBetweenPos);
            
            // Updates list price of items depending on offer/sold last cycle
            foreach (var item in itemDatabase.ItemObjects)
            {
                item.data.listPrice.UpdatePrice();
            }

            StartCoroutine(SendClients());
        }
        
        /// <summary>
        /// Should be called when cicle ends
        /// </summary>
        private void EndDayCycle()
        {
            EndCycle?.Invoke();

            for (int i = clients.Count - 1; i >= 0; i--)
            {
                clients[i].Deinitialize();
                Destroy(clients[i].gameObject);
                clients.RemoveAt(i);
            }
            
            _clientsSent = 0;
            _clientsLeft = 0;
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

            itemDatabase.ItemObjects[id].data.listPrice.wasSold = true;
            itemDatabase.ItemObjects[id].data.listPrice.amountSoldLastDay++;
        }

        private bool AvailableItem()
        {
            return itemDisplayer.Items.Any(displayItem => displayItem is { BeingViewed: false, Bought: false });
        }

        /// <summary>
        /// Sends clients to the store
        /// </summary>
        /// <returns> Wait time between clients </returns>
        private IEnumerator SendClients()
        {
            // TODO improve this with object pooling
            for (int i = 0; i < _dailyClients; i++)
            {
                GameObject newClient = Instantiate(clientPrefab);
                newClient.transform.position = clientExit.position;
                
                clients.Add(newClient.GetComponent<Client>());
                
                clients[i].Initialize(i, ChooseItem());
                _clientsSent++;
                
                yield return new WaitForSeconds(_clientTimer);
            }
        }

        /// <summary>
        /// Chooses a random item for the client
        /// </summary>
        private DisplayItem ChooseItem()
        {
            if (!AvailableItem()) return null; // TODO handle this better

            DisplayItem[] avaliableItems =
                Array.FindAll(itemDisplayer.Items, displayItem => displayItem is { BeingViewed: false, Bought: false });
            
            int randomItem = Random.Range(0, avaliableItems.Length);
            avaliableItems[randomItem].BeingViewed = true;
            return avaliableItems[randomItem];
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
            }
        }
    }
}