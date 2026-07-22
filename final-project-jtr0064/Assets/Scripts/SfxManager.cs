using UnityEngine;

// Central one-shot SFX player. A single AudioSource plays every gameplay/UI sound effect via
// PlayOneShot, so multiple sounds can overlap without needing a dedicated source per event.
// Other scripts reach this through the static Instance (set in Awake) and call the matching
// Play*() method, e.g. SfxManager.Instance?.PlayGather() - the null-conditional means missing
// or not-yet-set-up audio is silently skipped rather than breaking gameplay.
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    public AudioSource source;

    [Header("Clips - assign whichever you have; missing ones are silently skipped")]
    public AudioClip gatherClip;
    public AudioClip depositClip;
    public AudioClip upgradeClip;
    public AudioClip wispHitClip;
    public AudioClip uiClickClip;

    [Range(0f, 1f)]
    public float volume = 0.5f;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    public void PlayGather() => Play(gatherClip);
    public void PlayDeposit() => Play(depositClip);
    public void PlayUpgrade() => Play(upgradeClip);
    public void PlayWispHit() => Play(wispHitClip);
    public void PlayUiClick() => Play(uiClickClip);

    private void Play(AudioClip clip)
    {
        if (source == null || clip == null) {
            return;
        }

        source.PlayOneShot(clip, volume);
    }
}
