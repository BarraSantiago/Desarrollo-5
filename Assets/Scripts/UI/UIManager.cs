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
        public void SpawnFlyingText(int num)
        {
            GameObject text = Instantiate(flyingtext, flyingTextLocation);
            
            text.transform.position = Vector3.zero;
            
            text.GetComponent<TMP_Text>().text = "+" + num.ToString();
        }
        public void UpdateMoneyText(int money)
        {
            showMoney.text = "$ " + money.ToString();
        }
    }
}