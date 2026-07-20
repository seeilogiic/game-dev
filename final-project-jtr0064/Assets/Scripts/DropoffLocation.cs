using UnityEngine;

// A type-specific bank point: depositing here moves a matching resource type out of the
// player's carried PlayerInventory and into ResourceCounter's banked tally, awarding 1
// PlayerPoints point per resource deposited.
public class DropoffLocation : MonoBehaviour
{
    [Tooltip("Resource type this dropoff accepts, matched like ResourceCounter (\"apple\", \"ore\", \"poppy\").")]
    public string acceptedResourceType = "ore";

    private ResourceCounter resourceCounter;

    void Start()
    {
        resourceCounter = FindObjectOfType<ResourceCounter>();
    }

    // Deposits everything of acceptedResourceType currently carried. Returns the amount
    // deposited (0 if the player wasn't carrying any of this type).
    public int Deposit(PlayerInventory inventory)
    {
        if (inventory == null) {
            return 0;
        }

        int amount = inventory.GetCarried(acceptedResourceType);
        if (amount <= 0) {
            return 0;
        }

        if (resourceCounter != null) {
            resourceCounter.AddResource(acceptedResourceType, amount);
        }

        PlayerPoints points = inventory.GetComponent<PlayerPoints>();
        if (points != null) {
            points.AddPoints(amount);
        }

        inventory.Consume(acceptedResourceType, amount);

        return amount;
    }
}
