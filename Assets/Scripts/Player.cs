using System;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private TMP_Text showMoney;
    public static int Money { get; set; }

    private void Start()
    {
        UpdateMoneyText();
    }

    public void UpdateMoneyText()
    {
        showMoney.text = "$ " + Money.ToString();
    }
    
}