using UnityEngine;
using UnityEngine.UI;

namespace Store
{
    public class TimeCycle : MonoBehaviour
    {
        [SerializeField] private Transform directionalLight;
        [SerializeField] private Image timeImage;
        [SerializeField] private GameObject[] lights;
        [SerializeField] private float timeOfDay;
        [SerializeField] private float timeSpeed;
        
        public float CycleDuration { get; set; }
        public bool StartCycle { get; set; }
        
        private void Update()
        {
            if(!StartCycle) return;
            if(timeOfDay >= CycleDuration) return;
            timeOfDay += Time.deltaTime * timeSpeed;
            timeOfDay %= CycleDuration;
            directionalLight.localRotation = Quaternion.Euler(new Vector3((timeOfDay / CycleDuration) * 360f, 0, 0));
            timeImage.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, (timeOfDay / CycleDuration) * 360f));
            
            if (timeOfDay > CycleDuration / 1.8)
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

        public void Deinitialize()
        {
            StartCycle = false;
            timeOfDay = 0;
            directionalLight.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            timeImage.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
    }
}