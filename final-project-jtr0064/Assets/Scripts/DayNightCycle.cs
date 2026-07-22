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
    private float timeOfDay = 0.25f; // start partway into the day

    // Warm color blended into ambient/sky during dusk and dawn, weighted by how deep into
    // the transition the smoothed light level currently is (see twilightWeight below).
    private Color twilightTint = new Color(0.95f, 0.55f, 0.3f);

    private float fogChance = 0.2f;
    private Color fogColor = new Color(0.75f, 0.76f, 0.78f);
    private float fogDensity = 0.045f;

    // Raw day/night lighting target is a half-sine over the day that already reaches 0 at
    // both ends, but its *slope* is nonzero right at the day/night boundary while night holds
    // a flat 0 - that mismatched slope is what reads as an abrupt cutoff. Easing the applied
    // value toward that target (instead of applying it directly) smooths dusk/dawn out.
    private float smoothedLightAmount;
    private float lightAmountVelocity;
    // True once UpdateLighting has run at least one frame - see the snap-to-target check below.
    private bool lightInitialized;
    [Tooltip("Roughly how many seconds a full night<->day light swing takes to ease across.")]
    public float lightTransitionSeconds = 10f;

    // Same idea for fog: fades density in/out instead of snapping RenderSettings.fog on/off.
    private float smoothedFogDensity;
    private float fogDensityVelocity;
    [Tooltip("Roughly how many seconds fog takes to fully fade in or out.")]
    public float fogTransitionSeconds = 2f;

    // Treated as "just came out of night" so the very first day also gets a fog roll.
    private bool wasNight = true;

    // Rolled once per day; only actually foggy while it's also daytime, since the roll
    // stays set through the following night otherwise. Use IsFoggy for that check.
    private bool isFoggyToday;

    public bool IsNight { get; private set; }
    public bool IsFoggy => isFoggyToday && !IsNight;

    void Start()
    {
        if (RenderSettings.skybox != null) {
            // Work on a runtime-only copy so the per-frame _Tint/_Exposure writes below don't
            // dirty (and persist into) the shared skybox material asset in-editor.
            RenderSettings.skybox = new Material(RenderSettings.skybox);
        }
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
            timeOfDay -= 1f;
        }

        UpdateLighting(fullCycleLength);

        // Fog chance is re-rolled once per day, at the moment night ends.
        if (wasNight && !IsNight) {
            isFoggyToday = Random.value < fogChance;
        }
        wasNight = IsNight;
    }

    private void UpdateLighting(float fullCycleLength) {
        float sunAngle = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // timeOfDay sweeps 0..1 at a constant rate over fullCycleLength seconds; the split
        // between the day and night portions of that range is what makes each phase last
        // its own configured duration, even though the sun's angular speed stays constant.
        float dayFraction = dayLengthInSeconds / fullCycleLength;
        IsNight = timeOfDay >= dayFraction;

        float targetLightAmount;
        if (IsNight) {
            targetLightAmount = 0f;
        } else {
            float dayPhaseT = dayFraction > 0f ? timeOfDay / dayFraction : 0f;
            targetLightAmount = Mathf.Clamp01(Mathf.Sin(dayPhaseT * Mathf.PI));
        }

        if (!lightInitialized) {
            // Snap straight to the correct brightness for the scene's starting timeOfDay
            // instead of easing up from a cold 0 - that easing is for smoothing dusk/dawn
            // mid-game, not for making the game boot dim and fade in.
            smoothedLightAmount = targetLightAmount;
            lightAmountVelocity = 0f;
            lightInitialized = true;
        } else {
            smoothedLightAmount = Mathf.SmoothDamp(smoothedLightAmount, targetLightAmount, ref lightAmountVelocity, lightTransitionSeconds);
        }

        sun.intensity = Mathf.Lerp(nightIntensity, dayIntensity, smoothedLightAmount);

        Color ambient = Color.Lerp(nightAmbientColor, dayAmbientColor, smoothedLightAmount);
        Color skyTint = Color.Lerp(nightSkyTint, daySkyTint, smoothedLightAmount);

        // Twilight warmth peaks mid-transition (smoothedLightAmount near 0.5) and fades back
        // out at full day or full night.
        float twilightWeight = 1f - Mathf.Abs(smoothedLightAmount * 2f - 1f);
        ambient = Color.Lerp(ambient, twilightTint, twilightWeight * 0.35f);
        skyTint = Color.Lerp(skyTint, twilightTint, twilightWeight * 0.5f);

        RenderSettings.ambientLight = ambient;

        if (RenderSettings.skybox != null) {
            // PT_Skybox_mat (the assigned skybox) exposes _Tint and _Exposure - guard with
            // HasProperty so this stays a no-op instead of a silent typo if the skybox ever
            // changes to a shader without one of these properties.
            if (RenderSettings.skybox.HasProperty("_Tint")) {
                RenderSettings.skybox.SetColor("_Tint", skyTint);
            }
            if (RenderSettings.skybox.HasProperty("_Exposure")) {
                RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(0.08f, 0.65f, smoothedLightAmount));
            }
        }

        float targetFogDensity = IsFoggy ? fogDensity : 0f;
        smoothedFogDensity = Mathf.SmoothDamp(smoothedFogDensity, targetFogDensity, ref fogDensityVelocity, fogTransitionSeconds);

        RenderSettings.fog = smoothedFogDensity > 0.0001f;
        if (RenderSettings.fog) {
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = smoothedFogDensity;
        }
    }
}
