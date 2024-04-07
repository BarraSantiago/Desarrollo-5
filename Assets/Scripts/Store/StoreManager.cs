using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Store
{
    public class StoreManager : MonoBehaviour
    {
        [SerializeField] private Items items;
        [SerializeField] private Client[] clients;
        [SerializeField] public ListPrices listPrices;

        public static DisplayItem[] DisplayedItems;

        private Dictionary<int, Item> _items;
        private int _dailyClients = 1; // TODO update this, should variate depending on popularity
        private readonly float _popularity = 0.5f;
        private readonly int _maxClients = 15;
        private readonly int _minClients = 5;
        private int _buyersInShop; // amount of clients currently in the shop
        private float _timeBetweenClients;
        private float _clientTimer;
        private float _cicleMaxTime;
        private float _cilceTimer;

        private void Start()
        {
            // Updates list price of items depending on offer/sold last cycle
            foreach (var listPrice in listPrices.prices)
            {
                listPrice.UpdatePrice();
            }
            
            DisplayedItems = FindObjectsOfType<DisplayItem>(); // TODO Update this
            StartCicle();
        }

        public void StartCicle()
        {
            float clientsVariation = Random.Range(_popularity * 0.8f, _popularity * 1.2f);
            _dailyClients = (int)math.lerp(_minClients, _maxClients, clientsVariation);

            SendClient();
        }

        public static bool AvailableItem()
        {
            foreach (DisplayItem displayedItem in DisplayedItems)
            {
                if (!displayedItem.BeingViewed) return true;
            }

            return false;
        }

        private void SendClient()
        {
            foreach (Client client in clients)
            {
                if (client.inShop) continue;

                client.EnterStore();
            }
        }
    }
}