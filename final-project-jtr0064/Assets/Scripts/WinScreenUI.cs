using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using StarterAssets;

// Shown once ResourceCounter detects every resource has been collected and deposited. Its
// GameObject starts inactive (set by WinScreenSetupTool) and Show() is the only thing that
// ever activates it, so Awake (button listener wiring) always runs on that first activation -
// unlike IntroScreenUI, this panel must never fail to appear since it only fires once.
// Freezes the player the same way IntroScreenUI/UpgradeMenuUI do while open. The Restart
// button reloads the active scene, which resets all game state and brings back IntroScreenUI
// (active by default at scene start).
public class WinScreenUI : MonoBehaviour
{
    public Button restartButton;
    public ThirdPersonController controller;
    public StarterAssetsInputs starterInputs;
    public PlayerInput playerInput;

    void Awake()
    {
        if (restartButton != null) {
            restartButton.onClick.AddListener(Restart);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetPlayerControlActive(false);
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SetPlayerControlActive(bool active)
    {
        if (controller != null) {
            controller.enabled = active;
        }

        if (playerInput != null) {
            playerInput.enabled = active;
        }

        if (starterInputs != null) {
            starterInputs.enabled = active;
        }

        if (active) {
            if (starterInputs != null) {
                starterInputs.cursorLocked = true;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
