using System.Collections;
using UnityEngine;

//[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    //Scene References
    [SerializeField] private Light DirectionalLight;

    [SerializeField] private Material[] skyboxMaterials = new Material[4]; // Morning, Noon, Afternoon, Night
    [SerializeField] private Light[] lights;
    [SerializeField] private Light spotLight;

    //Variables
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

        // Create dynamic material for skybox interpolation
        if (skyboxMaterials[0] != null)
        {
            dynamicSkyboxMaterial = new Material(skyboxMaterials[0]);
            RenderSettings.skybox = dynamicSkyboxMaterial;
        }
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

        // Map time to 4 periods: Morning (0-0.25), Noon (0.25-0.5), Afternoon (0.5-0.75), Night (0.75-1)
        float normalizedTime = timePercent * 4f;
        int currentPeriod = Mathf.FloorToInt(normalizedTime);

        // Clamp to prevent wrapping back to first element
        currentPeriod = Mathf.Clamp(currentPeriod, 0, skyboxMaterials.Length - 1);
        int nextPeriod = Mathf.Clamp(currentPeriod + 1, 0, skyboxMaterials.Length - 1);

        float lerpFactor = normalizedTime - currentPeriod;

        // If we're at the last period, don't interpolate
        if (currentPeriod >= skyboxMaterials.Length - 1)
        {
            lerpFactor = 0f;
            nextPeriod = currentPeriod;
        }

        Material currentMaterial = skyboxMaterials[currentPeriod];
        Material nextMaterial = skyboxMaterials[nextPeriod];

        // Interpolate skybox shader properties
        LerpSkyboxProperties(currentMaterial, nextMaterial, lerpFactor);

        // Update directional light
        if (!DirectionalLight) return;
        DirectionalLight.transform.localRotation =
            Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));

        // Adjust light intensity based on time of day
        float lightIntensityMultiplier = Mathf.Clamp01(Mathf.Sin(timePercent * Mathf.PI));
        DirectionalLight.intensity = lightIntensityMultiplier * 2f;
    }

    private void LerpSkyboxProperties(Material from, Material to, float t)
    {
        // Sun Disc properties
        Color sunDiscColor = Color.Lerp(from.GetColor("_SunDiscColor"), to.GetColor("_SunDiscColor"), t);
        float sunDiscMultiplier = Mathf.Lerp(from.GetFloat("_SunDiscMultiplier"), to.GetFloat("_SunDiscMultiplier"), t);
        float sunDiscExponent = Mathf.Lerp(from.GetFloat("_SunDiscExponent"), to.GetFloat("_SunDiscExponent"), t);

        // Sun Halo properties
        Color sunHaloColor = Color.Lerp(from.GetColor("_SunHaloColor"), to.GetColor("_SunHaloColor"), t);
        float sunHaloExponent = Mathf.Lerp(from.GetFloat("_SunHaloExponent"), to.GetFloat("_SunHaloExponent"), t);
        float sunHaloContribution =
            Mathf.Lerp(from.GetFloat("_SunHaloContribution"), to.GetFloat("_SunHaloContribution"), t);

        // Horizon Line properties
        Color horizonLineColor = Color.Lerp(from.GetColor("_HorizonLineColor"), to.GetColor("_HorizonLineColor"), t);
        float horizonLineExponent =
            Mathf.Lerp(from.GetFloat("_HorizonLineExponent"), to.GetFloat("_HorizonLineExponent"), t);
        float horizonLineContribution = Mathf.Lerp(from.GetFloat("_HorizonLineContribution"),
            to.GetFloat("_HorizonLineContribution"), t);

        // Sky Gradient properties
        Color skyGradientTop = Color.Lerp(from.GetColor("_SkyGradientTop"), to.GetColor("_SkyGradientTop"), t);
        Color skyGradientBottom = Color.Lerp(from.GetColor("_SkyGradientBottom"), to.GetColor("_SkyGradientBottom"), t);
        float skyGradientExponent =
            Mathf.Lerp(from.GetFloat("_SkyGradientExponent"), to.GetFloat("_SkyGradientExponent"), t);

        // Apply interpolated values to dynamic material
        dynamicSkyboxMaterial.SetColor("_SunDiscColor", sunDiscColor);
        dynamicSkyboxMaterial.SetFloat("_SunDiscMultiplier", sunDiscMultiplier);
        dynamicSkyboxMaterial.SetFloat("_SunDiscExponent", sunDiscExponent);

        dynamicSkyboxMaterial.SetColor("_SunHaloColor", sunHaloColor);
        dynamicSkyboxMaterial.SetFloat("_SunHaloExponent", sunHaloExponent);
        dynamicSkyboxMaterial.SetFloat("_SunHaloContribution", sunHaloContribution);

        dynamicSkyboxMaterial.SetColor("_HorizonLineColor", horizonLineColor);
        dynamicSkyboxMaterial.SetFloat("_HorizonLineExponent", horizonLineExponent);
        dynamicSkyboxMaterial.SetFloat("_HorizonLineContribution", horizonLineContribution);

        dynamicSkyboxMaterial.SetColor("_SkyGradientTop", skyGradientTop);
        dynamicSkyboxMaterial.SetColor("_SkyGradientBottom", skyGradientBottom);
        dynamicSkyboxMaterial.SetFloat("_SkyGradientExponent", skyGradientExponent);

        // Force update global illumination
        DynamicGI.UpdateEnvironment();
    }

    public void StartDay()
    {
        StartCycle = true;
        animator.SetTrigger("Play");
        foreach (Light light in lights)
        {
            light.intensity = 0;
        }

        spotLight.intensity = 0;
        nightStarted = false;
    }

    private void NightTime()
    {
        nightStarted = true;
        StartCoroutine(IncreaseLightIntensity(0, lightIntensity));
        StartCoroutine(IncreaseSpotlightIntensity(0, spotlightIntensity));
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

    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        if (RenderSettings.sun != null)
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
        if (dynamicSkyboxMaterial != null)
        {
            DestroyImmediate(dynamicSkyboxMaterial);
        }
    }
}