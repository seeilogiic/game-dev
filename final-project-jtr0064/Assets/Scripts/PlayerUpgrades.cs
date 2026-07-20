using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerUpgrades : MonoBehaviour
{
    public const int upgradeCost = 10;

    public float speedIncrement = 0.5f;
    public float gatherIncrement = 0.5f;
    public float gatherSpeedIncrement = 0.25f;

    public int speedLevel;
    public int gatherLevel;
    public int gatherSpeedLevel;

    // 0 = locked, 1 = unlocked. Auto-Collect has no further levels yet.
    public int autoCollectLevel;

    private ThirdPersonController controller;
    private PlayerInteraction interaction;
    private PlayerPoints points;

    void Start()
    {
        controller = GetComponent<ThirdPersonController>();
        interaction = GetComponent<PlayerInteraction>();
        points = GetComponent<PlayerPoints>();
    }

    public bool UpgradeSpeed()
    {
        if (controller == null || points == null || !points.TrySpend(upgradeCost)) {
            return false;
        }

        controller.MoveSpeed += speedIncrement;
        controller.SprintSpeed += speedIncrement;
        speedLevel++;
        return true;
    }

    public bool UpgradeGatherDistance()
    {
        if (interaction == null || points == null || !points.TrySpend(upgradeCost)) {
            return false;
        }

        interaction.interactionRange += gatherIncrement;
        gatherLevel++;
        return true;
    }

    public bool UpgradeGatherSpeed()
    {
        if (interaction == null || points == null || !points.TrySpend(upgradeCost)) {
            return false;
        }

        interaction.gatherSpeedMultiplier += gatherSpeedIncrement;
        gatherSpeedLevel++;
        return true;
    }

    public bool UnlockAutoCollect()
    {
        if (autoCollectLevel >= 1 || points == null || !points.TrySpend(upgradeCost)) {
            return false;
        }

        autoCollectLevel = 1;
        return true;
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
