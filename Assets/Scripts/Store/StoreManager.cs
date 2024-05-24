using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Store
{
    public class StoreManager : MonoBehaviour
    {
        public static Action OnEndCycle;

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

        [Header("Demo")] 
        [SerializeField] private Button goToDungeon;
        [SerializeField] private Button startCicle;

        #endregion

        #region Private Variables

        [SerializeField]private List<Client> _clients = new List<Client>();
        private WaitingLine _waitingLine;
        private Dictionary<int, Item> _items;
        private int _dailyClients = 2; // TODO update this, should variate depending on popularity
        private int _clientsSent = 0;
        private int _clientsLeft = 0;
        private int _buyersInShop; // amount of clients currently in the shop
        private float _timeBetweenClients;
        private float _clientTimer = 3f;
        private float _cicleMaxTime = 30f;
        private float _cilceTimer = 0;
        private readonly float _popularity = 0.5f;
        private readonly int _maxClients = 4;
        private readonly int _minClients = 3;
        
        #endregion

        private void Start()
        {
            goToDungeon.onClick.AddListener(ChangeScene);
            goToDungeon.onClick.AddListener(EndDayCycle);
            startCicle.onClick.AddListener(StartDayCicle);
            
            Client.ItemDatabase = itemDatabase;
            Client.exit = clientExit;

            UpdateMoneyText();
            UpdateCurrentPrices();

            _waitingLine = new WaitingLine();
            
            itemDisplayer.Initialize(itemDatabase, storeInventory);
        }

        private void OnDestroy()
        {
            goToDungeon.onClick.RemoveListener(ChangeScene);
            goToDungeon.onClick.RemoveListener(EndDayCycle);
        }

        public void StartDayCicle()
        {
            startCicle.onClick.RemoveListener(StartDayCicle);

            float clientsVariation = Random.Range(_popularity * 0.8f, _popularity * 1.2f);
            _dailyClients = (int)math.lerp(_minClients, _maxClients, clientsVariation);

            _waitingLine.Initialize(waitingLineStart, _dailyClients, distanceBetweenPos);
            
            Client.OnItemBought += ItemBought;
            Client.OnMoneyAdded += SpawnText;
            Client.OnStartLine += AddToQueue;
            Client.OnLeaveLine += RemoveFromQueue;
            Client.OnLeftStore += CheckEndCicle;
            
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
            OnEndCycle?.Invoke();

            Client.OnItemBought -= ItemBought;
            Client.OnMoneyAdded -= SpawnText;
            Client.OnStartLine  -= AddToQueue;
            Client.OnLeaveLine  -= RemoveFromQueue;
            Client.OnLeftStore  -= CheckEndCicle;
            
            for (int i = _clients.Count - 1; i >= 0; i--)
            {
                _clients[i].Deinitialize();
                Destroy(_clients[i].gameObject);
                _clients.RemoveAt(i);
            }

            _clientsSent = 0;
            _clientsLeft = 0;
            startCicle.onClick.AddListener(StartDayCicle);
        }
        
        private void ChangeScene()
        {
            SceneManager.LoadScene("MovementScene");
        }

        private void UpdateCurrentPrices()
        {
            foreach (var item in itemDatabase.ItemObjects)
            {
                item.data.listPrice.UpdatePrice();
            }
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
            int rand = Random.Range(0, itemDatabase.ItemObjects.Length);
            player.inventory.AddItem(itemDatabase.ItemObjects[rand].data, 1);
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
                //GameObject newClient = Instantiate(clientPrefab);
                //newClient.transform.position = clientExit.position;

                //_clients.Add(newClient.GetComponent<Client>());

                _clients[i].Initialize(i, ChooseItem());
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