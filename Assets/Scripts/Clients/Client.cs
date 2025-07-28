using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using Store;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Clients
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

    [System.Serializable]
    public class ClientPersonality
    {
        public float patience = 1f; // 0.5-2.0x multiplier
        public float haggling = 1f; // 0.5-1.5x price tolerance
        public float wanderTime = 1f; // 0.7-1.3x wander duration
        public float tipGenerosity = 1f; // 0.5-2.0x tip chance
        public string personalityType; // For debugging/display
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
        [SerializeField] private ClientPersonality personality;

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
        private bool _isInQueue = false;
        private float _queueWaitTime = 0f;
        private float _maxQueueWaitTime = 60f;
        private List<int> _preferredItemIds = new List<int>();
        private List<int> _dislikedItemIds = new List<int>();
        private Dictionary<int, float> _itemMemory = new Dictionary<int, float>(); // itemId -> last seen price
        private bool _hasShoppedBefore;
        
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
        
        public void Initialize(int id)
        {
            this.id = id;
            _hasShoppedBefore = Random.Range(0, 100) < 40; // 40% are returning customers
            GeneratePersonality();
            GenerateItemPreferences();
            LoadItemMemory();
            CurrentState = State.Idle;
        }

        private void ReactToEnvironment()
        {
            // React to long queues
            if (_isInQueue && _queueWaitTime > _maxQueueWaitTime * 0.5f)
            {
                if (Random.Range(0, 100) < 10) // 10% chance per frame
                {
                    animator.SetTrigger("LookAround");
                    if (Random.Range(0, 100) < 20)
                    {
                        InstantiateTexture(10f); // Slight annoyance
                    }
                }
            }
            
            // React to preferred items
            if (CurrentState == State.ChoosingItem)
            {
                foreach (var displayItem in ItemDisplayer.DisplayItems)
                {
                    if (displayItem && _preferredItemIds.Contains(displayItem.Item.data.id))
                    {
                        float distance = Vector3.Distance(transform.position, displayItem.displayObject.transform.position);
                        if (distance < 5f && Random.Range(0, 100) < 5)
                        {
                            // Look towards preferred item
                            Vector3 directionToItem = (displayItem.displayObject.transform.position - transform.position).normalized;
                            transform.rotation = Quaternion.LookRotation(directionToItem);
                            InstantiateTexture(-20f); // Show interest
                        }
                    }
                }
            }
        }
        
        private void Update()
        {
            if (CurrentState == State.None) return;
            
            audioSource.enabled = agent.velocity.magnitude > 0.1f;
            animator.SetFloat("Speed", agent.velocity.magnitude);
            
            ReactToEnvironment(); // Add environmental reactions
            
            if (agent.velocity.magnitude < 0.1f) return;
        
            Quaternion toRotation = Quaternion.LookRotation(agent.velocity, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 10f);
        }
        

        
        private void LoadItemMemory()
        {
            if (!_hasShoppedBefore) return;
            
            // Returning customers remember 1-3 item prices from "previous visits"
            int memorizedItems = Random.Range(1, 4);
            for (int i = 0; i < memorizedItems; i++)
            {
                int itemId = Random.Range(0, ItemDatabase.ItemObjects.Length);
                float rememberedPrice = ItemDatabase.ItemObjects[itemId].data.listPrice.CurrentPrice * Random.Range(0.9f, 1.1f);
                _itemMemory[itemId] = rememberedPrice;
            }
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

        private void GeneratePersonality()
        {
            // Generate random personality traits
            personality = new ClientPersonality();

            int personalityRoll = Random.Range(0, 5);
            switch (personalityRoll)
            {
                case 0: // Impatient
                    personality.patience = Random.Range(0.5f, 0.8f);
                    personality.wanderTime = Random.Range(0.7f, 0.9f);
                    personality.personalityType = "Impatient";
                    break;
                case 1: // Patient
                    personality.patience = Random.Range(1.5f, 2.0f);
                    personality.wanderTime = Random.Range(1.1f, 1.3f);
                    personality.personalityType = "Patient";
                    break;
                case 2: // Generous
                    personality.tipGenerosity = Random.Range(1.5f, 2.0f);
                    personality.haggling = Random.Range(1.2f, 1.5f);
                    personality.personalityType = "Generous";
                    break;
                case 3: // Stingy
                    personality.haggling = Random.Range(0.5f, 0.8f);
                    personality.tipGenerosity = Random.Range(0.3f, 0.7f);
                    personality.personalityType = "Stingy";
                    break;
                default: // Average
                    personality.personalityType = "Average";
                    break;
            }

            // Apply personality to existing values
            _wanderTimemin *= personality.wanderTime;
            _wanderTimeMax *= personality.wanderTime;
            _maxQueueWaitTime *= personality.patience;
            _willingnessToPay = (int)(_willingnessToPay * personality.haggling);
            _tipChance *= personality.tipGenerosity;
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

        private void GenerateItemPreferences()
        {
            // Generate 1-3 preferred items
            int preferenceCount = Random.Range(1, 4);
            for (int i = 0; i < preferenceCount; i++)
            {
                int randomItemId = Random.Range(0, ItemDatabase.ItemObjects.Length);
                if (!_preferredItemIds.Contains(randomItemId))
                    _preferredItemIds.Add(randomItemId);
            }

            // Generate 0-2 disliked items
            int dislikeCount = Random.Range(0, 3);
            for (int i = 0; i < dislikeCount; i++)
            {
                int randomItemId = Random.Range(0, ItemDatabase.ItemObjects.Length);
                if (!_dislikedItemIds.Contains(randomItemId) && !_preferredItemIds.Contains(randomItemId))
                    _dislikedItemIds.Add(randomItemId);
            }
        }

        private void ChooseItem()
        {
            List<DisplayItem> availableItems = ItemDisplayer.DisplayItems.Where(displayItem =>
                displayItem is not null && !displayItem.BeingViewed && !displayItem.Bought &&
                displayItem.amount > 0).ToList();

            if (availableItems.Count > 0)
            {
                // Prioritize preferred items (70% chance)
                List<DisplayItem> preferredItems = availableItems.Where(item =>
                    _preferredItemIds.Contains(item.Item.data.id)).ToList();

                if (preferredItems.Count > 0 && Random.Range(0, 100) < 70)
                {
                    DisplayItem chosenItem = preferredItems[Random.Range(0, preferredItems.Count)];
                    SelectItem(chosenItem);
                    return;
                }

                // Avoid disliked items (80% chance)
                List<DisplayItem> nonDislikedItems = availableItems.Where(item =>
                    !_dislikedItemIds.Contains(item.Item.data.id)).ToList();

                if (nonDislikedItems.Count > 0 && Random.Range(0, 100) < 80)
                {
                    DisplayItem chosenItem = nonDislikedItems[Random.Range(0, nonDislikedItems.Count)];
                    SelectItem(chosenItem);
                    return;
                }

                // Fallback to any available item
                DisplayItem fallbackItem = availableItems[Random.Range(0, availableItems.Count)];
                SelectItem(fallbackItem);
            }
            
        }

        private void SelectItem(DisplayItem item)
        {
            item.BeingViewed = true;
            _desiredItemIndex = Array.IndexOf(ItemDisplayer.DisplayItems, item);
            CurrentState = State.GrabbingItem;
        }

        /// <summary>
        /// The client goes to the item and grabs it
        /// </summary>
        private IEnumerator GrabItem()
        {
            // 1) Move to the item
            DisplayItem targetItem = ItemDisplayer.DisplayItems[_desiredItemIndex];
            Collider targetCollider = targetItem.displayObject.GetComponent<Collider>();

            agent.SetDestination(targetCollider.bounds.center);
            float startTime = Time.time;
            float maxWaitTime = 6f;

            // Wait until we're near the item or time out
            while (!NearItem(targetCollider))
            {
                if (Time.time - startTime > maxWaitTime)
                    break;
                yield return null;
            }

            // Check if client is still willing to buy
            if (!CheckBuyItem())
                yield break;

            // Stop moving
            agent.ResetPath();
            animator.SetFloat("Speed", 0);
            float currentSpeed = agent.speed;
            agent.speed = 0;
            agent.velocity = Vector3.zero;

            yield return null;

            // 2) Trigger the "GrabItem" animation
            targetItem.displayObject.GetComponent<Animator>().enabled = false;
            animator.SetTrigger("GrabItem");

            // Give Animator a frame to transition into the GrabItem state 
            // so GetCurrentAnimatorClipInfo returns the new clip.
            yield return null;

            // 3) Determine the clip length for the GrabItem animation
            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
            float grabAnimLength = 0.5f; // fallback length
            if (clipInfos.Length > 0)
            {
                // If your "GrabItem" clip is first, clipInfos[0] is the correct clip
                grabAnimLength = clipInfos[0].clip.length;
            }

            // 4) Lerp the item to the hand *during* the animation
            yield return new WaitForSeconds(3f);

            // If item got destroyed mid-lerp, leave
            if (!targetItem.displayObject)
            {
                targetItem.BeingViewed = false;
                CurrentState = State.Leaving;
                yield break;
            }

            // 5) Finally, set the item as a child of this client’s transform
            targetItem.displayObject.transform.SetParent(itemPosition);
            targetItem.displayObject.transform.position = itemPosition.position;

            // Cache item info
            _itemPrice = targetItem.Item.Price;
            _itemAmount = targetItem.amount;
            _itemId = targetItem.Item.data.id;

            // Clear the display so it doesn't show the old item
            targetItem.displayObject = null;
            targetItem.CleanDisplay();
            targetItem.inventory.RemoveItem(ItemDatabase.ItemObjects[_itemId].data, _itemAmount);

            agent.speed = currentSpeed;

            // 6) Proceed to queue in line
            CurrentState = State.WaitingInline;
        }

        public void MoveItemToHand()
        {
            DisplayItem targetItem = ItemDisplayer.DisplayItems[_desiredItemIndex];

            if (!targetItem || !targetItem.displayObject) return;

            targetItem.displayObject.transform.position = itemPosition.position;
            targetItem.displayObject.transform.SetParent(itemPosition);
        }


        private IEnumerator StartWaitingLine()
        {
            agent.SetDestination(ClientTransforms.WaitingLineStart.position);
            while (!NearWaitingLine())
            {
                yield return null;
            }

            _isInQueue = true;
            _queueWaitTime = 0f;
            OnStartLine?.Invoke(this);
            CurrentState = State.Buying;

            // Start queue patience countdown
            StartCoroutine(QueuePatienceCountdown());
        }

        private IEnumerator QueuePatienceCountdown()
        {
            while (_isInQueue && _queueWaitTime < _maxQueueWaitTime)
            {
                _queueWaitTime += Time.deltaTime;

                // Show impatience at 75% wait time
                if (_queueWaitTime > _maxQueueWaitTime * 0.75f && Random.Range(0, 100) < 5)
                {
                    InstantiateTexture(25f); // Show angry reaction
                }

                yield return null;
            }

            // If still waiting after max time, leave angry
            if (_isInQueue)
            {
                _isInQueue = false;
                CurrentState = State.Angry;
            }
        }

        private bool CheckBuyItem()
        {
            if (!ItemDisplayer.DisplayItems[_desiredItemIndex]) return false;
        
            int itemId = ItemDisplayer.DisplayItems[_desiredItemIndex].Item.data.id;
            float currentPrice = ItemDisplayer.DisplayItems[_desiredItemIndex].Item.Price;
            
            // Check if returning customer remembers this item's price
            if (_itemMemory.ContainsKey(itemId))
            {
                float rememberedPrice = _itemMemory[itemId];
                if (currentPrice > rememberedPrice * 1.2f) // 20% price increase tolerance
                {
                    InstantiateTexture(30f); // Show disappointment
                    CurrentState = State.Angry;
                    return false;
                }
                else if (currentPrice < rememberedPrice * 0.9f) // Price went down
                {
                    InstantiateTexture(-25f); // Show pleasant surprise
                    _leaveTip = true;
                }
            }
        
            // Rest of existing price checking logic...
            ListPrice itemList = ItemDatabase.ItemObjects[itemId].data.listPrice;
            float difference = currentPrice - itemList.CurrentPrice;
            float percentageDifference = (difference / itemList.CurrentPrice) * 100f;
        
            InstantiateTexture(percentageDifference);
            
            if (currentPrice >= itemList.CurrentPrice)
            {
                if (percentageDifference < _willingnessToPay) return true;
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
            // Add randomization around the exit point to prevent clustering
            Vector3 exitPosition = ClientTransforms.Exit.transform.position;
            Vector3 randomOffset = new Vector3(
                Random.Range(-2f, 2f),
                0,
                Random.Range(-2f, 2f)
            );
            Vector3 randomizedExit = exitPosition + randomOffset;

            agent.SetDestination(randomizedExit);
            CurrentState = State.LeftStore;
        }

        private IEnumerator CheckLeftStore()
        {
            float timeoutDuration = 10f; // Prevent infinite waiting
            float startTime = Time.time;

            while (!NearExit() && Time.time - startTime < timeoutDuration)
            {
                // Check if agent is stuck (not moving for too long)
                if (agent.velocity.magnitude < 0.1f && Time.time - startTime > 3f)
                {
                    // Force move to a new randomized exit position
                    Vector3 exitPosition = ClientTransforms.Exit.transform.position;
                    Vector3 emergencyOffset = new Vector3(
                        Random.Range(-3f, 3f),
                        0,
                        Random.Range(-3f, 3f)
                    );
                    agent.SetDestination(exitPosition + emergencyOffset);
                }

                yield return null;
            }

            CurrentState = State.None;
            OnLeftStore?.Invoke();
            gameObject.SetActive(false);
        }

        private bool NearExit()
        {
            // Use a more generous distance check for the exit
            float exitDistance = _minimumDistance + 3f;

            // Also check if we're generally in the exit area, not just the exact point
            Vector3 exitArea = ClientTransforms.Exit.position;
            float distanceToExit = Vector3.Distance(transform.position, exitArea);

            return distanceToExit < exitDistance;
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
                case < neutralThreshold:
                    AudioManager.instance.Play("ClientHum");
                    return ReactionTextures[2];
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