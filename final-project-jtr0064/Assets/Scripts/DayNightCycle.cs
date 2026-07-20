using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;
    public float dayLengthInSeconds = 240f;
    public float nightLengthInSeconds = 60f;
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

        float fullCycleLength = Mathf.Max(0.01f, dayLengthInSeconds + nightLengthInSeconds);
        timeOfDay += Time.deltaTime / fullCycleLength;

        if (timeOfDay >= 1) {
            timeOfDay = 0;
        }

        UpdateLighting(fullCycleLength);
    }

    private void UpdateLighting(float fullCycleLength) {
        float sunAngle = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // timeOfDay sweeps 0..1 at a constant rate over fullCycleLength seconds; the split
        // between the day and night portions of that range is what makes each phase last
        // its own configured duration, even though the sun's angular speed stays constant.
        float dayFraction = dayLengthInSeconds / fullCycleLength;
        IsNight = timeOfDay >= dayFraction;

        float lightAmount;
        if (IsNight) {
            lightAmount = 0f;
        } else {
            float dayPhaseT = dayFraction > 0f ? timeOfDay / dayFraction : 0f;
            lightAmount = Mathf.Clamp01(Mathf.Sin(dayPhaseT * Mathf.PI));
        }

        sun.intensity = Mathf.Lerp(nightIntensity, dayIntensity, lightAmount);

        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, lightAmount);

        if (RenderSettings.skybox != null) {
            RenderSettings.skybox.SetColor("_Tint", Color.Lerp(nightSkyTint, daySkyTint, lightAmount));
            RenderSettings.skybox.SetFloat("_Explorer", Mathf.Lerp(0.08f, 0.65f, lightAmount));
        }
    }
}
