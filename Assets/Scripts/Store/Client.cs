using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Store
{
    public class Client : MonoBehaviour
    {
        private enum State
        {
            ChoosingItem,
            WaitingInline,
            Buying,
            Leaving,
            Happy,
            Angry
        }
        #region public variables

        public DisplayItem desiredItem = null;
        public static Action<Client> StartLine;
        public static Action LeaveLine;
        public static Action<int> ItemBought;
        public static Action<int> MoneyAdded; //TODO change name

        /// <summary>
        /// booleano para decir si el cliente ya esta en la tienda
        /// </summary>
        public bool inShop;

        public bool firstInLine;

        #endregion

        #region Serialized variables

        [SerializeField] private Player player; // TODO remove this
        [SerializeField] private StoreManager storeManager; // TODO remove this
        [SerializeField] public NavMeshAgent agent;
        [SerializeField] private Tilemap waitingLine;
        [SerializeField] private Transform exit;

        #endregion

        #region priavete variables

        /// <summary>
        /// rango de 0 a 100 de lo que esta dispuesto a pagar por sobre el precio de lista (ListPrice)
        /// </summary>
        private int _willingnessToPay = 20; // TODO update WOT value

        /// <summary>
        /// afecta al willingnessToPay y da una porbabilidad de que deje propina
        /// </summary>
        private int _happiness;

        /// <summary>
        /// Por si hacemos mejoras para modificar la probabilidad que dejen propina
        /// </summary>
        private int _tipChanceModifier = 0;

        private int _tipValue;
        private float minimumDistance = 0.9f;
        private bool waitingForPlayer;
        private State _currentState;

        #endregion

        private void Start()
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        private void Update()
        {
            switch (_currentState)
            {
                case State.ChoosingItem:
                    break;
                case State.WaitingInline:
                    break;
                case State.Buying:
                    break;
                case State.Leaving:
                    break;
                case State.Happy:
                    break;
                case State.Angry:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void EnterStore()
        {
            StartCoroutine(WalkToItem());
        }

        private IEnumerator WalkToItem()
        {
            //Move
            agent.SetDestination(desiredItem.transform.position);

            //TODO update this
            yield return new WaitUntil(DistanceToItem);

            //When client reaches item
            if (CheckBuyItem())
            {
                StartLine?.Invoke(this);
                
                waitingForPlayer = true;
                yield return new WaitUntil(PlayerInteraction);
                waitingForPlayer = false;
                
                GoToCashier();
            }

            //Leave store after buying item
            LeaveStore();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool PlayerInteraction()
        {
            if (!firstInLine) return false;

            return CheckDistance(player.transform.position, minimumDistance);
        }

        private void GoToCashier()
        {
            BuyItem();
            LeaveLine?.Invoke();
        }

        private bool DistanceToItem()
        {
            return CheckDistance(desiredItem.transform.position, minimumDistance);
        }

        private bool CheckDistance(Vector3 pos, float distanceDif)
        {
            float distance = Vector3.Distance(transform.position, pos);

            if (distance < distanceDif)
            {
                return true;
            }

            return false;
        }

        private bool CheckBuyItem()
        {
            ListPrice itemList = storeManager.listPrices.prices[desiredItem.ItemId];
            float difference = desiredItem.price - itemList.CurrentPrice;
            float percentageDifference = (difference / itemList.CurrentPrice) * 100f;

            if (desiredItem.price >= itemList.CurrentPrice)
            {
                // Cliente no compra el item y se va
                if (percentageDifference > _willingnessToPay)
                {
                    _happiness--; //TODO cambiar la cantidad de felicidad que pierde
                    LeaveStore();
                    return false;
                }

                //Esta aca para que no deje propina si el precio es mas caro que el precio de lista
                return true;
            }
            
            if (_happiness > 0)
                LeaveTip(Math.Abs(percentageDifference) + _happiness);
            return true;
        }

        /// <summary>
        /// Gives the player money and removes the item from being displayed
        /// </summary>
        private void BuyItem()
        {
            MoneyAdded?.Invoke(desiredItem.price);

            storeManager.listPrices.prices[desiredItem.ItemId].amountSoldLastDay++;
            ItemBought?.Invoke(desiredItem.ItemId);
        }

        /// <summary>
        /// Al comprar el item deja propina
        /// </summary>
        /// <param name="chance"></param>
        private void LeaveTip(float chance)
        {
            int randomNum = Random.Range(0, 100);

            if (chance < randomNum + _tipChanceModifier)
            {
                MoneyAdded.Invoke(_tipValue);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LeaveStore()
        {
            //move to outside of store
            agent.SetDestination(exit.transform.position);
        }

        private void OnDrawGizmos()
        {
            if (waitingForPlayer)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, minimumDistance);
            }
        }
    }
}