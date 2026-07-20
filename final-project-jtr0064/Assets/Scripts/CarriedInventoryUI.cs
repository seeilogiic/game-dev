using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

// Shows what the player is currently carrying (not yet banked), e.g. "Carrying: Ore x3, Poppy x1".
public class CarriedInventoryUI : MonoBehaviour
{
    public PlayerInventory inventory;
    public TextMeshProUGUI carriedText;
    public string[] displayedResourceTypes = new string[] { "apple", "ore", "poppy" };

    void Update()
    {
        if (inventory == null || carriedText == null) {
            return;
        }

        StringBuilder builder = new StringBuilder();
        foreach (string type in displayedResourceTypes) {
            int amount = inventory.GetCarried(type);
            if (amount <= 0) {
                continue;
            }

            if (builder.Length > 0) {
                builder.Append(", ");
            }
            builder.Append(Capitalize(type)).Append(" x").Append(amount);
        }

        carriedText.text = builder.Length > 0 ? "Carrying: " + builder : "";
    }

    private static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) {
            return s;
        }
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}
