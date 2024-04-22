using InventorySystem;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private TMP_Text showMoney;
    public int Money { get; set; }
    [SerializeField] public Inventory Inventory;

}