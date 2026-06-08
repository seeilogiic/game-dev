using UnityEngine;
using UnityEngine.UI; // Required for dealing with UI Images

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth; // Drag your Player object here
    public Image[] heartImages;       // Drag your 5 Heart Images here

    void Update()
    {
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        // Loop through all 5 heart slots
        for (int i = 0; i < heartImages.Length; i++)
        {
            // If the current slot index is less than the player's health, turn it on
            if (i < playerHealth.currentHealth)
            {
                heartImages[i].enabled = true;
            }
            // Otherwise, turn it off
            else
            {
                heartImages[i].enabled = false;
            }
        }
    }
}