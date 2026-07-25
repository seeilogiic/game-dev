using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerUpgrades : MonoBehaviour
{
    // All upgrades share the same 10-level cap and cost curve: 1, 2, 5, 10, 20, 30, 40,
    // 50, 75, 100 to go from level 0->1, 1->2, ... 9->10.
    public const int maxLevel = 10;

    [Header("Speed - Sprint reaches maxSprintSpeed at level 10; Move scales with the same ratio")]
    public float maxSprintSpeed = 30f;

    [Header("Gather Distance - reaches maxGatherRange at level 10")]
    public float maxGatherRange = 30f;

    [Header("Gather Speed - reaches maxGatherSpeedMultiplier at level 10")]
    public float maxGatherSpeedMultiplier = 10f;

    [Header("Auto-Collect - locked until level 1; range grows evenly from level 1 to level 9, then goes unlimited at level 10; cooldown shrinks evenly from level 1 to level 10")]
    public float autoCollectLevel1Range = 10f;
    public float autoCollectLevel9Range = 90f;
    public float autoCollectLevel1Cooldown = 30f;
    public float autoCollectLevel10Cooldown = 5f;

    [Header("Highlight - locked until level 1; cooldown shrinks and active duration grows evenly from level 1 to level 10; stays on permanently at level 10")]
    public float highlightLevel1Cooldown = 60f;
    public float highlightLevel10Cooldown = 10f;
    public float highlightLevel1Duration = 2f;
    public float highlightLevel10Duration = 10f;

    [Header("Inventory Capacity - reaches maxCarryCapacity at level 10")]
    public int maxCarryCapacity = 100;

    [Header("Auto-Dropoff - locked until level 1; deposits carried resources at any range; cooldown shrinks evenly from level 1 to level 10")]
    public float autoDropoffLevel1Cooldown = 30f;
    public float autoDropoffLevel10Cooldown = 5f;

    [Header("Dropoff Highlight - locked until level 1; cooldown shrinks and active duration grows evenly from level 1 to level 10; stays on permanently at level 10")]
    public float dropoffHighlightLevel1Cooldown = 60f;
    public float dropoffHighlightLevel10Cooldown = 10f;
    public float dropoffHighlightLevel1Duration = 2f;
    public float dropoffHighlightLevel10Duration = 10f;

    public int speedLevel;
    public int gatherLevel;
    public int gatherSpeedLevel;
    public int autoCollectLevel;
    public int highlightLevel;
    public int capacityLevel;
    public int autoDropoffLevel;
    public int dropoffHighlightLevel;

    private float baseMoveSpeed;
    private float baseSprintSpeed;
    private float baseGatherRange;
    private float baseGatherSpeedMultiplier;
    private int baseCarryCapacity;

    private ThirdPersonController controller;
    private PlayerInteraction interaction;
    private PlayerPoints points;
    private PlayerInventory inventory;
    private DayNightCycle dayNightCycle;
    private bool wasFoggy;

    void Start()
    {
        controller = GetComponent<ThirdPersonController>();
        interaction = GetComponent<PlayerInteraction>();
        points = GetComponent<PlayerPoints>();
        inventory = GetComponent<PlayerInventory>();
        dayNightCycle = FindFirstObjectByType<DayNightCycle>();

        // Capture whatever's set in the inspector as the level-0 baseline, then apply
        // levels 1..10 on top of that.
        if (controller != null) {
            baseMoveSpeed = controller.MoveSpeed;
            baseSprintSpeed = controller.SprintSpeed;
        }
        if (interaction != null) {
            baseGatherRange = interaction.interactionRange;
            baseGatherSpeedMultiplier = interaction.gatherSpeedMultiplier;
        }
        if (inventory != null) {
            baseCarryCapacity = inventory.maxTotalCarry;
        }

        ApplySpeed();
        ApplyGatherDistance();
        ApplyGatherSpeed();
        ApplyCapacity();
    }

    void Update()
    {
        // Fog halves move speed and gather range; re-apply whenever it toggles on/off
        // rather than every frame, since ApplySpeed/ApplyGatherDistance aren't otherwise
        // driven by Update.
        bool foggy = dayNightCycle != null && dayNightCycle.IsFoggy;
        if (foggy == wasFoggy) {
            return;
        }

        wasFoggy = foggy;
        ApplySpeed();
        ApplyGatherDistance();
    }

    private static readonly int[] upgradeCosts = { 1, 2, 5, 10, 20, 30, 40, 50, 75, 100 };

    // Cost to go from `level` to `level + 1`, indexed 0..9 for levels 0->1 ... 9->10.
    public static int GetUpgradeCost(int level)
    {
        return upgradeCosts[Mathf.Clamp(level, 0, upgradeCosts.Length - 1)];
    }

    public int GetNextSpeedCost() => GetUpgradeCost(speedLevel);
    public int GetNextGatherCost() => GetUpgradeCost(gatherLevel);
    public int GetNextGatherSpeedCost() => GetUpgradeCost(gatherSpeedLevel);
    public int GetNextAutoCollectCost() => GetUpgradeCost(autoCollectLevel);
    public int GetNextHighlightCost() => GetUpgradeCost(highlightLevel);
    public int GetNextCapacityCost() => GetUpgradeCost(capacityLevel);
    public int GetNextAutoDropoffCost() => GetUpgradeCost(autoDropoffLevel);
    public int GetNextDropoffHighlightCost() => GetUpgradeCost(dropoffHighlightLevel);

    public bool UpgradeSpeed()
    {
        if (controller == null || points == null || speedLevel >= maxLevel
            || !points.TrySpend(GetUpgradeCost(speedLevel))) {
            return false;
        }

        speedLevel++;
        ApplySpeed();
        return true;
    }

    public bool UpgradeGatherDistance()
    {
        if (interaction == null || points == null || gatherLevel >= maxLevel
            || !points.TrySpend(GetUpgradeCost(gatherLevel))) {
            return false;
        }

        gatherLevel++;
        ApplyGatherDistance();
        return true;
    }

    public bool UpgradeGatherSpeed()
    {
        if (interaction == null || points == null || gatherSpeedLevel >= maxLevel
            || !points.TrySpend(GetUpgradeCost(gatherSpeedLevel))) {
            return false;
        }

        gatherSpeedLevel++;
        ApplyGatherSpeed();
        return true;
    }

    public bool UpgradeAutoCollect()
    {
        if (points == null || autoCollectLevel >= maxLevel
            || !points.TrySpend(GetUpgradeCost(autoCollectLevel))) {
            return false;
        }

        autoCollectLevel++;
        return true;
    }

    public bool UpgradeHighlight()
    {
        if (points == null || highlightLevel >= maxLevel
            || !points.TrySpend(GetUpgradeCost(highlightLevel))) {
            return false;
        }

        highlightLevel++;
        return true;
    }

    public bool UpgradeCapacity()
    {
        if (inventory == null || points == null || capacityLevel >= maxLevel
            || !points.TrySpend(GetUpgradeCost(capacityLevel))) {
            return false;
        }

        capacityLevel++;
        ApplyCapacity();
        return true;
    }

    public bool UpgradeAutoDropoff()
    {
        if (points == null || autoDropoffLevel >= maxLevel
            || !points.TrySpend(GetUpgradeCost(autoDropoffLevel))) {
            return false;
        }

        autoDropoffLevel++;
        return true;
    }

    public bool UpgradeDropoffHighlight()
    {
        if (points == null || dropoffHighlightLevel >= maxLevel
            || !points.TrySpend(GetUpgradeCost(dropoffHighlightLevel))) {
            return false;
        }

        dropoffHighlightLevel++;
        return true;
    }

    private void ApplySpeed()
    {
        if (controller == null || baseSprintSpeed <= 0f) {
            return;
        }

        float fogMultiplier = (dayNightCycle != null && dayNightCycle.IsFoggy) ? 0.5f : 1f;
        float t = speedLevel / (float)maxLevel;
        controller.SprintSpeed = Mathf.Lerp(baseSprintSpeed, maxSprintSpeed, t) * fogMultiplier;
        controller.MoveSpeed = Mathf.Lerp(baseMoveSpeed, baseMoveSpeed * (maxSprintSpeed / baseSprintSpeed), t) * fogMultiplier;
    }

    private void ApplyGatherDistance()
    {
        if (interaction == null) {
            return;
        }

        float fogMultiplier = (dayNightCycle != null && dayNightCycle.IsFoggy) ? 0.5f : 1f;
        float t = gatherLevel / (float)maxLevel;
        interaction.interactionRange = Mathf.Lerp(baseGatherRange, maxGatherRange, t) * fogMultiplier;
    }

    private void ApplyGatherSpeed()
    {
        if (interaction == null) {
            return;
        }

        float t = gatherSpeedLevel / (float)maxLevel;
        interaction.gatherSpeedMultiplier = Mathf.Lerp(baseGatherSpeedMultiplier, maxGatherSpeedMultiplier, t);
    }

    private void ApplyCapacity()
    {
        if (inventory == null) {
            return;
        }

        float t = capacityLevel / (float)maxLevel;
        inventory.maxTotalCarry = Mathf.RoundToInt(Mathf.Lerp(baseCarryCapacity, maxCarryCapacity, t));
    }

    // 0 while locked; grows evenly from autoCollectLevel1Range (level 1) to
    // autoCollectLevel9Range (level 9); unlimited (Infinity) at level 10.
    public float GetAutoCollectRange()
    {
        if (autoCollectLevel <= 0) {
            return 0f;
        }
        if (autoCollectLevel >= maxLevel) {
            return Mathf.Infinity;
        }

        float t = (autoCollectLevel - 1) / (float)(maxLevel - 2);
        return Mathf.Lerp(autoCollectLevel1Range, autoCollectLevel9Range, t);
    }

    // Shrinks evenly from autoCollectLevel1Cooldown (level 1) to autoCollectLevel10Cooldown
    // (level 10).
    public float GetAutoCollectCooldown()
    {
        if (autoCollectLevel <= 0) {
            return autoCollectLevel1Cooldown;
        }

        float t = (autoCollectLevel - 1) / (float)(maxLevel - 1);
        return Mathf.Lerp(autoCollectLevel1Cooldown, autoCollectLevel10Cooldown, t);
    }

    // Shrinks evenly from highlightLevel1Cooldown (level 1) to highlightLevel10Cooldown
    // (level 10). Unused at level 10 since the highlight is simply left on permanently.
    public float GetHighlightCooldown()
    {
        if (highlightLevel <= 0) {
            return highlightLevel1Cooldown;
        }

        float t = (highlightLevel - 1) / (float)(maxLevel - 1);
        return Mathf.Lerp(highlightLevel1Cooldown, highlightLevel10Cooldown, t);
    }

    // Grows evenly from highlightLevel1Duration (level 1) to highlightLevel10Duration
    // (level 10); at level 10 PlayerAbilities ignores this and leaves the highlight on
    // permanently instead of timing it out.
    public float GetHighlightDuration()
    {
        if (highlightLevel <= 0) {
            return highlightLevel1Duration;
        }

        float t = (highlightLevel - 1) / (float)(maxLevel - 1);
        return Mathf.Lerp(highlightLevel1Duration, highlightLevel10Duration, t);
    }

    // Shrinks evenly from autoDropoffLevel1Cooldown (level 1) to autoDropoffLevel10Cooldown
    // (level 10). Range is unlimited at every unlocked level - only the cooldown improves.
    public float GetAutoDropoffCooldown()
    {
        if (autoDropoffLevel <= 0) {
            return autoDropoffLevel1Cooldown;
        }

        float t = (autoDropoffLevel - 1) / (float)(maxLevel - 1);
        return Mathf.Lerp(autoDropoffLevel1Cooldown, autoDropoffLevel10Cooldown, t);
    }

    // Shrinks evenly from dropoffHighlightLevel1Cooldown (level 1) to
    // dropoffHighlightLevel10Cooldown (level 10). Unused at level 10 since the highlight is
    // simply left on permanently.
    public float GetDropoffHighlightCooldown()
    {
        if (dropoffHighlightLevel <= 0) {
            return dropoffHighlightLevel1Cooldown;
        }

        float t = (dropoffHighlightLevel - 1) / (float)(maxLevel - 1);
        return Mathf.Lerp(dropoffHighlightLevel1Cooldown, dropoffHighlightLevel10Cooldown, t);
    }

    // Grows evenly from dropoffHighlightLevel1Duration (level 1) to
    // dropoffHighlightLevel10Duration (level 10); at level 10 PlayerAbilities ignores this and
    // leaves the highlight on permanently instead of timing it out.
    public float GetDropoffHighlightDuration()
    {
        if (dropoffHighlightLevel <= 0) {
            return dropoffHighlightLevel1Duration;
        }

        float t = (dropoffHighlightLevel - 1) / (float)(maxLevel - 1);
        return Mathf.Lerp(dropoffHighlightLevel1Duration, dropoffHighlightLevel10Duration, t);
    }

    public void OnToggleMenu(InputValue value)
    {
        if (!value.isPressed) {
            return;
        }

        UpgradeMenuUI menu = FindFirstObjectByType<UpgradeMenuUI>(FindObjectsInactive.Include);
        if (menu != null) {
            menu.Toggle();
        }
    }
}
