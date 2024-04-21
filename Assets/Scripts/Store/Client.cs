using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Store
{
    public class Client : MonoBehaviour
    {
        private enum State
        {
            None,
            ChoosingItem,
            GrabbingItem,
            WaitingInline,
            Buying,
            Leaving,
            Happy,
            Angry
        }

        #region Public Variables

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

        #region Serialized Variables

        [SerializeField] private Player player; // TODO remove this
        [SerializeField] private StoreManager storeManager; // TODO remove this
        [SerializeField] public NavMeshAgent agent;
        [SerializeField] private Transform exit;
        [SerializeField] private Button cobrar; // TODO update this

        #endregion

        #region Priavete Variables

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

        private bool cobrado = false;
        private int _tipValue;
        private float minimumDistance = 0.9f;
        private bool waitingForPlayer;
        private State _currentState;
        private State _previousState;

        #endregion

        private void Start()
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            cobrar.onClick.AddListener(Cobrado);
        }

        private void Update()
        {
            if(_currentState == State.None) return;
            ClientBehaviour();
        }

        private void ClientBehaviour()
        {
            switch (_currentState)
            {
                case State.ChoosingItem:
                    // in this state, the client should wonder around if there are no available items
                    _currentState = State.GrabbingItem;
                    break;

                case State.GrabbingItem:
                    GrabItem();
                    
                    break;

                case State.WaitingInline:
                    StartLine?.Invoke(this);
                    _currentState = State.Buying;
                    break;

                case State.Buying:
                    waitingForPlayer = true;
                    if (PlayerInteraction())
                    {
                        waitingForPlayer = false;
                        _currentState = State.Leaving;
                    }

                    break;

                case State.Leaving:
                    GoToCashier();
                    LeaveStore();
                    break;

                case State.Happy:
                    _happiness++;
                    LeaveStore();
                    break;

                case State.Angry:
                    _happiness--;
                    LeaveStore();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void EnterStore()
        {
            //StartCoroutine(WalkToItem());
            _currentState = State.ChoosingItem;
        }

        private void GrabItem()
        {
            if (!CheckBuyItem()) return;
            agent.SetDestination(desiredItem.transform.position);
            if (!DistanceToItem()) return;
            desiredItem.gameObject.transform.SetParent(this.transform);
            _currentState = State.WaitingInline;
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
                desiredItem.gameObject.transform.SetParent(this.transform);

                StartLine?.Invoke(this);

                waitingForPlayer = true;
                yield return new WaitUntil(PlayerInteraction);
                waitingForPlayer = false;
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
            cobrar.transform.position = Camera.main.WorldToScreenPoint(transform.position);
            cobrar.gameObject.SetActive(true); // TODO REMOVE THIS

            return CheckDistance(player.transform.position, minimumDistance) || cobrado;
        }

        private void Cobrado()
        {
            cobrado = true;
        }

        private void GoToCashier()
        {
            BuyItem();
            cobrar.gameObject.SetActive(false);
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
                    _currentState = State.Angry;
                    return false;
                }

                //Esta aca para que no deje propina si el precio es mas caro que el precio de lista
                return true;
            }

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
        /// Chance to leave a tip when buying an item
        /// </summary>
        /// <param name="chance"> Chance to leave a tip </param>
        private void LeaveTip(float chance)
        {
            int randomNum = Random.Range(0, 100);

            if (chance < randomNum + _tipChanceModifier)
            {
                MoneyAdded.Invoke(_tipValue);
            }
        }

        /// <summary>
        /// Exits the store
        /// </summary>
        private void LeaveStore()
        {
            cobrado = false;
            // TODO update this
            //move to outside of store
            agent.SetDestination(exit.transform.position);
            _currentState = State.None;
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