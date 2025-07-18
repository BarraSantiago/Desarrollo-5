﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Clients;
using InventorySystem;
using player;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

namespace Store
{
    public class StoreManager : MonoBehaviour
    {
        public static Action OnEndCycle;

        #region Serialized Fields

        [Header("Client Setup")] 
        [SerializeField] private Button chargeButton;
        [SerializeField] private GameObject[] clientPrefabs;
        [SerializeField] private ClientTransforms clientTransforms;
        [SerializeField] private Texture[] reactionTextures;

        [Header("Popularity Setup")] 
        [SerializeField] private PopularityManager popularityManager;

        [Header("Items Setup")]
        [SerializeField] private InventoryObject[] storeInventories;
        [SerializeField] private DisplayItem[] displayItems;
        [SerializeField] public ItemDatabaseObject itemDatabase;
        [SerializeField] private ItemDisplayer itemDisplayer;

        [Header("Waiting Line Setup")]
        [SerializeField] private Transform checkOut;
        [SerializeField] private Transform waitingLineStart;
        [SerializeField] private int posAmount;
        [SerializeField] private float distanceBetweenPos;

        [Header("Misc Setup")] 
        [SerializeField] private LightingManager lightingManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private Player player;

        [Header("Demo")] 
        [SerializeField] private Button startCycle;
        [SerializeField] private EndDayStats endDayStats;
        [SerializeField] private Button endCycle;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GameObject endDayInput;

        #endregion

        #region Private Variables

        public static Action<int, int> OnMoneyUpdated;
        private readonly List<Client> _clients = new List<Client>();
        private const string WaitingSoundKey = "ClientWaiting";
        private const string MurmurSoundKey = "Murmur";
        private Initializer _initializer;
        private WaitingLine _waitingLine;
        private Dictionary<int, Item> _items;
        private int _dailyClients = 2; // TODO update this, should variate depending on popularity
        public int _clientsLeft = 0;
        private int _buyersInShop;
        private float _timeBetweenClients;
        private readonly float _clientTimer = 7.87f;
        private const float CycleMaxTime = 210f;
        private int _experienceWon;
        private int _moneyWon;
        private int _moneyLost;
        private int _itemsSold;
        private int _satisfiedClients;
        private int _angryClients;
        private int _maxMoney;

        #endregion

        private void Start()
        {
            startCycle.onClick.AddListener(StartDayCycle);
            endCycle.onClick.AddListener(EndDayCycle);

            ItemDisplayer.DisplayItems = displayItems;
            Client.ItemDatabase = itemDatabase;
            Client.ClientTransforms = clientTransforms;
            Client.ReactionTextures = reactionTextures;
            Client.OnHappy += () => _experienceWon += 1;
            Client.OnHappy += () => _satisfiedClients += 1;
            Client.OnAngry += () => _angryClients += 1;
            Client.OnMoneyAdded += money => _moneyWon += money;
            player.OnMoneyReduced += money => _moneyLost += money;

            UpdateMoneyText();
            UpdateCurrentPrices();

            _waitingLine = new WaitingLine();
            chargeButton.onClick.AddListener(_waitingLine.ChargeClient);

            itemDisplayer.Initialize(storeInventories);
            UIManager.MainCanvas = mainCanvas;

            foreach (InventoryObject storeInventory in storeInventories)
            {
                storeInventory.Load();
            }

            Player.OnMoneyUpdate += UpdateMoneyText;

            _initializer = gameObject.AddComponent<Initializer>();
            _initializer.InitializeAll();
            _waitingLine.OnItemPaid += popularityManager.ItemPaid;
        }

        private void Update()
        {
            ChargeClient();
        }

        public void OnApplicationQuit()
        {
            _initializer.DeinitializeAll();
            SaveInventories();
        }


        private void StartDayCycle()
        {
            startCycle.interactable = false;
            _dailyClients = popularityManager.DailyClients;

            _experienceWon = 0;
            _moneyWon = 0;
            _itemsSold = 0;
            _satisfiedClients = 0;
            _angryClients = 0;
            _moneyLost = 0;

            _waitingLine.Initialize(waitingLineStart, _dailyClients, distanceBetweenPos, checkOut);

            Client.OnItemBought += ItemBought;
            Client.OnMoneyAdded += SpawnText;
            Client.OnStartLine += AddToQueue;
            Client.OnLeftStore += CheckEndCycle;
            //Client.OnLeftStore += PlayBackgroundNoise;

            foreach (ItemObject item in itemDatabase.ItemObjects)
            {
                item.data.listPrice.UpdatePrice();
            }

            lightingManager.StartDay();
            popularityManager.Initialize();
            StartCoroutine(SendClients());
        }

        /// <summary>
        /// Should be called when cycle ends
        /// </summary>
        private void EndDayCycle()
        {
            OnEndCycle?.Invoke();

            Client.OnItemBought -= ItemBought;
            Client.OnMoneyAdded -= SpawnText;
            Client.OnStartLine -= AddToQueue;
            Client.OnLeftStore -= CheckEndCycle;
            //Client.OnLeftStore -= PlayBackgroundNoise;

            endDayInput.SetActive(false);
            playerController.dayEnded = false;

            for (int i = _clients.Count - 1; i >= 0; i--)
            {
                _clients[i].Deinitialize();
                Destroy(_clients[i].gameObject);
                _clients.RemoveAt(i);
            }

            _clientsLeft = 0;
            startCycle.interactable = true; // TODO fix this
            lightingManager.Deinitialize();

            popularityManager.Deinitialize();
        }

        private void UpdateCurrentPrices()
        {
            foreach (ItemObject item in itemDatabase.ItemObjects)
            {
                item.data.listPrice.UpdatePrice();
            }
        }

        private void CheckEndCycle()
        {
            _clientsLeft++;

            if (_clientsLeft < _dailyClients) return;
            endDayStats.UpdateStats(_satisfiedClients, _angryClients, _experienceWon, _moneyWon, _itemsSold, _moneyLost);
            endDayInput.SetActive(true);
            playerController.dayEnded = true;
            
        }

        public void AddItem()
        {
            int rand = Random.Range(0, itemDatabase.ItemObjects.Length);
            player.inventory.AddItem(itemDatabase.ItemObjects[rand].data, 1);
        }

        public void AddGold()
        {
            const int goldAdded = 500;
            SpawnText(goldAdded);
        }

        private void SpawnText(int money)
        {
            _maxMoney = player.Money > _maxMoney ? player.Money : _maxMoney;
            player.Money += money;
            OnMoneyUpdated?.Invoke(player.Money, _maxMoney);
            uiManager.SpawnFlyingText(money);
            uiManager.UpdateMoneyText(player.Money);
        }

        private void UpdateMoneyText()
        {
            uiManager.UpdateMoneyText(player.Money);
        }

        private void UpdateMoneyText(int money)
        {
            uiManager.UpdateMoneyText(money);
        }

        private void ItemBought(int id, int amount)
        {
            itemDatabase.ItemObjects[id].data.listPrice.wasSold = true;
            itemDatabase.ItemObjects[id].data.listPrice.amountSoldLastDay++;
            _itemsSold += amount;
        }

        /// <summary>
        /// Sends clients to the store
        /// </summary>
        /// <returns> Wait time between clients </returns>
        private IEnumerator SendClients()
        {
            // TODO improve this with object pooling
            yield return new WaitForSeconds(_clientTimer);
            for (int i = 0; i < _dailyClients; i++)
            {
                GameObject newClient = Instantiate(clientPrefabs[Random.Range(0, clientPrefabs.Length)]);
                newClient.transform.position = clientTransforms.SpawnPoint.position;

                _clients.Add(newClient.GetComponent<Client>());

                _clients[i].Initialize(i);

                //PlayBackgroundNoise();
                float randomWaitTime = Random.Range(10f, 15f);

                yield return new WaitForSeconds(randomWaitTime);
            }
        }

        private void AddToQueue(Client agent)
        {
            if (!_waitingLine.AddToQueue(agent))
            {
                //No empty spaces in queue
            }

            if (_waitingLine.QueueCount == 1)
            {
                AudioManager.instance.Play(WaitingSoundKey);
            }
        }

        private void ChargeClient()
        {
            if (!_clients.Any(client => client.firstInLine))
            {
                chargeButton.gameObject.SetActive(false);
                return;
            }

            chargeButton.gameObject.SetActive(true);
        }

        private void PlayBackgroundNoise()
        {
            int clientsInStore = _clients.FindAll(client => client.InShop).Count;

            if (clientsInStore <= 3)
            {
                AudioManager.instance.Stop(MurmurSoundKey);
                return;
            }

            AudioManager.instance.GetAudioSource(MurmurSoundKey).volume = 0.3f * clientsInStore;
            if (!AudioManager.instance.GetAudioSource(MurmurSoundKey).isPlaying)
            {
                AudioManager.instance.Play(MurmurSoundKey);
            }
        }

        private void SaveInventories()
        {
            foreach (InventoryObject inventory in storeInventories)
            {
                inventory.Save();
            }
        }
    }
}