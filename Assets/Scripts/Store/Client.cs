using System;
using System.Collections;
using System.Linq;
using InventorySystem;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
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
        [SerializeField] private GameObject reactionSprite;

        #endregion

        #region Public Variables

        public int id;
        public static ClientTransforms ClientTransforms;
        public static ItemDatabaseObject ItemDatabase;
        public static Action<Client> OnStartLine;
        public static Action<int, int> OnItemBought;
        public static Action<int> OnMoneyAdded; //TODO change name
        public static Action<int, int> ItemGrabbed;
        public static Action OnLeftStore;
        public static Action OnAngry;
        public static Action OnHappy;
        public static Texture[] ReactionTextures;

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
        private float _reactionTime = 2f;
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

            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        public void Initialize(int id)
        {
            this.id = id;

            CurrentState = State.Idle;
        }

        public void Deinitialize()
        {
            if (ItemDisplayer.DisplayItems[_desiredItemIndex])
                ItemDisplayer.DisplayItems[_desiredItemIndex].BeingViewed = false;
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
                    OnHappy?.Invoke();
                    LeaveStore();
                    break;

                case State.Angry:
                    _happiness--;
                    OnAngry?.Invoke();
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

            CurrentState = State.ChoosingItem;
        }

        private IEnumerator WanderAndChooseItem()
        {
            float wanderTime = Random.Range(_wanderTimemin, _wanderTimeMax);
            float startTime = Time.time;

            while (Time.time - startTime < wanderTime)
            {
                Vector3 randomPosition =
                    new Vector3(
                        Random.Range(ClientTransforms.WanderBoundsMin.position.x,
                            ClientTransforms.WanderBoundsMax.position.x), 0,
                        Random.Range(ClientTransforms.WanderBoundsMin.position.z,
                            ClientTransforms.WanderBoundsMax.position.z));
                agent.SetDestination(randomPosition);

                yield return new WaitForSeconds(3f);
            }

            ChooseItem();
        }
        
        private void ChooseItem()
        {
            var availableItems = ItemDisplayer.DisplayItems.Where(displayItem =>
                displayItem is not null && !displayItem.BeingViewed && !displayItem.Bought &&
                displayItem.amount > 0).ToList();
            
            if (availableItems.Count > 0)
            {
                int randomItemIndex = Random.Range(0, availableItems.Count);
                var chosenItem = availableItems[randomItemIndex];
                chosenItem.BeingViewed = true;
                _desiredItemIndex = Array.IndexOf(ItemDisplayer.DisplayItems, chosenItem);

                CurrentState = State.GrabbingItem;
            }
            else
            {
                // Item not found
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
            var targetItem = ItemDisplayer.DisplayItems[_desiredItemIndex];
            var targetCollider = targetItem.displayObject.GetComponent<Collider>();

            agent.SetDestination(targetCollider.bounds.center);
            float startTime = Time.time;
            float maxWaitTime = 6f;

            while (!NearItem(targetCollider))
            {
                if (Time.time - startTime > maxWaitTime)
                {
                    break;
                }

                yield return null;
            }

            
            if (!CheckBuyItem()) yield break;

            agent.ResetPath();
            
            animator.SetTrigger("GrabItem");

            StartCoroutine(LerpDisplayObjectPosition());
            yield return new WaitForSeconds(_reactionTime);


            if (!targetItem.displayObject)
            {
                targetItem.BeingViewed = false;
                CurrentState = State.Leaving;
                yield break;
            }

            targetItem.displayObject.transform.SetParent(transform);
            targetItem.displayObject.transform.position = itemPosition.position;
            _itemPrice = targetItem.Item.price;
            _itemAmount = targetItem.amount;
            _itemId = targetItem.Item.data.id;
            targetItem.displayObject = null;
            targetItem.CleanDisplay();
            targetItem.inventory.RemoveItem(ItemDatabase.ItemObjects[_itemId].data, _itemAmount);
            CurrentState = State.WaitingInline;
        }

        private IEnumerator LerpDisplayObjectPosition()
        {
            float lerpTime = 0.3f;
            float startTime = Time.time;

            Vector3 initialPosition = ItemDisplayer.DisplayItems[_desiredItemIndex].displayObject.transform.position;
            Vector3 targetPosition = itemPosition.position;

            while (Time.time - startTime < lerpTime)
            {
                float t = (Time.time - startTime) / lerpTime;
                ItemDisplayer.DisplayItems[_desiredItemIndex].displayObject.transform.position =
                    Vector3.Lerp(initialPosition, targetPosition, t);
                yield return null;
            }

            ItemDisplayer.DisplayItems[_desiredItemIndex].displayObject.transform.position = targetPosition;
            ItemDisplayer.DisplayItems[_desiredItemIndex].displayObject.transform.SetParent(itemPosition);
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


            ListPrice itemList = ItemDatabase.ItemObjects[ItemDisplayer.DisplayItems[_desiredItemIndex].Item.data.id]
                .data.listPrice;
            float difference = ItemDisplayer.DisplayItems[_desiredItemIndex].Item.price - itemList.CurrentPrice;
            float percentageDifference = (difference / itemList.CurrentPrice) * 100f;

            InstantiateTexture(percentageDifference);
            if (ItemDisplayer.DisplayItems[_desiredItemIndex].Item.price >= itemList.CurrentPrice)
            {
                // Reaction somewhat happy
                if (percentageDifference < _willingnessToPay) return true;

                // Reaction angry
                CurrentState = State.Angry;
                return false;
            }

            // Reaction very happy
            _leaveTip = true;
            return true;
        }

        public void PayItem()
        {
            _paid = true;
            BuyItem();
            AudioManager.instance.Play(SoldSoundKey);
            CurrentState = _leaveTip ? State.Happy : State.Leaving;
        }


        /// <summary>
        /// Gives the player money and removes the item from being displayed
        /// </summary>
        private void BuyItem()
        {
            int finalPrice = _itemPrice * _itemAmount;
            OnMoneyAdded?.Invoke(finalPrice);
            OnItemBought?.Invoke(_itemId, _itemAmount);
            StartCoroutine(LeaveTipAfterPayment(_waitTime));
        }

        private IEnumerator LeaveTipAfterPayment(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

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
        /// Exits the store
        /// </summary>
        private void LeaveStore()
        {
            // TODO update this
            agent.SetDestination(ClientTransforms.Exit.transform.position);
            CurrentState = State.LeftStore;
        }

        private IEnumerator CheckLeftStore()
        {
            while (!NearExit())
            {
                yield return null;
            }

            CurrentState = State.None;
            OnLeftStore?.Invoke();
            gameObject.SetActive(false);
        }

        private bool NearWaitingLine()
        {
            return CheckDistance(ClientTransforms.WaitingLineStart.position, _minimumDistance + 2f);
        }

        private bool NearItem(Collider targetCollider)
        {
            float combinedRadius = GetComponent<Collider>().bounds.extents.magnitude +
                                   targetCollider.bounds.extents.magnitude;
            return CheckDistance(targetCollider.bounds.center, combinedRadius + _minimumDistance);
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

        private void InstantiateTexture(float percentageDifference)
        {
            Texture selectedSprite = GetSpriteForPercentageDifference(percentageDifference);
            
            if (!selectedSprite) return;
            
            RawImage spriteRenderer = reactionSprite.GetComponent<RawImage>();
            spriteRenderer.texture = selectedSprite;
            reactionSprite.SetActive(true);
            reactionSprite.transform.LookAt(Camera.main.transform);
            
            StartCoroutine(DeactivateSpriteAfterDelay(_reactionTime));

        }
        
        private Texture GetSpriteForPercentageDifference(float percentageDifference)
        {
            const int veryHappyThreshold = -45;
            const int happyThreshold = -30;
            const int somewhatHappyThreshold = -15;
            const int neutralThreshold = 0;
            const int angryThreshold = 15;
            const int veryAngryThreshold = 30;

            switch (percentageDifference)
            {
                case <= veryHappyThreshold:
                    AudioManager.instance.Play("ClientHum");
                    return ReactionTextures[5];
                case <= happyThreshold:
                    AudioManager.instance.Play("ClientHum");
                    return ReactionTextures[4];
                case < somewhatHappyThreshold:
                    AudioManager.instance.Play("ClientHum");
                    return ReactionTextures[3];
                case >= veryAngryThreshold:
                    AudioManager.instance.Play("ClientGrunt");
                    return ReactionTextures[0];
                case >= angryThreshold:
                    AudioManager.instance.Play("ClientGrunt");
                    return ReactionTextures[1];
                case >= neutralThreshold:
                    AudioManager.instance.Play("ClientGrunt");
                    return ReactionTextures[2];
                default:
                    return null;
            }
        }
        
        private IEnumerator DeactivateSpriteAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            reactionSprite.SetActive(false);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, agent.destination);

            Gizmos.DrawSphere(agent.destination, 0.5f);
        }
    }
}