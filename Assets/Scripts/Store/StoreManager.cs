using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Store
{
    public class StoreManager : MonoBehaviour
    {
        [SerializeField] private GameObject clientPrefab;
        [SerializeField] private Client[] clients;
        [SerializeField] public ListPrices listPrices;
        [SerializeField] public DisplayItem[] displayedItems;
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private Player _player;

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

        private void Start()
        {
            Client.ItemBought += ItemBought;
            Client.MoneyAdded += SpawnText;
            Client.StartQueue += AddToQueue;

            UpdateMoneyText();

            _waitingLine = new WaitingLine();
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
            displayedItems[id].gameObject.SetActive(false);
            displayedItems[id].Bought = true;
        }

        public void StartCicle()
        {
            float clientsVariation = Random.Range(_popularity * 0.8f, _popularity * 1.2f);
            _dailyClients = (int)math.lerp(_minClients, _maxClients, clientsVariation);

            // Updates list price of items depending on offer/sold last cycle
            foreach (var listPrice in listPrices.prices)
            {
                listPrice.UpdatePrice();
            }

            StartCoroutine(SendClient());
        }

        private bool AvailableItem()
        {
            return displayedItems.Any(displayedItem => !displayedItem.BeingViewed && !displayedItem.Bought);
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
        
        private void ChooseItem(Client client)
        {
            if (!AvailableItem()) return;

            DisplayItem[] abaliavleItems =
                System.Array.FindAll(displayedItems, item => !item.BeingViewed && !item.Bought);

            int randomItem = Random.Range(0, abaliavleItems.Length);

            client.desiredItem = displayedItems[randomItem];

            displayedItems[randomItem].BeingViewed = true;
        }
    }
}