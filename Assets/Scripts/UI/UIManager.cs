using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject flyingtext;
        [SerializeField] private Transform flyingTextLocation;
        [SerializeField] private TMP_Text showMoney;

        public static Canvas MainCanvas;
        public static Canvas TopCanvas;

        private void Start()
        {
            TopCanvas = GameObject.Find("TopCanvas").GetComponent<Canvas>();
        }

        public void SpawnFlyingText(int num)
        {
            GameObject flyingText = Instantiate(flyingtext, flyingTextLocation);
            
            flyingText.GetComponent<TMP_Text>().text = "+" + num.ToString();
        }
        public void UpdateMoneyText(int money)
        {
            showMoney.text = money.ToString();
        }
    }
}