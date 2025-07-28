using System.Collections;
using UnityEngine;

namespace Spoonie_Tutorials.DayNight
{
    public class LightingManager : MonoBehaviour
    {
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
        [SerializeField] private Material particleMaterial;
        [SerializeField] private ParticleSystem[] particleSystems;
        [SerializeField] private Color nightParticleColor = Color.gray;
        [SerializeField] private float particleColorTransitionDuration = 15f;

        #region Private Fields

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
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private Color[] originalParticleColors;
        private Color[] originalEmissionColors;
        private Material[] originalMaterials;
        public bool StartCycle { get; set; }
        public float CycleDuration = 24;
        private bool nightStarted = false;
        private Material dynamicSkyboxMaterial;
        private Color originalMatColor;

        #endregion

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

            // Store original particle colors and emission colors
            if (particleSystems != null && particleSystems.Length > 0)
            {
                originalParticleColors = new Color[particleSystems.Length];
                originalEmissionColors = new Color[particleSystems.Length];
                originalMaterials = new Material[particleSystems.Length];

                for (int i = 0; i < particleSystems.Length; i++)
                {
                    if (particleSystems[i] != null)
                    {
                        ParticleSystem.MainModule main = particleSystems[i].main;
                        originalParticleColors[i] = main.startColor.color;

                        ParticleSystemRenderer renderer = particleSystems[i].GetComponent<ParticleSystemRenderer>();
                        if (renderer && renderer.material)
                        {
                            originalMaterials[i] = renderer.material;
                            originalEmissionColors[i] = renderer.material.GetColor(EmissionColor);
                        }
                    }
                }
            }
            originalMatColor = particleMaterial.GetColor(EmissionColor);
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

            StartCoroutine(TransitionParticleColors(false));
            nightStarted = false;
        }

        private void NightTime()
        {
            nightStarted = true;
            StartCoroutine(IncreaseLightIntensity(0, lightIntensity));
            StartCoroutine(IncreaseSpotlightIntensity(0, spotlightIntensity));
            StartCoroutine(IncreaseFireSize(0, fireSize));
            StartCoroutine(TransitionParticleColors(true));
        }

        private IEnumerator TransitionParticleColors(bool toNight)
        {
            if (particleSystems == null || originalParticleColors == null) yield break;

            float elapsedTime = 0f;
            while (elapsedTime < particleColorTransitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / particleColorTransitionDuration;

                for (int i = 0; i < particleSystems.Length; i++)
                {
                    if (!particleSystems[i]) continue;

                    ParticleSystem.MainModule main = particleSystems[i].main;
                    ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystems[i].colorOverLifetime;
                    ParticleSystemRenderer renderer = particleSystems[i].GetComponent<ParticleSystemRenderer>();

                    Color targetColor = toNight ? nightParticleColor : originalParticleColors[i];
                    Color currentColor = toNight ? originalParticleColors[i] : nightParticleColor;

                    // Update start color
                    main.startColor = Color.Lerp(currentColor, targetColor, t);

                    // Update color over lifetime if enabled
                    if (colorOverLifetime.enabled)
                    {
                        Gradient gradient = new Gradient();
                        GradientColorKey[] colorKeys = new GradientColorKey[2];
                        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

                        Color lerpedColor = Color.Lerp(currentColor, targetColor, t);

                        colorKeys[0].color = lerpedColor;
                        colorKeys[0].time = 0.0f;
                        colorKeys[1].color = lerpedColor;
                        colorKeys[1].time = 1.0f;

                        alphaKeys[0].alpha = lerpedColor.a;
                        alphaKeys[0].time = 0.0f;
                        alphaKeys[1].alpha = lerpedColor.a;
                        alphaKeys[1].time = 1.0f;

                        gradient.SetKeys(colorKeys, alphaKeys);
                        colorOverLifetime.color = gradient;
                    }

                    // Update emission color if material exists
                    if (renderer && originalMaterials[i] &&
                        originalMaterials[i].HasProperty(EmissionColor))
                    {
                        Color targetEmissionColor = toNight ? nightParticleColor : originalEmissionColors[i];
                        Color currentEmissionColor = toNight ? originalEmissionColors[i] : nightParticleColor;
                        Color lerpedEmissionColor = Color.Lerp(currentEmissionColor, targetEmissionColor, t);

                        particleMaterial.SetColor(EmissionColor, lerpedEmissionColor);
                        renderer.material.SetColor(EmissionColor, lerpedEmissionColor);
                        particleSystems[i].Clear();
                        particleSystems[i].Play();
                    }
                }

                yield return null;
            }
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

            // Reset particle systems to original colors
            if (particleSystems != null && originalParticleColors != null)
            {
                for (int i = 0; i < particleSystems.Length; i++)
                {
                    if (particleSystems[i] != null)
                    {
                        ParticleSystem.MainModule main = particleSystems[i].main;
                        main.startColor = originalParticleColors[i];

                        // Reset emission colors
                        ParticleSystemRenderer renderer = particleSystems[i].GetComponent<ParticleSystemRenderer>();
                        if (renderer && originalMaterials[i] && originalMaterials[i].HasProperty(EmissionColor))
                        {
                            renderer.material.SetColor(EmissionColor, originalEmissionColors[i]);
                        }
                    }
                }
            }

            // Reset the shared particle material
            if (particleMaterial && originalEmissionColors != null && originalEmissionColors.Length > 0)
            {
                particleMaterial.SetColor(EmissionColor, originalMatColor);
            }

            animator.SetTrigger("Stop");
            nightStarted = false;
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
            particleMaterial.SetColor(EmissionColor, originalMatColor);
        }
    }
}