using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{

    public float interactionRange = 3f;
    [Tooltip("Multiplies the gather animation's playback speed. 1 = normal speed.")]
    public float gatherSpeedMultiplier = 1f;
    public TextMeshProUGUI promptText;
    [Tooltip("Container holding the prompt's keycap + label. When assigned, this is what gets shown/hidden instead of promptText.gameObject directly.")]
    [SerializeField] private GameObject promptRoot;
    [Tooltip("Radial (Filled/Radial360) Image that visualizes gather progress, 0 to 1.")]
    public Image gatherProgressImage;
    [Tooltip("Shows a brief 'Gathered {item}' message after any gather, manual or Auto-Collect.")]
    public GatherPopupUI gatherPopup;

    [Tooltip("Safety cap while waiting for the animator to enter the pickup/gather state, in case the trigger name doesn't match any state.")]
    [SerializeField] private float enterStateTimeout = 2f;
    [Tooltip("Used only when there is no animator or no animation trigger set on the resource.")]
    [SerializeField] private float fallbackDuration = 1f;

    private InteractableResource currentResource;
    private Animator animator;
    private bool isInteracting;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        SetPromptVisible(false);

        if (gatherProgressImage != null) {
            gatherProgressImage.fillAmount = 0f;
            gatherProgressImage.gameObject.SetActive(false);
        }
    }

    // The prompt is a keycap + label under promptRoot when the setup tool has wired one up;
    // otherwise fall back to toggling promptText.gameObject directly so this still works
    // before the tool has been run.
    private void SetPromptVisible(bool visible)
    {
        if (promptRoot != null) {
            promptRoot.SetActive(visible);
        } else if (promptText != null) {
            promptText.gameObject.SetActive(visible);
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
            promptText.text = "gather " + currentResource.resourceName;
            SetPromptVisible(true);
        } else {
            SetPromptVisible(false);
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

        // Snapshot the resource/trigger so this pickup isn't affected by currentResource
        // changing (e.g. the player moving near a different resource) while it plays out.
        InteractableResource resource = currentResource;
        string trigger = resource != null ? resource.animationTrigger : null;

        SetPromptVisible(false);

        if (gatherProgressImage != null) {
            gatherProgressImage.fillAmount = 0f;
            gatherProgressImage.gameObject.SetActive(true);
        }

        if (animator != null && !string.IsNullOrEmpty(trigger)) {
            // Gather Speed upgrades raise this multiplier; since everything below waits on
            // normalizedTime rather than a fixed timer, speeding up the clip here speeds up
            // the whole gather (and its progress radial) proportionally.
            animator.speed = Mathf.Max(0.01f, gatherSpeedMultiplier);
            animator.SetTrigger(trigger);

            // Wait for the animator to actually enter the pickup/gather state (it may take
            // a frame or two), with a timeout so a mismatched trigger/state name can never
            // lock the player out of interacting permanently.
            float elapsed = 0f;
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(trigger) && elapsed < enterStateTimeout) {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Wait for that state to fully finish playing (normalizedTime reaches 1),
            // so the next pickup can't start until this animation has completely played out.
            // The gather timer just visualizes this same normalizedTime as it climbs to 1.
            while (animator.GetCurrentAnimatorStateInfo(0).IsName(trigger)
                   && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) {
                if (gatherProgressImage != null) {
                    gatherProgressImage.fillAmount = Mathf.Clamp01(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                }
                yield return null;
            }

            // Restore normal playback speed so locomotion/other animations aren't affected.
            animator.speed = 1f;
        } else {
            // No animator/trigger to drive off of - fall back to a fixed duration (scaled by
            // the same Gather Speed multiplier), but still tick the gather timer up over that
            // duration instead of just blocking blindly.
            float duration = fallbackDuration / Mathf.Max(0.01f, gatherSpeedMultiplier);
            float elapsed = 0f;
            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                if (gatherProgressImage != null) {
                    gatherProgressImage.fillAmount = Mathf.Clamp01(elapsed / duration);
                }
                yield return null;
            }
        }

        if (gatherProgressImage != null) {
            gatherProgressImage.fillAmount = 0f;
            gatherProgressImage.gameObject.SetActive(false);
        }

        if (resource != null) {
            string resourceName = resource.resourceName;
            resource.Interact();
            ShowGatherPopup(resourceName);
        }

        isInteracting = false;
    }

    public void ShowGatherPopup(string resourceName)
    {
        if (gatherPopup != null) {
            gatherPopup.Show(resourceName);
        }
    }
}
