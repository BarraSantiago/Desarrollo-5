using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Store
{
    public class StoreManager : MonoBehaviour
    {
        [SerializeField] private Items items;
        [SerializeField] private GameObject clientPrefab;
        [SerializeField] private Client[] clients;
        [SerializeField] public ListPrices listPrices;
        [SerializeField] public DisplayItem[] displayedItems;

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

            // Updates list price of items depending on offer/sold last cycle
            foreach (var listPrice in listPrices.prices)
            {
                listPrice.UpdatePrice();
            }
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

            StartCoroutine( SendClient());
        }

        public bool AvailableItem()
        {
            foreach (DisplayItem displayedItem in displayedItems)
            {
                if (!displayedItem.BeingViewed && !displayedItem.Bought) return true;
            }

            return false;
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

            DisplayItem[] abaliavleItems = System.Array.FindAll(displayedItems, item => !item.BeingViewed && !item.Bought);

            int randomItem = Random.Range(0, abaliavleItems.Length);

            client.desiredItem = displayedItems[randomItem];

            displayedItems[randomItem].BeingViewed = true;
        }
    }
}