using System;
using InventorySystem;
using UnityEngine;
using UnityEngine.AI;
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

        #region Serialized Variables

        [SerializeField] public NavMeshAgent agent;
        [SerializeField] private float _minimumDistance = 1.6f;

        #endregion

        #region Public Variables

        public int id;
        public static Transform Exit;
        public static Transform WaitingLineStart;
        public static ItemDatabaseObject ItemDatabase;
        public static Action<Client> OnStartLine;
        public static Action OnLeaveLine;
        public static Action<DisplayItem> OnItemBought;
        public static Action<int> OnMoneyAdded; //TODO change name
        public static Action<int, int> ItemGrabbed;
        public static Action OnLeftStore;

        /// <summary>
        /// Bool that indicates if the client is in the shop
        /// </summary>
        public bool inShop;

        public bool firstInLine;

        #endregion

        #region Priavete Variables

        /// <summary>
        /// Percentage that reprecents how much the client is willing to pay over the list price
        /// </summary>
        private int _willingnessToPay = 20; // TODO update WOT value

        /// <summary>
        /// Chance to leave a tip
        /// </summary>
        private int _happiness;

        /// <summary>
        /// If we want to modify the chance to leave a tip
        /// </summary>
        private int _tipChanceModifier = 0;

        private bool _paid;
        private int _tipValue;
        private State _currentState;
        private DisplayItem _desiredItem;
        private GameObject _itemInstance;

        #endregion

        private void Start()
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        private void Update()
        {
            if (_currentState == State.None) return;
            ClientBehaviour();

            if (!(agent.velocity.magnitude > 0.1f)) return;

            Quaternion toRotation = Quaternion.LookRotation(agent.velocity, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 10f);
        }

        public void Initialize(int id, DisplayItem item)
        {
            this.id = id;
            _desiredItem = item;

            EnterStore();
        }

        public void Deinitialize()
        {
            if (_desiredItem != null) _desiredItem.BeingViewed = false;
            _desiredItem = null;
        }

        private void ClientBehaviour()
        {
            switch (_currentState)
            {
                case State.None:
                    break;
                case State.Idle:
                    // in this state, the client should wonder around until an item has been choosen or if there are no available items
                    _currentState = State.ChoosingItem;
                    break;
                case State.ChoosingItem:
                    // in this state, the client should go to the item and decide if they want to buy it
                    _currentState = State.GrabbingItem;
                    break;

                case State.GrabbingItem:
                    GrabItem();
                    break;

                case State.WaitingInline:
                    agent.SetDestination(WaitingLineStart.position);
                    if (NearWaitingLine())
                    {
                        OnStartLine?.Invoke(this);
                        _currentState = State.Buying;
                    }
                    break;

                case State.Buying:
                    if (_paid)
                    {
                        firstInLine = false;
                        _currentState = State.Leaving;
                    }

                    break;

                case State.Leaving:
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

        private void EnterStore()
        {
            _currentState = State.Idle;
        }

        /// <summary>
        /// The client goes to the item and grabs it
        /// </summary>
        private void GrabItem()
        {
            if (!CheckBuyItem()) return;
            agent.SetDestination(_desiredItem.displayObject.transform.position);

            if (!NearItem()) return;
            _desiredItem.displayObject.transform.SetParent(gameObject.transform);
            ItemGrabbed?.Invoke(_desiredItem.id, _desiredItem.amount);
            _desiredItem.BeingViewed = false;
            _currentState = State.WaitingInline;
        }

        public void PayItem()
        {
            _paid = true;
            BuyItem();
            //OnLeaveLine?.Invoke();
        }

        private bool CheckBuyItem()
        {
            if (!_desiredItem) return false;

            ListPrice itemList = ItemDatabase.ItemObjects[_desiredItem.Item.data.id].data.listPrice;
            float difference = _desiredItem.Item.price - itemList.CurrentPrice;
            float percentageDifference = (difference / itemList.CurrentPrice) * 100f;

            if (_desiredItem.Item.price >= itemList.CurrentPrice)
            {
                // Client buys item and leaves the store
                if (!(percentageDifference > _willingnessToPay)) return true;

                // If the item is too expensive, the client leaves angry
                _currentState = State.Angry;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gives the player money and removes the item from being displayed
        /// </summary>
        private void BuyItem()
        {
            int finalPrice = _desiredItem.Item.price * _desiredItem.amount;
            OnMoneyAdded?.Invoke(finalPrice);
            OnItemBought?.Invoke(_desiredItem);
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
                OnMoneyAdded.Invoke(_tipValue);
            }
        }

        /// <summary>
        /// Exits the store
        /// </summary>
        private void LeaveStore()
        {
            // TODO update this
            //move to outside of store
            agent.SetDestination(Exit.transform.position);
            _currentState = State.LeftStore;
        }

        private void CheckLeftStore()
        {
            if (!NearExit()) return;

            OnLeftStore?.Invoke();
            gameObject.SetActive(false);
            _currentState = State.None;
        }

        private bool NearWaitingLine()
        {
            return CheckDistance(WaitingLineStart.position, _minimumDistance + 2f);
        }

        private bool NearItem()
        {
            return CheckDistance(_desiredItem.displayObject.transform.position, _minimumDistance);
        }

        private bool NearExit()
        {
            return CheckDistance(Exit.position, _minimumDistance + 1f);
        }

        private bool CheckDistance(Vector3 pos, float distanceDif)
        {
            float distance = Vector3.Distance(transform.position, pos);

            return distance < distanceDif;
        }
    }
}