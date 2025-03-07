using System.Collections;
using System.Linq;
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
        [SerializeField] private float lightIncreaseDuration = 2f;
        [SerializeField] private float lightIntensity = 30f;
        private Light[] lightComponents;

        private void Awake()
        {
            lightComponents = lights.Select(light => light.GetComponent<Light>()).ToArray();
        }

        public float CycleDuration { get; set; }
        public bool StartCycle { get; set; }

        private void Update()
        {
            if (!StartCycle) return;
            if (timeOfDay / CycleDuration >= 0.75) return;
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
            StartCoroutine(IncreaseLightIntensity(lightIntensity, 0));
        }

        private void NightTime()
        {
            StartCoroutine(IncreaseLightIntensity(0, lightIntensity));
        }

        private IEnumerator IncreaseLightIntensity(float a, float b)
        {
            float elapsedTime = 0f;
            while (elapsedTime < lightIncreaseDuration)
            {
                elapsedTime += Time.deltaTime;
                float intensity = Mathf.Lerp(a, b, elapsedTime / lightIncreaseDuration);
                foreach (Light light in lightComponents)
                {
                    light.intensity = intensity;
                }

                yield return null;
            }
        }

        public void Deinitialize()
        {
            timeOfDay = 0;
            StartCycle = false;
            directionalLight.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            timeImage.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            DayTime();
        }
    }
}