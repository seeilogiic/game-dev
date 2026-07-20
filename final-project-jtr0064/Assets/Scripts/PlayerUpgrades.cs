using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerUpgrades : MonoBehaviour
{
    // All upgrades share the same 10-level cap and cost curve: 1, 2, 4, 8, 16, 32, 64,
    // 128, 256, 512 to go from level 0->1, 1->2, ... 9->10.
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

    public int speedLevel;
    public int gatherLevel;
    public int gatherSpeedLevel;
    public int autoCollectLevel;

    private float baseMoveSpeed;
    private float baseSprintSpeed;
    private float baseGatherRange;
    private float baseGatherSpeedMultiplier;

    private ThirdPersonController controller;
    private PlayerInteraction interaction;
    private PlayerPoints points;

    void Start()
    {
        controller = GetComponent<ThirdPersonController>();
        interaction = GetComponent<PlayerInteraction>();
        points = GetComponent<PlayerPoints>();

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

        ApplySpeed();
        ApplyGatherDistance();
        ApplyGatherSpeed();
    }

    // Cost to go from `level` to `level + 1`: 2^level, i.e. 1, 2, 4, ... 512.
    public static int GetUpgradeCost(int level)
    {
        return 1 << level;
    }

    public int GetNextSpeedCost() => GetUpgradeCost(speedLevel);
    public int GetNextGatherCost() => GetUpgradeCost(gatherLevel);
    public int GetNextGatherSpeedCost() => GetUpgradeCost(gatherSpeedLevel);
    public int GetNextAutoCollectCost() => GetUpgradeCost(autoCollectLevel);

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

    private void ApplySpeed()
    {
        if (controller == null || baseSprintSpeed <= 0f) {
            return;
        }

        float t = speedLevel / (float)maxLevel;
        controller.SprintSpeed = Mathf.Lerp(baseSprintSpeed, maxSprintSpeed, t);
        controller.MoveSpeed = Mathf.Lerp(baseMoveSpeed, baseMoveSpeed * (maxSprintSpeed / baseSprintSpeed), t);
    }

    private void ApplyGatherDistance()
    {
        if (interaction == null) {
            return;
        }

        float t = gatherLevel / (float)maxLevel;
        interaction.interactionRange = Mathf.Lerp(baseGatherRange, maxGatherRange, t);
    }

    private void ApplyGatherSpeed()
    {
        if (interaction == null) {
            return;
        }

        float t = gatherSpeedLevel / (float)maxLevel;
        interaction.gatherSpeedMultiplier = Mathf.Lerp(baseGatherSpeedMultiplier, maxGatherSpeedMultiplier, t);
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
