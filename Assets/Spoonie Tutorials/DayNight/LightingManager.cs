using System.Collections;
using UnityEngine;

namespace Spoonie_Tutorials.DayNight
{
    public class LightingManager : MonoBehaviour
    {
        private static readonly int SunDiscColor = Shader.PropertyToID("_SunDiscColor");
        private static readonly int SunDiscExponent = Shader.PropertyToID("_SunDiscExponent");
        private static readonly int SunDiscMultiplier = Shader.PropertyToID("_SunDiscMultiplier");
        private static readonly int SunHaloColor = Shader.PropertyToID("_SunHaloColor");
        private static readonly int SunHaloExponent = Shader.PropertyToID("_SunHaloExponent");
        private static readonly int SunHaloContribution = Shader.PropertyToID("_SunHaloContribution");
        private static readonly int HorizonLineColor = Shader.PropertyToID("_HorizonLineColor");
        private static readonly int HorizonLineExponent = Shader.PropertyToID("_HorizonLineExponent");
        private static readonly int HorizonLineContribution = Shader.PropertyToID("_HorizonLineContribution");
        private static readonly int SkyGradientTop = Shader.PropertyToID("_SkyGradientTop");
        private static readonly int SkyGradientBottom = Shader.PropertyToID("_SkyGradientBottom");
        private static readonly int SkyGradientExponent = Shader.PropertyToID("_SkyGradientExponent");
        private static readonly int Play = Animator.StringToHash("Play");
        [SerializeField] private Light DirectionalLight;
        [SerializeField] private Material[] skyboxMaterials = new Material[4];
        [SerializeField] private Light[] lights;
        [SerializeField] private Light spotLight;
        [SerializeField] private GameObject[] fires;
        [SerializeField] [Range(0, 0.5f)] private float fireSize = 0.036f;

        [SerializeField, Range(0, 210)] private float TimeOfDay;
        [SerializeField] private Animator animator;
        [SerializeField] private float lightIncreaseDuration = 15f;
        [SerializeField] private float lightIntensity = 30f;
        [SerializeField] private float spotlightIncreaseDuration = 15f;
        [SerializeField] private float spotlightIntensity = 1250f;
        public bool StartCycle { get; set; }
        public float CycleDuration = 24;
        private bool nightStarted = false;

        private Material dynamicSkyboxMaterial;

        private void Awake()
        {
            TimeOfDay = 0;
            StartCycle = false;
            foreach (Light light in lights)
            {
                light.intensity = 0;
            }

            spotLight.intensity = 0;

            foreach (GameObject fire in fires)
            {
                if (fire != null)
                {
                    fire.transform.localScale = Vector3.zero;
                }
            }

            if (!skyboxMaterials[0]) return;
            dynamicSkyboxMaterial = new Material(skyboxMaterials[0]);
            RenderSettings.skybox = dynamicSkyboxMaterial;
        }

        private void Update()
        {
            if (skyboxMaterials == null || skyboxMaterials.Length != 4)
                return;

            if (Application.isPlaying)
            {
                if (!StartCycle || TimeOfDay / CycleDuration >= 0.985f) return;
                TimeOfDay += Time.deltaTime;
                TimeOfDay %= CycleDuration;
                UpdateLighting(TimeOfDay / CycleDuration);
                if (TimeOfDay > CycleDuration / 1.8f && !nightStarted)
                {
                    NightTime();
                }
            }
            else
            {
                UpdateLighting(TimeOfDay / CycleDuration);
            }
        }

        private void UpdateLighting(float timePercent)
        {
            if (!dynamicSkyboxMaterial) return;

            float normalizedTime = timePercent * 4f;
            int currentPeriod = Mathf.FloorToInt(normalizedTime);

            currentPeriod = Mathf.Clamp(currentPeriod, 0, skyboxMaterials.Length - 1);
            int nextPeriod = Mathf.Clamp(currentPeriod + 1, 0, skyboxMaterials.Length - 1);

            float lerpFactor = normalizedTime - currentPeriod;

            if (currentPeriod >= skyboxMaterials.Length - 1)
            {
                lerpFactor = 0f;
                nextPeriod = currentPeriod;
            }

            Material currentMaterial = skyboxMaterials[currentPeriod];
            Material nextMaterial = skyboxMaterials[nextPeriod];

            LerpSkyboxProperties(currentMaterial, nextMaterial, lerpFactor);

            if (!DirectionalLight) return;
            DirectionalLight.transform.localRotation =
                Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));

            float lightIntensityMultiplier = Mathf.Clamp01(Mathf.Sin(timePercent * Mathf.PI));
            DirectionalLight.intensity = lightIntensityMultiplier * 2f;
        }

        private void LerpSkyboxProperties(Material from, Material to, float t)
        {
            Color sunDiscColor = Color.Lerp(from.GetColor(SunDiscColor), to.GetColor(SunDiscColor), t);
            float sunDiscMultiplier = Mathf.Lerp(from.GetFloat(SunDiscMultiplier), to.GetFloat(SunDiscMultiplier), t);
            float sunDiscExponent = Mathf.Lerp(from.GetFloat(SunDiscExponent), to.GetFloat(SunDiscExponent), t);

            Color sunHaloColor = Color.Lerp(from.GetColor(SunHaloColor), to.GetColor(SunHaloColor), t);
            float sunHaloExponent = Mathf.Lerp(from.GetFloat(SunHaloExponent), to.GetFloat(SunHaloExponent), t);
            float sunHaloContribution =
                Mathf.Lerp(from.GetFloat(SunHaloContribution), to.GetFloat(SunHaloContribution), t);

            Color horizonLineColor = Color.Lerp(from.GetColor(HorizonLineColor), to.GetColor(HorizonLineColor), t);
            float horizonLineExponent =
                Mathf.Lerp(from.GetFloat(HorizonLineExponent), to.GetFloat(HorizonLineExponent), t);
            float horizonLineContribution = Mathf.Lerp(from.GetFloat(HorizonLineContribution),
                to.GetFloat(HorizonLineContribution), t);

            Color skyGradientTop = Color.Lerp(from.GetColor(SkyGradientTop), to.GetColor(SkyGradientTop), t);
            Color skyGradientBottom = Color.Lerp(from.GetColor(SkyGradientBottom), to.GetColor(SkyGradientBottom), t);
            float skyGradientExponent =
                Mathf.Lerp(from.GetFloat(SkyGradientExponent), to.GetFloat(SkyGradientExponent), t);

            dynamicSkyboxMaterial.SetColor(SunDiscColor, sunDiscColor);
            dynamicSkyboxMaterial.SetFloat(SunDiscMultiplier, sunDiscMultiplier);
            dynamicSkyboxMaterial.SetFloat(SunDiscExponent, sunDiscExponent);

            dynamicSkyboxMaterial.SetColor(SunHaloColor, sunHaloColor);
            dynamicSkyboxMaterial.SetFloat(SunHaloExponent, sunHaloExponent);
            dynamicSkyboxMaterial.SetFloat(SunHaloContribution, sunHaloContribution);

            dynamicSkyboxMaterial.SetColor(HorizonLineColor, horizonLineColor);
            dynamicSkyboxMaterial.SetFloat(HorizonLineExponent, horizonLineExponent);
            dynamicSkyboxMaterial.SetFloat(HorizonLineContribution, horizonLineContribution);

            dynamicSkyboxMaterial.SetColor(SkyGradientTop, skyGradientTop);
            dynamicSkyboxMaterial.SetColor(SkyGradientBottom, skyGradientBottom);
            dynamicSkyboxMaterial.SetFloat(SkyGradientExponent, skyGradientExponent);

            DynamicGI.UpdateEnvironment();
        }

        public void StartDay()
        {
            StartCycle = true;
            animator.SetTrigger(Play);
            foreach (Light light in lights)
            {
                light.intensity = 0;
            }

            spotLight.intensity = 0;

            foreach (GameObject fire in fires)
            {
                if (fire)
                {
                    fire.transform.localScale = Vector3.zero;
                }
            }

            nightStarted = false;
        }

        private void NightTime()
        {
            nightStarted = true;
            StartCoroutine(IncreaseLightIntensity(0, lightIntensity));
            StartCoroutine(IncreaseSpotlightIntensity(0, spotlightIntensity));
            StartCoroutine(IncreaseFireSize(0, fireSize));
        }

        public void Deinitialize()
        {
            TimeOfDay = 0;
            StartCycle = false;
            UpdateLighting(TimeOfDay / 24f);
            foreach (Light light in lights)
            {
                light.intensity = 0;
            }

            foreach (GameObject fire in fires)
            {
                if (fire != null)
                {
                    fire.transform.localScale = Vector3.zero;
                }
            }

            animator.SetTrigger("Stop");
        }

        private IEnumerator IncreaseLightIntensity(float a, float b)
        {
            float elapsedTime = 0f;
            while (elapsedTime < lightIncreaseDuration)
            {
                elapsedTime += Time.deltaTime;
                float intensity = Mathf.Lerp(a, b, elapsedTime / lightIncreaseDuration);
                foreach (Light light in lights)
                {
                    light.intensity = intensity;
                }

                yield return null;
            }
        }

        private IEnumerator IncreaseSpotlightIntensity(float a, float b)
        {
            float elapsedTime = 0f;
            while (elapsedTime < spotlightIncreaseDuration)
            {
                elapsedTime += Time.deltaTime;
                float intensity = Mathf.Lerp(a, b, elapsedTime / lightIncreaseDuration);
                spotLight.intensity = intensity;
                yield return null;
            }
        }

        private IEnumerator IncreaseFireSize(float fromSize, float toSize)
        {
            float elapsedTime = 0f;
            while (elapsedTime < lightIncreaseDuration)
            {
                elapsedTime += Time.deltaTime;
                float currentSize = Mathf.Lerp(fromSize, toSize, elapsedTime / lightIncreaseDuration);
                Vector3 scale = Vector3.one * currentSize;

                foreach (GameObject fire in fires)
                {
                    if (fire)
                    {
                        fire.transform.localScale = scale;
                    }
                }

                yield return null;
            }
        }

        private void OnValidate()
        {
            if (DirectionalLight)
                return;

            if (RenderSettings.sun)
            {
                DirectionalLight = RenderSettings.sun;
            }
            else
            {
                Light[] lights = FindObjectsOfType<Light>();
                foreach (Light light in lights)
                {
                    if (light.type == LightType.Directional)
                    {
                        DirectionalLight = light;
                        return;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (dynamicSkyboxMaterial)
            {
                DestroyImmediate(dynamicSkyboxMaterial);
            }
        }
    }
}