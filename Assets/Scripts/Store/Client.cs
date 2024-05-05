using System;
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
            Idle,
            ChoosingItem,
            GrabbingItem,
            WaitingInline,
            Buying,
            Leaving,
            LeftStore,
            Happy,
            Angry
        }

        #region Public Variables

        public int id;
        public static Transform exit;
        public DisplayItem desiredItem;
        public static ItemDatabaseObject ItemDatabase;
        public static Action<Client> StartLine;
        public static Action LeaveLine;
        public static Action<DisplayItem> ItemBought;
        public static Action<int> MoneyAdded; //TODO change name
        public static Action<int> ItemGrabbed;
        public static Action LeftStore;

        /// <summary>
        /// Bool that indicates if the client is in the shop
        /// </summary>
        public bool inShop;
        public bool firstInLine;

        #endregion

        #region Serialized Variables
        
        [SerializeField] public NavMeshAgent agent;
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
        
        public void Initialize(int id, DisplayItem item)
        {
            this.id = id;
            desiredItem = item;
            EnterStore();
        }

        public void Deinitialize()
        {
            desiredItem.BeingViewed = false;
            desiredItem = null;
        }

        private void ClientBehaviour()
        {
            switch (_currentState)
            {
                case State.None:
                    break;
                case State.Idle:
                    // in this state, the client should wonder around until an item has been choosen or if there are no available items
                    break;
                case State.ChoosingItem:
                    // in this state, the client should go to the item and decide if they want to buy it
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
                    PayItem();
                    LeaveStore();
                    break;
                
                case State.LeftStore:
                    CheckLeftStore();
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
            _currentState = State.ChoosingItem;
        }

        private void GrabItem()
        {
            if (!CheckBuyItem()) return;
            agent.SetDestination(desiredItem.Object.transform.position);
            
            if (!NearItem()) return;
            desiredItem.Object.transform.SetParent(this.transform);
            ItemGrabbed?.Invoke(desiredItem.id);
            _currentState = State.WaitingInline;
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

            return cobrado;
        }

        private void Cobrado()
        {
            cobrado = true;
        }

        private void PayItem()
        {
            BuyItem();
            cobrar.gameObject.SetActive(false);
            LeaveLine?.Invoke();
        }

        private bool CheckBuyItem()
        {
            if(desiredItem == null) return false;
            
            ListPrice itemList = ItemDatabase.ItemObjects[desiredItem.ItemObject.data.id].data.listPrice;
            float difference = desiredItem.ItemObject.price - itemList.CurrentPrice;
            float percentageDifference = (difference / itemList.CurrentPrice) * 100f;

            if (desiredItem.ItemObject.price >= itemList.CurrentPrice)
            {
                // Client buys item and leaves the store
                if (!(percentageDifference > _willingnessToPay)) return true;
                
                // If the item is too expensive, the client leaves angry
                _currentState = State.Angry;
                return false;
            }

            LeaveTip(_happiness);
            return true;
        }

        /// <summary>
        /// Gives the player money and removes the item from being displayed
        /// </summary>
        private void BuyItem()
        {
            MoneyAdded?.Invoke(desiredItem.ItemObject.price);
            ItemBought?.Invoke(desiredItem);
        }

        /// <summary>
        /// Chance to leave a tip when buying an item
        /// </summary>
        /// <param name="chance"> Chance to leave a tip </param>
        private void LeaveTip(float chance)
        {
            // TODO make tip be a random consumable item
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
            _currentState = State.LeftStore;
        }

        private void CheckLeftStore()
        {
            if (NearExit())
            {
                LeftStore?.Invoke();
                gameObject.SetActive(false);
                _currentState = State.None;
            }
        }
        
        private bool NearItem()
        {
            return CheckDistance(desiredItem.Object.transform.position, minimumDistance);
        }

        private bool NearExit()
        {
            return CheckDistance(exit.position, minimumDistance);
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
    }
}