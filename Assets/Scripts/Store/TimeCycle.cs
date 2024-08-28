using UnityEngine;
using UnityEngine.UI;

namespace Store
{
    public class TimeCycle : MonoBehaviour
    {
        [SerializeField] private Transform directionalLight;
        [SerializeField] private Image timeImage;
        [SerializeField] private GameObject[] lights;
        [SerializeField] private float cycleDuration;
        [SerializeField] private float timeOfDay;
        [SerializeField] private float timeSpeed;
        
        private void Update()
        {
            timeOfDay += Time.deltaTime * timeSpeed;
            timeOfDay %= cycleDuration;
            directionalLight.localRotation = Quaternion.Euler(new Vector3((timeOfDay / cycleDuration) * 360f, 0, 0));
            timeImage.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, (timeOfDay / cycleDuration) * 360f));
            
            if (timeOfDay > cycleDuration / 2)
            {
                NightTime();
            } 
            else
            {
                DayTime();
            }
        }

        private void DayTime()
        {
            foreach (var light in lights)
            {
                light.SetActive(false);
            }
        }

        private void NightTime()
        {
            foreach (var light in lights)
            {
                light.SetActive(true);
            }
        }
    }
}