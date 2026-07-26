using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Number-key abilities (1, 2, 3, 4...). Auto-Collect on "1", Highlight on "2", Auto-Dropoff on
// "3", Dropoff Highlight on "4", following the same Send Messages convention as
// PlayerInteraction.OnInteract / PlayerUpgrades.OnToggleMenu.
public class PlayerAbilities : MonoBehaviour
{
    [Tooltip("Radial (Filled/Radial360) Image that sweeps down as Auto-Collect cools down. 0 = ready, 1 = just used.")]
    public Image autoCollectCooldownFill;
    [Tooltip("Shown while Auto-Collect hasn't been unlocked yet.")]
    public GameObject autoCollectLockedOverlay;

    [Tooltip("Radial (Filled/Radial360) Image that sweeps down as Highlight cools down. 0 = ready, 1 = just used.")]
    public Image highlightCooldownFill;
    [Tooltip("Shown while Highlight hasn't been unlocked yet.")]
    public GameObject highlightLockedOverlay;

    [Tooltip("Radial (Filled/Radial360) Image that sweeps down as Auto-Dropoff cools down. 0 = ready, 1 = just used.")]
    public Image autoDropoffCooldownFill;
    [Tooltip("Shown while Auto-Dropoff hasn't been unlocked yet.")]
    public GameObject autoDropoffLockedOverlay;

    [Tooltip("Radial (Filled/Radial360) Image that sweeps down as Dropoff Highlight cools down. 0 = ready, 1 = just used.")]
    public Image dropoffHighlightCooldownFill;
    [Tooltip("Shown while Dropoff Highlight hasn't been unlocked yet.")]
    public GameObject dropoffHighlightLockedOverlay;

    private PlayerUpgrades playerUpgrades;
    private PlayerInteraction interaction;
    private PlayerInventory inventory;
    private float autoCollectReadyTime;
    // Cooldown duration in effect for the current/last cooldown window, snapshotted at use
    // time so the fill radial doesn't jump around if the player levels up mid-cooldown.
    private float currentCooldownDuration;

    private float highlightReadyTime;
    private float currentHighlightCooldown;
    private float highlightActiveUntil;
    private bool highlightPermanentlyOn;
    private float nextPermanentHighlightScan;
    private readonly List<GameObject> activeHighlightRings = new List<GameObject>();

    private float autoDropoffReadyTime;
    private float currentAutoDropoffCooldownDuration;

    private float dropoffHighlightReadyTime;
    private float currentDropoffHighlightCooldown;
    private float dropoffHighlightActiveUntil;
    private bool dropoffHighlightPermanentlyOn;
    private float nextPermanentDropoffHighlightScan;
    private readonly List<GameObject> activeDropoffHighlightRings = new List<GameObject>();

    void Start()
    {
        playerUpgrades = GetComponent<PlayerUpgrades>();
        interaction = GetComponent<PlayerInteraction>();
        inventory = GetComponent<PlayerInventory>();

        if (autoCollectCooldownFill != null) {
            autoCollectCooldownFill.fillAmount = 0f;
            autoCollectCooldownFill.gameObject.SetActive(false);
        }

        if (highlightCooldownFill != null) {
            highlightCooldownFill.fillAmount = 0f;
            highlightCooldownFill.gameObject.SetActive(false);
        }

        if (autoDropoffCooldownFill != null) {
            autoDropoffCooldownFill.fillAmount = 0f;
            autoDropoffCooldownFill.gameObject.SetActive(false);
        }

        if (dropoffHighlightCooldownFill != null) {
            dropoffHighlightCooldownFill.fillAmount = 0f;
            dropoffHighlightCooldownFill.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        bool unlocked = playerUpgrades != null && playerUpgrades.autoCollectLevel > 0;

        if (autoCollectLockedOverlay != null) {
            autoCollectLockedOverlay.SetActive(!unlocked);
        }

        if (autoCollectCooldownFill != null) {
            float remaining = Mathf.Max(0f, autoCollectReadyTime - Time.time);
            bool onCooldown = unlocked && remaining > 0f;

            autoCollectCooldownFill.gameObject.SetActive(onCooldown);
            if (onCooldown && currentCooldownDuration > 0f) {
                autoCollectCooldownFill.fillAmount = Mathf.Clamp01(remaining / currentCooldownDuration);
            }
        }

        bool autoDropoffUnlocked = playerUpgrades != null && playerUpgrades.autoDropoffLevel > 0;

        if (autoDropoffLockedOverlay != null) {
            autoDropoffLockedOverlay.SetActive(!autoDropoffUnlocked);
        }

        if (autoDropoffCooldownFill != null) {
            float remaining = Mathf.Max(0f, autoDropoffReadyTime - Time.time);
            bool onCooldown = autoDropoffUnlocked && remaining > 0f;

            autoDropoffCooldownFill.gameObject.SetActive(onCooldown);
            if (onCooldown && currentAutoDropoffCooldownDuration > 0f) {
                autoDropoffCooldownFill.fillAmount = Mathf.Clamp01(remaining / currentAutoDropoffCooldownDuration);
            }
        }

        UpdateHighlight();
        UpdateDropoffHighlight();
    }

    private void UpdateHighlight()
    {
        bool highlightUnlocked = playerUpgrades != null && playerUpgrades.highlightLevel > 0;
        bool highlightMaxed = playerUpgrades != null && playerUpgrades.highlightLevel >= PlayerUpgrades.maxLevel;

        if (highlightLockedOverlay != null) {
            highlightLockedOverlay.SetActive(!highlightUnlocked);
        }

        if (highlightUnlocked && highlightMaxed) {
            // Level 10: the highlight is simply left on forever - no key press, cooldown, or
            // duration needed anymore.
            if (!highlightPermanentlyOn) {
                highlightPermanentlyOn = true;
                ClearHighlights();
            }

            if (Time.time >= nextPermanentHighlightScan) {
                RefreshPermanentHighlights();
                nextPermanentHighlightScan = Time.time + 1f;
            }
        } else {
            if (highlightPermanentlyOn) {
                highlightPermanentlyOn = false;
                ClearHighlights();
            }

            if (highlightActiveUntil > 0f && Time.time >= highlightActiveUntil) {
                highlightActiveUntil = 0f;
                ClearHighlights();
            }
        }

        if (highlightCooldownFill != null) {
            float remaining = Mathf.Max(0f, highlightReadyTime - Time.time);
            bool onCooldown = highlightUnlocked && !highlightMaxed && remaining > 0f;

            highlightCooldownFill.gameObject.SetActive(onCooldown);
            if (onCooldown && currentHighlightCooldown > 0f) {
                highlightCooldownFill.fillAmount = Mathf.Clamp01(remaining / currentHighlightCooldown);
            }
        }
    }

    private void UpdateDropoffHighlight()
    {
        bool dropoffHighlightUnlocked = playerUpgrades != null && playerUpgrades.dropoffHighlightLevel > 0;
        bool dropoffHighlightMaxed = playerUpgrades != null && playerUpgrades.dropoffHighlightLevel >= PlayerUpgrades.maxLevel;

        if (dropoffHighlightLockedOverlay != null) {
            dropoffHighlightLockedOverlay.SetActive(!dropoffHighlightUnlocked);
        }

        if (dropoffHighlightUnlocked && dropoffHighlightMaxed) {
            // Level 10: the highlight is simply left on forever - no key press, cooldown, or
            // duration needed anymore.
            if (!dropoffHighlightPermanentlyOn) {
                dropoffHighlightPermanentlyOn = true;
                ClearDropoffHighlights();
            }

            if (Time.time >= nextPermanentDropoffHighlightScan) {
                RefreshPermanentDropoffHighlights();
                nextPermanentDropoffHighlightScan = Time.time + 1f;
            }
        } else {
            if (dropoffHighlightPermanentlyOn) {
                dropoffHighlightPermanentlyOn = false;
                ClearDropoffHighlights();
            }

            if (dropoffHighlightActiveUntil > 0f && Time.time >= dropoffHighlightActiveUntil) {
                dropoffHighlightActiveUntil = 0f;
                ClearDropoffHighlights();
            }
        }

        if (dropoffHighlightCooldownFill != null) {
            float remaining = Mathf.Max(0f, dropoffHighlightReadyTime - Time.time);
            bool onCooldown = dropoffHighlightUnlocked && !dropoffHighlightMaxed && remaining > 0f;

            dropoffHighlightCooldownFill.gameObject.SetActive(onCooldown);
            if (onCooldown && currentDropoffHighlightCooldown > 0f) {
                dropoffHighlightCooldownFill.fillAmount = Mathf.Clamp01(remaining / currentDropoffHighlightCooldown);
            }
        }
    }

    public void OnAbilityOne(InputValue value)
    {
        if (!value.isPressed) {
            return;
        }

        if (playerUpgrades == null || playerUpgrades.autoCollectLevel <= 0) {
            return;
        }

        if (Time.time < autoCollectReadyTime) {
            return;
        }

        InteractableResource target = FindClosestResource();
        if (target == null) {
            if (interaction != null) {
                interaction.ShowMessagePopup("No resources in range");
            }
            return;
        }

        string resourceName = target.resourceName;
        bool gathered = target.Interact(inventory);
        if (!gathered) {
            return;
        }

        currentCooldownDuration = playerUpgrades.GetAutoCollectCooldown();
        autoCollectReadyTime = Time.time + currentCooldownDuration;

        if (interaction != null) {
            interaction.ShowGatherPopup(resourceName);
        }
        SfxManager.Instance?.PlayGather();
    }

    // Scene-wide version of PlayerInteraction.FindNearbyResource's nearest-match search,
    // capped to the current Auto-Collect range (unlimited once that hits level 10).
    private InteractableResource FindClosestResource()
    {
        float range = playerUpgrades.GetAutoCollectRange();
        InteractableResource[] resources = FindObjectsByType<InteractableResource>(FindObjectsSortMode.None);
        InteractableResource closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (InteractableResource resource in resources) {
            if (resource == null || !resource.isActiveAndEnabled || resource.usesRemaining <= 0) {
                continue;
            }

            float distance = Vector3.Distance(transform.position, resource.transform.position);
            if (distance <= range && distance < closestDistance) {
                closestDistance = distance;
                closest = resource;
            }
        }

        return closest;
    }

    public void OnAbilityTwo(InputValue value)
    {
        if (!value.isPressed) {
            return;
        }

        if (playerUpgrades == null || playerUpgrades.highlightLevel <= 0) {
            return;
        }

        // Already on permanently at level 10 - nothing left for the key press to do.
        if (playerUpgrades.highlightLevel >= PlayerUpgrades.maxLevel) {
            return;
        }

        if (Time.time < highlightReadyTime) {
            return;
        }

        SpawnHighlights();
        highlightActiveUntil = Time.time + playerUpgrades.GetHighlightDuration();

        currentHighlightCooldown = playerUpgrades.GetHighlightCooldown();
        highlightReadyTime = Time.time + currentHighlightCooldown;
    }

    private void SpawnHighlights()
    {
        ClearHighlights();

        InteractableResource[] resources = FindObjectsByType<InteractableResource>(FindObjectsSortMode.None);
        foreach (InteractableResource resource in resources) {
            if (resource == null || !resource.isActiveAndEnabled || resource.usesRemaining <= 0) {
                continue;
            }

            HighlightRing ring = HighlightRing.Attach(resource.transform, HighlightRing.GoldColor);
            activeHighlightRings.Add(ring.gameObject);
        }
    }

    // Level 10 only: adds rings to any gatherable resource that doesn't already have one
    // (e.g. one that only just became active), rather than tearing down and rebuilding
    // every resource's ring every second.
    private void RefreshPermanentHighlights()
    {
        InteractableResource[] resources = FindObjectsByType<InteractableResource>(FindObjectsSortMode.None);
        foreach (InteractableResource resource in resources) {
            if (resource == null || !resource.isActiveAndEnabled || resource.usesRemaining <= 0) {
                continue;
            }

            if (resource.GetComponentInChildren<HighlightRing>() != null) {
                continue;
            }

            HighlightRing ring = HighlightRing.Attach(resource.transform, HighlightRing.GoldColor);
            activeHighlightRings.Add(ring.gameObject);
        }
    }

    private void ClearHighlights()
    {
        foreach (GameObject ring in activeHighlightRings) {
            if (ring != null) {
                Destroy(ring);
            }
        }

        activeHighlightRings.Clear();
    }

    public void OnAbilityThree(InputValue value)
    {
        if (!value.isPressed) {
            return;
        }

        if (playerUpgrades == null || playerUpgrades.autoDropoffLevel <= 0) {
            return;
        }

        if (Time.time < autoDropoffReadyTime) {
            return;
        }

        if (inventory == null || inventory.GetTotalCarried() <= 0) {
            if (interaction != null) {
                interaction.ShowMessagePopup("Nothing to drop off");
            }
            return;
        }

        // Unlimited distance by design - unlike Auto-Collect, this doesn't scale a range with
        // level, it just deposits into every matching dropoff in the scene regardless of
        // proximity. DropoffLocation.Deposit already plays the deposit SFX per type.
        DropoffLocation[] dropoffs = FindObjectsByType<DropoffLocation>(FindObjectsSortMode.None);
        int totalDeposited = 0;
        foreach (DropoffLocation dropoff in dropoffs) {
            if (dropoff == null) {
                continue;
            }

            totalDeposited += dropoff.Deposit(inventory);
        }

        if (totalDeposited <= 0) {
            return;
        }

        currentAutoDropoffCooldownDuration = playerUpgrades.GetAutoDropoffCooldown();
        autoDropoffReadyTime = Time.time + currentAutoDropoffCooldownDuration;
    }

    public void OnAbilityFour(InputValue value)
    {
        if (!value.isPressed) {
            return;
        }

        if (playerUpgrades == null || playerUpgrades.dropoffHighlightLevel <= 0) {
            return;
        }

        // Already on permanently at level 10 - nothing left for the key press to do.
        if (playerUpgrades.dropoffHighlightLevel >= PlayerUpgrades.maxLevel) {
            return;
        }

        if (Time.time < dropoffHighlightReadyTime) {
            return;
        }

        SpawnDropoffHighlights();
        dropoffHighlightActiveUntil = Time.time + playerUpgrades.GetDropoffHighlightDuration();

        currentDropoffHighlightCooldown = playerUpgrades.GetDropoffHighlightCooldown();
        dropoffHighlightReadyTime = Time.time + currentDropoffHighlightCooldown;
    }

    private void SpawnDropoffHighlights()
    {
        ClearDropoffHighlights();

        DropoffLocation[] dropoffs = FindObjectsByType<DropoffLocation>(FindObjectsSortMode.None);
        foreach (DropoffLocation dropoff in dropoffs) {
            if (dropoff == null) {
                continue;
            }

            HighlightRing ring = HighlightRing.Attach(dropoff.transform, HighlightRing.LightBlueColor);
            activeDropoffHighlightRings.Add(ring.gameObject);
        }
    }

    // Level 10 only: adds rings to any dropoff that doesn't already have one, rather than
    // tearing down and rebuilding every dropoff's ring every second.
    private void RefreshPermanentDropoffHighlights()
    {
        DropoffLocation[] dropoffs = FindObjectsByType<DropoffLocation>(FindObjectsSortMode.None);
        foreach (DropoffLocation dropoff in dropoffs) {
            if (dropoff == null) {
                continue;
            }

            if (dropoff.GetComponentInChildren<HighlightRing>() != null) {
                continue;
            }

            HighlightRing ring = HighlightRing.Attach(dropoff.transform, HighlightRing.LightBlueColor);
            activeDropoffHighlightRings.Add(ring.gameObject);
        }
    }

    private void ClearDropoffHighlights()
    {
        foreach (GameObject ring in activeDropoffHighlightRings) {
            if (ring != null) {
                Destroy(ring);
            }
        }

        activeDropoffHighlightRings.Clear();
    }
}
