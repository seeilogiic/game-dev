using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

// Shown automatically at scene start (its GameObject starts active). Freezes the player the
// same way UpgradeMenuUI does while open, and also disables PlayerInput itself so E/M can't
// fire underneath it. Enter is polled directly via Keyboard.current, same convention as
// CameraZoom's Z-key handling, since no "Enter" action exists in StarterAssets.inputactions.
public class IntroScreenUI : MonoBehaviour
{
    public ThirdPersonController controller;
    public StarterAssetsInputs starterInputs;
    public PlayerInput playerInput;

    void Awake()
    {
        SetPlayerControlActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null) {
            return;
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame) {
            Dismiss();
        }
    }

    private void Dismiss()
    {
        gameObject.SetActive(false);
        SetPlayerControlActive(true);
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
