using System.Linq;
using UnityEngine;

namespace Store
{
    public class WaitingPosition
    {
        public Vector3 position;
        public bool occupied;
        public Client client;
    }

    public class WaitingLine
    {
        public int QueueCount
        {
            get { return _queuePositions.Count(x => x.occupied); }
        }
        private int _positionsAmount;
        private float _positionsDistance;
        private Transform _startingPosition;
        private WaitingPosition[] _queuePositions;
        private Transform _checkOut;

        public void Initialize(Transform startingPosition, int positionsAmount, float positionsDistance,
            Transform checkOut)
        {
            StoreManager.OnEndCycle += Deinitialize;
            _startingPosition = startingPosition;
            _positionsAmount = positionsAmount;
            _positionsDistance = positionsDistance;
            _checkOut = checkOut;

            CleanUpQueue();
        }

        private void Deinitialize()
        {
            foreach (var position in _queuePositions)
            {
                position.occupied = false;
                position.client = null;
            }

            StoreManager.OnEndCycle -= Deinitialize;
        }

        public void ChargeClient()
        {
            _queuePositions[0].client.PayItem();
            AdvanceQueue();
        }

        /// <summary>
        /// Adds an client to the "cash register" queue.
        /// </summary>
        /// <param name="client"> New client to add to the queue </param>
        /// <returns> Returns true if it was able to queue the client. Returns false if there are no available positions. </returns>
        public bool AddToQueue(Client client)
        {
            if (_queuePositions == null) CleanUpQueue();

            if (_queuePositions.All(queuePosition => queuePosition.occupied)) return false;

            foreach (var position in _queuePositions)
            {
                if (position.occupied) continue;

                position.client = client;
                position.client.agent.SetDestination(position.position);
                position.occupied = true;
                break;
            }

            _queuePositions[0].client.firstInLine = true;
            return true;
        }

        public void AdvanceQueue()
        {
            for (int i = 0; i < _queuePositions.Length; i++)
            {
                if (_queuePositions[i].client)
                {
                    _queuePositions[i].client.firstInLine = false;
                }

                _queuePositions[i].client = null;
                _queuePositions[i].occupied = false; // Ensure the position is marked as unoccupied

                if (i + 1 >= _queuePositions.Length || !_queuePositions[i + 1].occupied) continue;
                
                _queuePositions[i].client = _queuePositions[i + 1].client;
                _queuePositions[i].occupied = true;
                _queuePositions[i + 1].client = null;
                _queuePositions[i + 1].occupied = false;
                
                if (_queuePositions[i].client)
                {
                    _queuePositions[i].client.agent.SetDestination(_queuePositions[i].position);
                }
            }

            if (_queuePositions[0].client)
            {
                _queuePositions[0].client.firstInLine = true;
            }
        }
        private void CleanUpQueue()
        {
            _queuePositions = new WaitingPosition[_positionsAmount];

            for (int i = 0; i < _positionsAmount; i++)
            {
                _queuePositions[i] = new WaitingPosition
                {
                    position = _startingPosition.position + Vector3.back * (_positionsDistance * i)
                };
            }
        }
    }
}