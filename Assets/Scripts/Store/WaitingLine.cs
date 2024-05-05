using System.Linq;
using UnityEngine;

namespace Store
{
    public struct WaitingPosition
    {
        public Vector3 position;
        public bool occupied;
        public Client client;
    }

    public class WaitingLine
    {
        private WaitingPosition[] queuePositions;
        
        public void Initialize(Transform startingPosition, int positionsAmount, float positionsDistance)
        {
            StoreManager.EndCycle += Deinitialize;

            queuePositions = new WaitingPosition[positionsAmount];

            for (int i = 0; i < positionsAmount; i++)
            {
                queuePositions[i].position = startingPosition.position + (Vector3.up * positionsDistance) * i;
            }
        }

        private void Deinitialize()
        {
            for (int i = 0; i < queuePositions.Length; i++)
            {
                queuePositions[i].occupied = false;
                queuePositions[i].client = null;
            }

            StoreManager.EndCycle -= Deinitialize;
        }

        /// <summary>
        /// Adds an client to the "cash register" queue.
        /// </summary>
        /// <param name="clientAgent"> Client's agent to occupy position. </param>
        /// <returns> Returns true if it was able to queue the client. Returns false if there are no available positions. </returns>
        public bool AddToQueue(Client client)
        {
            if (queuePositions.All(queuePosition => queuePosition.occupied)) return false;

            for (int i = 0; i < queuePositions.Length; i++)
            {
                if (queuePositions[i].occupied) continue;
                queuePositions[i].client = client;
                queuePositions[i].client.agent.SetDestination(queuePositions[i].position);
                queuePositions[i].occupied = true;
                break;
            }

            queuePositions[0].client.firstInLine = true;
            return true;
        }

        public void AdvanceQueue()
        {
            if (queuePositions[0].client != null)
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
                if (queuePositions[i].client != null)
                {
                    queuePositions[i].client.agent.SetDestination(queuePositions[i].position);
                }
            }

            if (queuePositions[0].client != null)
            {
                queuePositions[0].client.firstInLine = true;
            }
        }
    }
}