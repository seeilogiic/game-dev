using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{

    public float interactionRange = 3f;
    public TextMeshProUGUI promptText;
    
    private InteractableResource currentResource;
    private Animator animator;
    private bool isInteracting;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        if (promptText != null) {
            promptText.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        FindNearbyResource();
    }

    private void FindNearbyResource()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange);
        InteractableResource closestResource = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            InteractableResource resource = hit.GetComponentInParent<InteractableResource>();
            if (resource == null) {
                continue;
            }

            float distance = Vector3.Distance(transform.position, resource.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestResource = resource;
            }
        }
    
        currentResource = closestResource;

        if (promptText == null) {
            return;
        }

        if (currentResource != null && !isInteracting) {
            promptText.text = "Press E to interact";
            promptText.gameObject.SetActive(true);
        } else {
            promptText.gameObject.SetActive(false);
        }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) {
            return;
        }
        
        if (currentResource == null || isInteracting) {
            return;
        }

        StartCoroutine(InteractRoutine());
    }

    private IEnumerator InteractRoutine()
    {
        isInteracting = true;

        if (promptText != null) {
            promptText.gameObject.SetActive(false);
        }

        if (animator != null && !string.IsNullOrEmpty(currentResource.animationTrigger)) {
            animator.SetTrigger(currentResource.animationTrigger);
        }

        yield return new WaitForSeconds(0.8f);

        if (currentResource != null) {
            currentResource.Interact();
        }

        yield return new WaitForSeconds(0.3f);
        isInteracting = false;
    }
}
