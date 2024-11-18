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

        private void Start()
        {
            StoreManager.OnMoneyUpdated += UpdateMoney;
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
            int level = _currentMoney * 100 / _maxMoney;
            
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

        private int _maxMoney;
        private int _currentMoney;
        private readonly int _charge = Animator.StringToHash("Charge");
        private readonly int _open = Animator.StringToHash("Open");

        private void Awake()
        {
            Client.OnItemBought += OpenChest;
        }

        private void OnDestroy()
        {
            Client.OnItemBought -= OpenChest;
        }

        private void OpenChest(int i, int i1)
        {
            sellerAnimator.SetTrigger(_charge);
            chestAnimator.SetTrigger(_open);
        }
    }
}