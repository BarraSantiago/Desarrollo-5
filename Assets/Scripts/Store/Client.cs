using System;
using InventorySystem;
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

        #region Serialized Variables

        [SerializeField] public NavMeshAgent agent;
        [SerializeField] private Button cobrar; // TODO update this

        #endregion

        #region Public Variables

        public int id;
        public static Transform exit;
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

        private int _tipValue;
        private int _itemAmount;
        private bool cobrado = false;
        [SerializeField] private float _minimumDistance = 1.6f;
        private bool _waitingForPlayer = false;
        private State _currentState;
        private State _previousState;
        private DisplayItem desiredItem;
        private GameObject _itemInstance;

        #endregion

        private void Start()
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            cobrar.onClick.AddListener(Cobrado);
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
            desiredItem = item;
            _itemAmount = Random.Range(1, desiredItem.amount + 1);

            EnterStore();
        }

        public void Deinitialize()
        {
            desiredItem.BeingViewed = false;
            _itemAmount = 0;
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
                    OnStartLine?.Invoke(this);
                    _currentState = State.Buying;
                    break;

                case State.Buying:
                    _waitingForPlayer = true;
                    if (PlayerInteraction())
                    {
                        _waitingForPlayer = false;
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
            agent.SetDestination(desiredItem.Object.transform.position);

            if (!NearItem()) return;
            desiredItem.Object.transform.SetParent(gameObject.transform);
            ItemGrabbed?.Invoke(desiredItem.id, _itemAmount);
            desiredItem.BeingViewed = false;
            _currentState = State.WaitingInline;
        }

        /// <summary>
        /// Way for the player to charge the client
        /// </summary>
        /// <returns></returns>
        private bool PlayerInteraction()
        {
            if (!firstInLine) return false;
            cobrar.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.down);
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
            OnLeaveLine?.Invoke();
        }

        private bool CheckBuyItem()
        {
            if (desiredItem == null) return false;

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

            return true;
        }

        /// <summary>
        /// Gives the player money and removes the item from being displayed
        /// </summary>
        private void BuyItem()
        {
            int finalPrice = desiredItem.ItemObject.price * _itemAmount;
            OnMoneyAdded?.Invoke(finalPrice);
            OnItemBought?.Invoke(desiredItem);
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
            cobrado = false;
            // TODO update this
            //move to outside of store
            agent.SetDestination(exit.transform.position);
            _currentState = State.LeftStore;
        }

        private void CheckLeftStore()
        {
            if (!NearExit()) return;

            OnLeftStore?.Invoke();
            gameObject.SetActive(false);
            _currentState = State.None;
        }

        private bool NearItem()
        {
            return CheckDistance(desiredItem.Object.transform.position, _minimumDistance);
        }

        private bool NearExit()
        {
            return CheckDistance(exit.position, _minimumDistance + 1f);
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