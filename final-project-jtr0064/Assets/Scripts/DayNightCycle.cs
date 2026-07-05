using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;
    public float fulleDayLengthInSeconds = 120f;
    private float dayIntensity = 0.2f;
    private float nightIntensity = 0.05f;
    private Color dayAmbientColor = new Color(0.55f, 0.58f, 0.62f);
    private Color nightAmbientColor = new Color(0.01f, 0.012f, 0.018f);
    private Color daySkyTint = new Color(0.35f, 0.45f, 0.6f);
    private Color nightSkyTint = new Color(0.005f, 0.0065f, 0.01f);
    private float timeOfDay = 0.25f;

    public bool IsNight { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (sun == null) {
            return;
        }

        timeOfDay += Time.deltaTime / fulleDayLengthInSeconds;

        if (timeOfDay >= 1) {
            timeOfDay = 0;
        }

        UpdateLighting();
    }

    private void UpdateLighting() {
        float sunAngle = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        float lightAmount = Mathf.Clamp01(Mathf.Sin(timeOfDay * Mathf.PI));
        sun.intensity = Mathf.Lerp(nightIntensity, dayIntensity, lightAmount);

        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, lightAmount);

        if (RenderSettings.skybox != null) {
            RenderSettings.skybox.SetColor("_Tint", Color.Lerp(nightSkyTint, daySkyTint, lightAmount));
            RenderSettings.skybox.SetFloat("_Explorer", Mathf.Lerp(0.08f, 0.65f, lightAmount));
        }

        IsNight = lightAmount < 0.25f;
    }
}
