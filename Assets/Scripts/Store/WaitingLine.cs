using System;
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
        private int positionsAmount;
        private float positionsDistance;
        private Transform startingPosition;
        private WaitingPosition[] queuePositions;
        private Transform checkOut;
        
        public void Initialize(Transform startingPosition, int positionsAmount, float positionsDistance, Transform checkOut)
        {
            StoreManager.OnEndCycle += Deinitialize;
            this.startingPosition = startingPosition;
            this.positionsAmount = positionsAmount;
            this.positionsDistance = positionsDistance;
            this.checkOut = checkOut;
            
            CleanUpQueue();
        }

        private void Deinitialize()
        {
            foreach (var position in queuePositions)
            {
                position.occupied = false;
                position.client = null;
            }

            StoreManager.OnEndCycle -= Deinitialize;
        }

        public void ChargeClient()
        {
            queuePositions[0].client.PayItem();
            AdvanceQueue();
        }
        
        /// <summary>
        /// Adds an client to the "cash register" queue.
        /// </summary>
        /// <param name="clientAgent"> Client's agent to occupy position. </param>
        /// <returns> Returns true if it was able to queue the client. Returns false if there are no available positions. </returns>
        public bool AddToQueue(Client client)
        {
            if(queuePositions == null) CleanUpQueue();
            
            if (queuePositions.All(queuePosition => queuePosition.occupied)) return false;

            foreach (var position in queuePositions)
            {
                if (position.occupied) continue;
                
                position.client = client;
                position.client.agent.SetDestination(position.position);
                position.occupied = true;
                break;
            }

            queuePositions[0].client.firstInLine = true;
            return true;
        }

        public void RotateToTarget(Client client)
        {
            Vector3 direction = checkOut.position - client.gameObject.transform.position;
            Quaternion toRotation = Quaternion.LookRotation(direction);
            client.gameObject.transform.rotation = Quaternion.Slerp(client.gameObject.transform.rotation, toRotation, 1f); // 1f means instant rotation
        }
        public void AdvanceQueue()
        {
            if (queuePositions[0]?.client)
            {
                queuePositions[0].client.firstInLine = false;
            }

            queuePositions[0].client = null;

            for (int i = 0; i < queuePositions.Length - 1; i++)
            {
                if (!queuePositions[i + 1].occupied)
                {
                    queuePositions[i].occupied = false;
                    queuePositions[i].client = null;
                    break;
                }

                queuePositions[i].client = queuePositions[i + 1].client;
                if (queuePositions[i].client)
                {
                    queuePositions[i].client.agent.SetDestination(queuePositions[i].position);
                }
            }

            if (queuePositions[0].client)
            {
                queuePositions[0].client.firstInLine = true;
            }
        }
        
        private void CleanUpQueue()
        {
            queuePositions = new WaitingPosition[positionsAmount];
            
            for (int i = 0; i < positionsAmount; i++)
            {
                queuePositions[i] = new WaitingPosition
                {
                    position = startingPosition.position + Vector3.forward * (positionsDistance * i)
                };
            }
        }
    }
}