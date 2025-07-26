using UnityEngine;

namespace Store.DayCycle
{
    public class BringToFront : MonoBehaviour
    {
        [SerializeField] private Transform morningImage;
        [SerializeField] private Transform midDayImage;
        [SerializeField] private Transform afternoonImage;
        [SerializeField] private Transform eveningImage;

        public void BringMorningForward()
        {
            morningImage.SetAsLastSibling();
        }
        
        public void BringMidDayForward()
        {
            morningImage.SetAsFirstSibling();
            midDayImage.SetAsLastSibling();
        }
        
        public void BringAfternoonForward()
        {
            midDayImage.SetAsFirstSibling();
            afternoonImage.SetAsLastSibling();
        }
        
        public void BringEveningForward()
        {
            afternoonImage.SetAsFirstSibling();
            eveningImage.SetAsLastSibling();
        }
    }
}