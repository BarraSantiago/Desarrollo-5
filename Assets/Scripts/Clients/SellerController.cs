using System;
using Store;
using UnityEngine;

namespace Clients
{
    public class SellerController : MonoBehaviour
    {
        [SerializeField] private Animator sellerAnimator;
        [SerializeField] private Animator chestAnimator;
        [SerializeField] private Transform chestTop;
        [SerializeField] private Transform chestMid;
        [SerializeField] private Transform chestBot;

        private int _maxMoney;
        private int _currentMoney;
        private readonly int _charge = Animator.StringToHash("Charge");
        private readonly int _open = Animator.StringToHash("Open");

        private void Start()
        {
            StoreManager.OnMoneyUpdated += UpdateMoney;
        }

        private void Awake()
        {
            Client.OnItemBought += OpenChest;
        }

        private void OnDestroy()
        {
            Client.OnItemBought -= OpenChest;
            StoreManager.OnMoneyUpdated -= UpdateMoney; // Unsubscribe from this event too
        }

        private void UpdateMoney(int currentMoney, int maxMoney)
        {
            _currentMoney = currentMoney;
            MaxMoney = maxMoney;
        }

        public int MaxMoney
        {
            get => _maxMoney;
            set
            {
                _maxMoney = value;
                UpdateChestLevel();
            }
        }

        private void UpdateChestLevel()
        {
            // Add null checks for all chest transforms
            if (chestTop == null || chestMid == null || chestBot == null)
            {
                Debug.LogWarning("One or more chest transforms are null in SellerController");
                return;
            }

            int level = _maxMoney > 0 ? _currentMoney * 100 / _maxMoney : 0;

            switch (level)
            {
                case > 90:
                    chestTop.gameObject.SetActive(true);
                    chestMid.gameObject.SetActive(false);
                    chestBot.gameObject.SetActive(false);
                    break;
                case > 50:
                    chestTop.gameObject.SetActive(false);
                    chestMid.gameObject.SetActive(true);
                    chestBot.gameObject.SetActive(false);
                    break;
                case > 25:
                    chestTop.gameObject.SetActive(false);
                    chestMid.gameObject.SetActive(false);
                    chestBot.gameObject.SetActive(true);
                    break;
                default:
                    chestTop.gameObject.SetActive(false);
                    chestMid.gameObject.SetActive(false);
                    chestBot.gameObject.SetActive(false);
                    break;
            }
        }

        private void OpenChest(int i, int i1)
        {
            if (sellerAnimator != null && chestAnimator != null)
            {
                sellerAnimator.SetTrigger(_charge);
                chestAnimator.SetTrigger(_open);
            }
        }
    }
}