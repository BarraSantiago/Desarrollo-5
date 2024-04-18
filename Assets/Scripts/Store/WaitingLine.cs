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
        public WaitingPosition[] queuePositions;
        public Transform startingPosition;
        public int positionsAmount = 3;

        private float positionsDistance = 1f;

        private void Awake()
        {
            queuePositions = new WaitingPosition[positionsAmount];
            for (int i = 0; i < positionsAmount; i++)
            {
                queuePositions[i].position = startingPosition.position + Vector3.up * i;
            }
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

            return true;
        }

        public void AdvanceQueue()
        {
            for (int i = 0; i < queuePositions.Length; i++)
            {
                if (!queuePositions[i + 1].occupied)
                {
                    queuePositions[i].occupied = false;
                    queuePositions[i].client = null;
                    
                    break;
                } 

                queuePositions[i].client = queuePositions[i + 1].client;
                queuePositions[i].client.agent.SetDestination(queuePositions[i].position);
            }
        }
    }
}