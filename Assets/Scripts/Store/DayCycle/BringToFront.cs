using UnityEngine;

namespace Store.DayCycle
{
    public class BringToFront : MonoBehaviour
    {
        [SerializeField] private Transform morningImage;
        [SerializeField] private Transform midDayImage;
        [SerializeField] private Transform afternoonImage;
        [SerializeField] private Transform eveningImage;

        public void BringMidDayForward()
        {
            midDayImage.SetAsLastSibling();
        }
        
        public void BringMorningForward()
        {
            morningImage.SetAsLastSibling();
        }
        
        public void BringAfternoonForward()
        {
            afternoonImage.SetAsLastSibling();
        }
        
        public void BringEveningForward()
        {
            eveningImage.SetAsLastSibling();
        }
    }
}