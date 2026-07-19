using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Number-key abilities (1, 2, 3, 4...). Currently just Auto-Collect on "1", following the
// same Send Messages convention as PlayerInteraction.OnInteract / PlayerUpgrades.OnToggleMenu.
public class PlayerAbilities : MonoBehaviour
{
    [Tooltip("Seconds between Auto-Collect uses once unlocked.")]
    public float autoCollectCooldown = 30f;

    [Tooltip("Radial (Filled/Radial360) Image that sweeps down as Auto-Collect cools down. 0 = ready, 1 = just used.")]
    public Image autoCollectCooldownFill;
    [Tooltip("Shown while Auto-Collect hasn't been unlocked yet.")]
    public GameObject autoCollectLockedOverlay;

    private PlayerUpgrades playerUpgrades;
    private PlayerInteraction interaction;
    private float autoCollectReadyTime;

    void Start()
    {
        playerUpgrades = GetComponent<PlayerUpgrades>();
        interaction = GetComponent<PlayerInteraction>();

        if (autoCollectCooldownFill != null) {
            autoCollectCooldownFill.fillAmount = 0f;
            autoCollectCooldownFill.gameObject.SetActive(false);
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
            if (onCooldown) {
                autoCollectCooldownFill.fillAmount = Mathf.Clamp01(remaining / autoCollectCooldown);
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
            return;
        }

        string resourceName = target.resourceName;
        autoCollectReadyTime = Time.time + autoCollectCooldown;
        target.Interact();

        if (interaction != null) {
            interaction.ShowGatherPopup(resourceName);
        }
    }

    // Scene-wide version of PlayerInteraction.FindNearbyResource's nearest-match search,
    // without the interactionRange limit - Auto-Collect can reach anywhere on the map.
    private InteractableResource FindClosestResource()
    {
        InteractableResource[] resources = FindObjectsByType<InteractableResource>(FindObjectsSortMode.None);
        InteractableResource closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (InteractableResource resource in resources) {
            if (resource == null || !resource.isActiveAndEnabled || resource.usesRemaining <= 0) {
                continue;
            }

            float distance = Vector3.Distance(transform.position, resource.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closest = resource;
            }
        }

        return closest;
    }
}
