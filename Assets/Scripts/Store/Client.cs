using System;
using System.Collections;
using InventorySystem;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Store
{
    [Serializable]
    public class ClientTransforms
    {
        public Transform SpawnPoint;
        public Transform Exit;
        public Transform Entrance;
        public Transform WaitingLineStart;
        public Transform WanderBoundsMin;
        public Transform WanderBoundsMax;
    }
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
        [SerializeField] private Transform itemPosition;
        [SerializeField] private AudioSource audioSource;

        #endregion

        #region Public Variables
    
        public int id;
        public static ClientTransforms ClientTransforms;
        public static ItemDatabaseObject ItemDatabase;
        public static Action<Client> OnStartLine;
        public static Action<int> OnItemBought;
        public static Action<int> OnMoneyAdded; //TODO change name
        public static Action<int, int> ItemGrabbed;
        public static Action OnLeftStore;

        /// <summary>
        /// Bool that indicates if the client is in the shop
        /// </summary>
        public bool InShop => _currentState < State.LeftStore;
        public bool firstInLine;

        #endregion

        #region Private Variables

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
        private int tipValue => (int)(_itemPrice * _itemAmount * 0.1f);

        private int _timesCheckedItems = 0;
        private int _maxCheckItems = 0;
        private int _desiredItemIndex;
        private int _itemPrice;
        private int _itemAmount;
        private int _itemId;
        private float _wanderTimemin = 7f;
        private float _wanderTimeMax = 15f;
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

        private GameObject _itemInstance;

        private Animator animator;
        private bool _leaveTip;
        private float _tipChance = 85f;
        private float _waitTime = 0.3f;

        #endregion

        private void Start()
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (CurrentState == State.None) return;
            audioSource.enabled = agent.velocity.magnitude > 0.1f;
            if (agent.velocity.magnitude < 0.1f) return;

            Quaternion toRotation = Quaternion.LookRotation(agent.velocity, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 10f);

            animator.SetFloat("speed", agent.velocity.magnitude);
        }

        public void Initialize(int id)
        {
            this.id = id;

            CurrentState = State.Idle;
        }

        public void Deinitialize()
        {
            if (ItemDisplayer.DisplayItems[_desiredItemIndex]) ItemDisplayer.DisplayItems[_desiredItemIndex].BeingViewed = false;
            ItemDisplayer.DisplayItems[_desiredItemIndex] = null;
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
            agent.SetDestination(ClientTransforms.Entrance.position);

            while (!CheckDistance(ClientTransforms.Entrance.position, _minimumDistance))
            {
                yield return null;
            }

            CurrentState++;
        }

        private IEnumerator WanderAndChooseItem()
        {
            float wanderTime = Random.Range(_wanderTimemin, _wanderTimeMax);
            float startTime = Time.time;

            while (Time.time - startTime < wanderTime)
            {
                Vector3 randomPosition =
                    new Vector3(Random.Range(ClientTransforms.WanderBoundsMin.position.x, ClientTransforms.WanderBoundsMax.position.x), 0,
                        Random.Range(ClientTransforms.WanderBoundsMin.position.z, ClientTransforms.WanderBoundsMax.position.z));
                agent.SetDestination(randomPosition);

                yield return new WaitForSeconds(3f);
            }

            ChooseItem();
        }


        private void ChooseItem()
        {
            // Get all available items
            DisplayItem[] availableItems = Array.FindAll(ItemDisplayer.DisplayItems,
                displayItem => displayItem is { BeingViewed: false, Bought: false, amount: > 0 });

            if (availableItems.Length > 0)
            {
                // Choose a random item from the available items
                int randomItemIndex = Random.Range(0, availableItems.Length);
                ItemDisplayer.DisplayItems[_desiredItemIndex] = availableItems[randomItemIndex];
                _itemInstance = ItemDisplayer.DisplayItems[_desiredItemIndex].displayObject;
                ItemDisplayer.DisplayItems[_desiredItemIndex].BeingViewed = true;
                _desiredItemIndex = Array.IndexOf(ItemDisplayer.DisplayItems, ItemDisplayer.DisplayItems[_desiredItemIndex]);
                
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
            agent.SetDestination(ItemDisplayer.DisplayItems[_desiredItemIndex].displayObject.transform.position);
            float startTime = Time.time;
            float maxWaitTime = 5f;

            while (!NearItem())
            {
                if (Time.time - startTime > maxWaitTime)
                {
                    break;
                }
                yield return null;
            }

            if (!CheckBuyItem()) yield break;
            
            
            ItemDisplayer.DisplayItems[_desiredItemIndex].displayObject.transform.SetParent(transform);
            ItemDisplayer.DisplayItems[_desiredItemIndex].displayObject.transform.position = itemPosition.position;
            _itemPrice = ItemDisplayer.DisplayItems[_desiredItemIndex].Item.price;
            _itemAmount = ItemDisplayer.DisplayItems[_desiredItemIndex].amount;
            _itemId = ItemDisplayer.DisplayItems[_desiredItemIndex].Item.data.id;
            ItemDisplayer.DisplayItems[_desiredItemIndex].displayObject = null;
            ItemDisplayer.DisplayItems[_desiredItemIndex].CleanDisplay();
            CurrentState++;
        }


        private IEnumerator StartWaitingLine()
        {
            agent.SetDestination(ClientTransforms.WaitingLineStart.position);
            while (!NearWaitingLine())
            {
                yield return null;
            }

            OnStartLine?.Invoke(this);
            CurrentState = State.Buying;
        }

   

        private bool CheckBuyItem()
        {
            if (!ItemDisplayer.DisplayItems[_desiredItemIndex]) return false;

            ListPrice itemList = ItemDatabase.ItemObjects[ItemDisplayer.DisplayItems[_desiredItemIndex].Item.data.id].data.listPrice;
            float difference = ItemDisplayer.DisplayItems[_desiredItemIndex].Item.price - itemList.CurrentPrice;
            float percentageDifference = (difference / itemList.CurrentPrice) * 100f;

            if (ItemDisplayer.DisplayItems[_desiredItemIndex].Item.price >= itemList.CurrentPrice)
            {
                // Client buys item and leaves the store
                if (!(percentageDifference > _willingnessToPay)) return true;

                // If the item is too expensive, the client leaves angry
                CurrentState = State.Angry;
                return false;
            }
            
            _leaveTip = true;
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
            int finalPrice = _itemPrice * _itemAmount;
            OnMoneyAdded?.Invoke(finalPrice);
            OnItemBought?.Invoke(_itemId);
            StartCoroutine(LeaveTipAfterPayment(_waitTime));
        }
        
        private IEnumerator LeaveTipAfterPayment(float waitTime)
        {
            // Wait for the specified time
            yield return new WaitForSeconds(waitTime);

            // Call the LeaveTip method
            LeaveTip(_tipChance);
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
                OnMoneyAdded?.Invoke(tipValue);
            }
        }

        /// <summary>
        /// ClientTransforms.Exits the store
        /// </summary>
        private void LeaveStore()
        {
            // TODO update this
            //move to outside of store
            agent.SetDestination(ClientTransforms.Exit .transform.position);
            CurrentState++;
        }

        private IEnumerator CheckLeftStore()
        {
            while (!NearExit())
            {
                yield return null; // Wait for the next frame
            }

            CurrentState = State.None;
            OnLeftStore?.Invoke();
            gameObject.SetActive(false);
        }

        private bool NearWaitingLine()
        {
            return CheckDistance(ClientTransforms.WaitingLineStart.position, _minimumDistance + 2f);
        }

        private bool NearItem()
        {
            float combinedRadius = GetComponent<Collider>().bounds.extents.magnitude + ItemDisplayer.DisplayItems[_desiredItemIndex].displayObject.GetComponent<Collider>().bounds.extents.magnitude;

            return CheckDistance(ItemDisplayer.DisplayItems[_desiredItemIndex].displayObject.transform.position, combinedRadius + _minimumDistance);
        }

        private bool NearExit()
        {
            return CheckDistance(ClientTransforms.Exit.position, _minimumDistance + 1f);
        }

        private bool CheckDistance(Vector3 pos, float distanceDif)
        {
            float distance = Vector3.Distance(transform.position, pos);

            return distance < distanceDif;
        }
    }
}