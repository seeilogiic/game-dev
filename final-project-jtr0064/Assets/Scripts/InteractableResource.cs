using UnityEngine;

public class InteractableResource : MonoBehaviour
{
    public string resourceName = "Apple";
    public int amountPerCollect = 1;
    public int usesRemaining = 1;

    public string promptText = "Press E to interact";
    public string animationTrigger = "PickFruit";

    public bool destroyWhenEmpty = true;

    // Adds this resource to the player's carried PlayerInventory (banking happens later, at
    // a matching DropoffLocation). Returns false without consuming a use if the inventory has
    // no room left for this type, so the resource stays gatherable until the player deposits.
    public bool Interact(PlayerInventory inventory)
    {
        if (usesRemaining <= 0) {
            return false;
        }

        int added = inventory != null ? inventory.Add(resourceName, amountPerCollect) : amountPerCollect;
        if (added <= 0) {
            return false;
        }

        usesRemaining--;

        if (usesRemaining <= 0 && destroyWhenEmpty) {
            gameObject.SetActive(false);
        }

        return true;
    }
}
