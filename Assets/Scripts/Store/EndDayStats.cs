using TMPro;
using UnityEngine;

namespace Store
{
    public class EndDayStats : MonoBehaviour
    {
        [SerializeField] private TMP_Text satisfiedClients;
        [SerializeField] private TMP_Text angryClients;
        [SerializeField] private TMP_Text prestige;
        [SerializeField] private TMP_Text income;
        [SerializeField] private TMP_Text itemsSold;
        
        public void UpdateStats(int satisfied, int angry, int prestigeValue, int incomeValue, int items)
        {
            satisfiedClients.text = satisfied.ToString();
            angryClients.text = angry.ToString();
            prestige.text = prestigeValue.ToString();
            income.text = incomeValue.ToString();
            itemsSold.text = items.ToString();
        }
    }
}