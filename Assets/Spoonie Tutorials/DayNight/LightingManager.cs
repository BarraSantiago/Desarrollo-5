using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    //Scene References
    [SerializeField] private Light DirectionalLight;

    [SerializeField] private LightingPreset Preset;
    [SerializeField] private Light[] lights;

    //Variables
    [SerializeField, Range(0, 210)] private float TimeOfDay;
    [SerializeField] private Animator animator;
    [SerializeField] private float lightIncreaseDuration = 15f;
    [SerializeField] private float lightIntensity = 30f;
    public bool StartCycle { get; set; }
    public float CycleDuration = 24;
    private bool nightStarted = false;

    private void Awake()
    {
        TimeOfDay = 0;
        StartCycle = false;
        foreach (Light light in lights)
        {
            light.intensity = 0;
        }
    }

    private void Update()
    {
        if (!Preset)
            return;

        if (Application.isPlaying)
        {
            if (!StartCycle || TimeOfDay / CycleDuration >= 0.985f) return;
            //(Replace with a reference to the game time)
            TimeOfDay += Time.deltaTime;
            TimeOfDay %= CycleDuration; //Modulus to ensure always between 0-24
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
        //Set ambient and fog
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        //If the directional light is set then rotate and set it's color, I actually rarely use the rotation because it casts tall shadows unless you clamp the value
        if (!DirectionalLight) return;
        DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);

        DirectionalLight.transform.localRotation =
            Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
    }

    public void StartDay()
    {
        StartCycle = true;
        animator.SetTrigger("Play");
        foreach (Light light in lights)
        {
            light.intensity = 0;
        }
        nightStarted = false;
    }

    private void NightTime()
    {
        nightStarted = true;
        StartCoroutine(IncreaseLightIntensity(0, lightIntensity));
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

    //Try to find a directional light to use if we haven't set one
    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        //Search for lighting tab sun
        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        //Search scene for light that fits criteria (directional)
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
}