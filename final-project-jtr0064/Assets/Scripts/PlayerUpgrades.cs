using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerUpgrades : MonoBehaviour
{
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

    void Start()
    {
        controller = GetComponent<ThirdPersonController>();
        interaction = GetComponent<PlayerInteraction>();
    }

    public void UpgradeSpeed()
    {
        if (controller == null) {
            return;
        }

        controller.MoveSpeed += speedIncrement;
        controller.SprintSpeed += speedIncrement;
        speedLevel++;
    }

    public void UpgradeGatherDistance()
    {
        if (interaction == null) {
            return;
        }

        interaction.interactionRange += gatherIncrement;
        gatherLevel++;
    }

    public void UpgradeGatherSpeed()
    {
        if (interaction == null) {
            return;
        }

        interaction.gatherSpeedMultiplier += gatherSpeedIncrement;
        gatherSpeedLevel++;
    }

    public void UnlockAutoCollect()
    {
        if (autoCollectLevel >= 1) {
            return;
        }

        autoCollectLevel = 1;
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
