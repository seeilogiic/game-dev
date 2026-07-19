using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerUpgrades : MonoBehaviour
{
    public float speedIncrement = 0.5f;
    public float gatherIncrement = 0.5f;

    public int speedLevel;
    public int gatherLevel;

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
