using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class UpgradeMenuUI : MonoBehaviour
{
    public GameObject panelRoot;
    public TextMeshProUGUI speedLabel;
    public TextMeshProUGUI gatherLabel;
    public Button upgradeSpeedButton;
    public Button upgradeGatherButton;

    public PlayerUpgrades playerUpgrades;
    public ThirdPersonController controller;
    public StarterAssetsInputs starterInputs;

    private bool isOpen;

    void Awake()
    {
        if (upgradeSpeedButton != null) {
            upgradeSpeedButton.onClick.AddListener(OnUpgradeSpeedClicked);
        }

        if (upgradeGatherButton != null) {
            upgradeGatherButton.onClick.AddListener(OnUpgradeGatherClicked);
        }

        if (panelRoot != null) {
            panelRoot.SetActive(false);
        }
    }

    public void Toggle()
    {
        isOpen = !isOpen;

        if (panelRoot != null) {
            panelRoot.SetActive(isOpen);
        }

        if (controller != null) {
            controller.enabled = !isOpen;
        }

        if (starterInputs != null) {
            starterInputs.enabled = !isOpen;
        }

        if (isOpen) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            RefreshUI();
        } else {
            if (starterInputs != null) {
                starterInputs.cursorLocked = true;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnUpgradeSpeedClicked()
    {
        if (playerUpgrades == null) {
            return;
        }

        playerUpgrades.UpgradeSpeed();
        RefreshUI();
    }

    private void OnUpgradeGatherClicked()
    {
        if (playerUpgrades == null) {
            return;
        }

        playerUpgrades.UpgradeGatherDistance();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (playerUpgrades == null || controller == null) {
            return;
        }

        if (speedLabel != null) {
            speedLabel.text = "Speed Lv. " + playerUpgrades.speedLevel +
                "  (Move " + controller.MoveSpeed.ToString("F1") +
                " / Sprint " + controller.SprintSpeed.ToString("F1") + ")";
        }

        if (gatherLabel != null) {
            PlayerInteraction interaction = playerUpgrades.GetComponent<PlayerInteraction>();
            float range = interaction != null ? interaction.interactionRange : 0f;
            gatherLabel.text = "Gather Distance Lv. " + playerUpgrades.gatherLevel +
                "  (" + range.ToString("F1") + "m)";
        }
    }
}
