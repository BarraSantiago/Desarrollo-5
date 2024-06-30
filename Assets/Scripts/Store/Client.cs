using System;
using System.Collections;
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
        public static Transform Entrance;
        public static Transform WaitingLineStart;
        public static Transform WanderBounds1;
        public static Transform WanderBounds2;
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
        private int _timesCheckedItems = 0;
        private int _maxCheckItems = 0;
        private const string SoldSoundKey = "ItemSold";
        private State _currentState;

        private State CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                ClientBehaviour();
            }
        }

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
            if (CurrentState == State.None) return;
            if (!(agent.velocity.magnitude > 0.1f)) return;

            Quaternion toRotation = Quaternion.LookRotation(agent.velocity, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 10f);
        }

        public void Initialize(int id)
        {
            this.id = id;

            //CurrentState = State.Idle;
            ChooseItemToGrab();
        }

        public void Deinitialize()
        {
            if (_desiredItem) _desiredItem.BeingViewed = false;
            _desiredItem = null;
        }

        private void ClientBehaviour()
        {
            switch (CurrentState)
            {
                case State.None:
                    break;
                case State.Idle:
                    StartCoroutine(EnterStoreAndWander());
                    break;
                case State.ChoosingItem:
                    StartCoroutine(WanderAndChooseItem());
                    break;

                case State.GrabbingItem:
                    StartCoroutine(GrabItem());
                    break;

                case State.WaitingInline:
                    StartCoroutine(StartWaitingLine());
                    break;

                case State.Buying:
                    if (_paid)
                    {
                        firstInLine = false;
                        CurrentState = State.Leaving;
                    }

                    break;

                case State.Leaving:
                    LeaveStore();
                    break;

                case State.LeftStore:
                    StartCoroutine(CheckLeftStore());
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


        private IEnumerator EnterStoreAndWander()
        {
            agent.SetDestination(Entrance.position);

            while (!CheckDistance(Entrance.position, _minimumDistance))
            {
                yield return null;
            }

            CurrentState++;
        }

        private IEnumerator WanderAndChooseItem()
        {
            float wanderTime = Random.Range(5f, 10f);
            float startTime = Time.time;

            while (Time.time - startTime < wanderTime)
            {
                Vector3 randomPosition =
                    new Vector3(Random.Range(WanderBounds1.position.x, WanderBounds2.position.x), 0,
                        Random.Range(WanderBounds1.position.z, WanderBounds2.position.z));
                agent.SetDestination(randomPosition);

                yield return new WaitForSeconds(3f);
            }

            ChooseItemToGrab();
        }

        private void ChooseItemToGrab()
        {
            // Get all available items
            DisplayItem[] availableItems = Array.FindAll(ItemDisplayer.DisplayItems,
                displayItem => displayItem is { BeingViewed: false, Bought: false, amount: > 0 });

            if (availableItems.Length > 0)
            {
                // Choose a random item from the available items
                int randomItemIndex = Random.Range(0, availableItems.Length);
                _desiredItem = availableItems[randomItemIndex];
                _desiredItem.BeingViewed = true;

                // Move to the chosen item
                CurrentState = State.GrabbingItem;
            }
            else
            {
                if (_timesCheckedItems >= _maxCheckItems)
                {
                    CurrentState = State.Leaving;
                }
                else
                {
                    _timesCheckedItems++;
                    CurrentState = State.ChoosingItem;
                }
            }
        }

        /// <summary>
        /// The client goes to the item and grabs it
        /// </summary>
        private IEnumerator GrabItem()
        {
            agent.SetDestination(_desiredItem.displayObject.transform.position);

            while (!NearItem())
            {
                yield return null;
            }

            if (!CheckBuyItem()) yield break;

            _desiredItem.displayObject.transform.SetParent(transform);
            ItemGrabbed?.Invoke(_desiredItem.id, _desiredItem.amount);
            _desiredItem.BeingViewed = false;
            CurrentState++;
        }

        private IEnumerator StartWaitingLine()
        {
            agent.SetDestination(WaitingLineStart.position);
            while (!NearWaitingLine())
            {
                yield return null; // Wait for the next frame
            }

            OnStartLine?.Invoke(this);
            CurrentState = State.Buying;
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
                CurrentState = State.Angry;
                return false;
            }

            return true;
        }
        
        public void PayItem()
        {
            _paid = true;
            BuyItem();
            AudioManager.instance.Play(SoldSoundKey);
            CurrentState = State.Leaving;

            //OnLeaveLine?.Invoke();
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
            CurrentState++;
        }

        private IEnumerator CheckLeftStore()
        {
            while (!NearExit())
            {
                yield return null; // Wait for the next frame
            }

            OnLeftStore?.Invoke();
            gameObject.SetActive(false);
            CurrentState = State.None;
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