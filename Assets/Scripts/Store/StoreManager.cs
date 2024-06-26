﻿using System;
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
        [SerializeField] private Button chargeButton;
        [SerializeField] private GameObject chargeSign;
        [SerializeField] private float maxDistance = 7;
        [SerializeField] private GameObject clientPrefab;
        [SerializeField] private ClientTransforms clientTransforms;

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
        [SerializeField] private Transform chargePosition;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private player.Player player;

        [Header("Demo")]
        [SerializeField] private Button goToDungeon;
        [SerializeField] private Button startCicle;

        #endregion

        #region Private Variables

        private readonly List<Client> _clients = new List<Client>();
        private const string WaitingSoundKey = "ClientWaiting";
        private const string MurmurSoundKey = "Murmur";
        private WaitingLine _waitingLine;
        private Dictionary<int, Item> _items;
        private int _dailyClients = 2; // TODO update this, should variate depending on popularity
        private int _clientsLeft = 0;
        private int _buyersInShop; // amount of clients currently in the shop
        private float _timeBetweenClients;
        private readonly float _clientTimer = 2.87f;
        private const float CicleMaxTime = 30f;
        private float _cilceTimer = 0;
        private readonly float _popularity = 0.5f;
        private readonly int _maxClients = 7;
        private readonly int _minClients = 3;

        #endregion

        private void Start()
        {
            goToDungeon.onClick.AddListener(ChangeScene);
            goToDungeon.onClick.AddListener(EndDayCycle);
            startCicle.onClick.AddListener(StartDayCicle);

            ItemDisplayer.DisplayItems = displayItems;
            Client.ItemDatabase = itemDatabase;
            Client.ClientTransforms = clientTransforms;

            UpdateMoneyText();
            UpdateCurrentPrices();

            _waitingLine = new WaitingLine();
            chargeButton.onClick.AddListener(_waitingLine.ChargeClient);

            itemDisplayer.Initialize(storeInventories);
            UIManager.MainCanvas = mainCanvas;
            foreach (var storeInventory in storeInventories)
            {
                storeInventory.Load();
            }
        }

        private void Update()
        {
            ChargeClient();
        }

        private void OnDestroy()
        {
            goToDungeon.onClick.RemoveListener(ChangeScene);
            goToDungeon.onClick.RemoveListener(EndDayCycle);
            SaveInventories();
        }

        public void OnApplicationQuit()
        {
            SaveInventories();
        }


        private void StartDayCicle()
        {
            startCicle.interactable = false;

            float clientsVariation = Random.Range(_popularity * 0.8f, _popularity * 1.2f);
            _dailyClients = (int)math.lerp(_minClients, _maxClients, clientsVariation);

            _waitingLine.Initialize(waitingLineStart, _dailyClients, distanceBetweenPos, checkOut);

            Client.OnItemBought += ItemBought;
            Client.OnMoneyAdded += SpawnText;
            Client.OnStartLine += AddToQueue;
            Client.OnLeftStore += CheckEndCicle;
            Client.OnLeftStore += PlayBackgrounNoise;

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
            Client.OnStartLine -= AddToQueue;
            Client.OnLeftStore -= CheckEndCicle;
            Client.OnLeftStore -= PlayBackgrounNoise;

            for (int i = _clients.Count - 1; i >= 0; i--)
            {
                _clients[i].Deinitialize();
                Destroy(_clients[i].gameObject);
                _clients.RemoveAt(i);
            }

            _clientsLeft = 0;
            //startCicle.interactable = true; // TODO fix this
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

        public void AddGold()
        {
            const int goldAdded = 500;
            player.money += goldAdded;
            SpawnText(goldAdded);
        }

        private void SpawnText(int money)
        {
            player.money += money;
            uiManager.SpawnFlyingText(money);
            uiManager.UpdateMoneyText(player.money);
        }

        private void UpdateMoneyText()
        {
            uiManager.UpdateMoneyText(player.money);
        }

        private void ItemBought(int id)
        {
            itemDatabase.ItemObjects[id].data.listPrice.wasSold = true;
            itemDatabase.ItemObjects[id].data.listPrice.amountSoldLastDay++;
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
                newClient.transform.position = clientTransforms.SpawnPoint.position;

                _clients.Add(newClient.GetComponent<Client>());

                _clients[i].Initialize(i);
                
                PlayBackgrounNoise();
                yield return new WaitForSeconds(_clientTimer);
            }
        }

        private void AddToQueue(Client agent)
        {
            if (!_waitingLine.AddToQueue(agent))
            {
                //No empty spaces in queue
            }

            if (_waitingLine.QueueCount == 1 &&
                Vector3.Distance(player.transform.position, waitingLineStart.position) > maxDistance)
            {
                // Play the sound
                AudioManager.instance.Play(WaitingSoundKey);
            }
        }

        private void ChargeClient()
        {
            if (!_clients.Any(client => client.firstInLine))
            {
                chargeSign.SetActive(false);
                chargeButton.gameObject.SetActive(false);
                return;
            }

            float distance = Vector3.Distance(player.transform.position, chargePosition.position);

            if (distance > maxDistance)
            {
                chargeSign.SetActive(true);
                chargeButton.gameObject.SetActive(false);
                return;
            }

            chargeSign.SetActive(false);
            chargeButton.gameObject.SetActive(true);
        }

        private void PlayBackgrounNoise()
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
            foreach (var inventory in storeInventories)
            {
                inventory.Save();
            }
        }
    }
}