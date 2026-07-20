using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class UpgradeMenuUI : MonoBehaviour
{
    public GameObject panelRoot;
    public TextMeshProUGUI pointsLabel;
    public TextMeshProUGUI speedLabel;
    public TextMeshProUGUI gatherLabel;
    public TextMeshProUGUI gatherSpeedLabel;
    public TextMeshProUGUI autoCollectLabel;
    public Button upgradeSpeedButton;
    public Button upgradeGatherButton;
    public Button upgradeGatherSpeedButton;
    public Button unlockAutoCollectButton;

    public PlayerUpgrades playerUpgrades;
    public PlayerPoints playerPoints;
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

        if (upgradeGatherSpeedButton != null) {
            upgradeGatherSpeedButton.onClick.AddListener(OnUpgradeGatherSpeedClicked);
        }

        if (unlockAutoCollectButton != null) {
            unlockAutoCollectButton.onClick.AddListener(OnUnlockAutoCollectClicked);
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

    private void OnUpgradeGatherSpeedClicked()
    {
        if (playerUpgrades == null) {
            return;
        }

        playerUpgrades.UpgradeGatherSpeed();
        RefreshUI();
    }

    private void OnUnlockAutoCollectClicked()
    {
        if (playerUpgrades == null) {
            return;
        }

        playerUpgrades.UpgradeAutoCollect();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (playerUpgrades == null || controller == null) {
            return;
        }

        int points = playerPoints != null ? playerPoints.points : 0;

        if (pointsLabel != null) {
            pointsLabel.text = "Points: " + points;
        }

        int maxLevel = PlayerUpgrades.maxLevel;

        bool speedMaxed = playerUpgrades.speedLevel >= maxLevel;
        int speedCost = playerUpgrades.GetNextSpeedCost();
        if (speedLabel != null) {
            speedLabel.text = "Speed Lv. " + playerUpgrades.speedLevel + "/" + maxLevel +
                "  (Move " + controller.MoveSpeed.ToString("F1") +
                " / Sprint " + controller.SprintSpeed.ToString("F1") + ")" +
                (speedMaxed ? "  - MAX" : "  - " + speedCost + " pts");
        }

        if (upgradeSpeedButton != null) {
            upgradeSpeedButton.interactable = !speedMaxed && points >= speedCost;
        }

        PlayerInteraction interaction = playerUpgrades.GetComponent<PlayerInteraction>();

        bool gatherMaxed = playerUpgrades.gatherLevel >= maxLevel;
        int gatherCost = playerUpgrades.GetNextGatherCost();
        if (gatherLabel != null) {
            float range = interaction != null ? interaction.interactionRange : 0f;
            gatherLabel.text = "Gather Distance Lv. " + playerUpgrades.gatherLevel + "/" + maxLevel +
                "  (" + range.ToString("F1") + "m)" +
                (gatherMaxed ? "  - MAX" : "  - " + gatherCost + " pts");
        }

        if (upgradeGatherButton != null) {
            upgradeGatherButton.interactable = !gatherMaxed && points >= gatherCost;
        }

        bool gatherSpeedMaxed = playerUpgrades.gatherSpeedLevel >= maxLevel;
        int gatherSpeedCost = playerUpgrades.GetNextGatherSpeedCost();
        if (gatherSpeedLabel != null) {
            float multiplier = interaction != null ? interaction.gatherSpeedMultiplier : 1f;
            gatherSpeedLabel.text = "Gather Speed Lv. " + playerUpgrades.gatherSpeedLevel + "/" + maxLevel +
                "  (" + multiplier.ToString("F2") + "x)" +
                (gatherSpeedMaxed ? "  - MAX" : "  - " + gatherSpeedCost + " pts");
        }

        if (upgradeGatherSpeedButton != null) {
            upgradeGatherSpeedButton.interactable = !gatherSpeedMaxed && points >= gatherSpeedCost;
        }

        bool autoCollectUnlocked = playerUpgrades.autoCollectLevel > 0;
        bool autoCollectMaxed = playerUpgrades.autoCollectLevel >= maxLevel;
        int autoCollectCost = playerUpgrades.GetNextAutoCollectCost();

        if (autoCollectLabel != null) {
            string status;
            if (!autoCollectUnlocked) {
                status = "Locked";
            } else if (autoCollectMaxed) {
                status = "unlimited range, " + playerUpgrades.GetAutoCollectCooldown().ToString("F0") + "s cooldown";
            } else {
                status = playerUpgrades.GetAutoCollectRange().ToString("F0") + "m range, " +
                    playerUpgrades.GetAutoCollectCooldown().ToString("F0") + "s cooldown";
            }

            autoCollectLabel.text = "Auto-Collect Lv. " + playerUpgrades.autoCollectLevel + "/" + maxLevel +
                "  (" + status + ")" +
                (autoCollectMaxed ? "  - MAX" : "  - " + autoCollectCost + " pts");
        }

        if (unlockAutoCollectButton != null) {
            unlockAutoCollectButton.interactable = !autoCollectMaxed && points >= autoCollectCost;
        }
    }
}
