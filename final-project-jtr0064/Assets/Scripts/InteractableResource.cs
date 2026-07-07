using UnityEngine;

public class InteractableResource : MonoBehaviour
{
    public string resourceName = "Apple";
    public int amountPerCollect = 1;
    public int usesRemaining = 1;

    public string promptText = "Press E to interact";
    public string animationTrigger = "PickFruit";

    public bool destroyWhenEmpty = true;

    private ResourceCounter resourceCounter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resourceCounter = FindObjectOfType<ResourceCounter>();
    }

    public void Interact()
    {
        if (usesRemaining <= 0) {
            return;
        }

        if (resourceCounter != null) {
            resourceCounter.AddResource(resourceName, amountPerCollect);
        }

        usesRemaining--;

        if (usesRemaining <= 0 && destroyWhenEmpty) {
            gameObject.SetActive(false);
        }
    }
}
