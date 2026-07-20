using UnityEngine;
using System.Collections.Generic;

// Carried (not yet banked) resources. Gathering fills this up to maxTotalCarry, shared
// across all resource types; only a matching DropoffLocation moves carried amounts into
// ResourceCounter's banked tally.
public class PlayerInventory : MonoBehaviour
{
    [Tooltip("Max total resources (summed across all types) that can be carried at once.")]
    public int maxTotalCarry = 10;

    private readonly Dictionary<string, int> carried = new Dictionary<string, int>();

    public int GetCarried(string resourceType)
    {
        if (string.IsNullOrEmpty(resourceType)) {
            return 0;
        }

        return carried.TryGetValue(resourceType.ToLower(), out int amount) ? amount : 0;
    }

    public int GetTotalCarried()
    {
        int total = 0;
        foreach (int amount in carried.Values) {
            total += amount;
        }
        return total;
    }

    // Returns the amount actually added (may be less than requested if capacity was hit).
    public int Add(string resourceType, int amount)
    {
        if (string.IsNullOrEmpty(resourceType) || amount <= 0) {
            return 0;
        }

        string key = resourceType.ToLower();
        int room = Mathf.Max(0, maxTotalCarry - GetTotalCarried());
        int actualAdded = Mathf.Min(amount, room);

        if (actualAdded > 0) {
            carried[key] = GetCarried(key) + actualAdded;
        }

        return actualAdded;
    }

    public void Consume(string resourceType, int amount)
    {
        if (string.IsNullOrEmpty(resourceType)) {
            return;
        }

        string key = resourceType.ToLower();
        carried[key] = Mathf.Max(0, GetCarried(key) - amount);
    }
}
