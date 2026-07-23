using UnityEngine;
using TMPro;
using System.Collections;

// Brief "Gathered {item}" popup shown after any gather, manual (PlayerInteraction) or
// Auto-Collect (PlayerAbilities).
public class GatherPopupUI : MonoBehaviour
{
    public TextMeshProUGUI popupText;
    [Tooltip("Seconds the message stays fully visible before fading out.")]
    public float displayDuration = 0.8f;
    [Tooltip("Seconds the fade-out takes.")]
    public float fadeDuration = 0.3f;

    private Coroutine activeRoutine;

    void Awake()
    {
        SetAlpha(0f);
    }

    public void Show(string resourceName)
    {
        ShowMessage("Gathered " + resourceName);
    }

    // Same fade behavior as Show(), but with no "Gathered " prefix - for dropoff/inventory
    // feedback that doesn't fit that template (e.g. "Delivered 3 Hay", "Hay inventory full").
    public void ShowMessage(string message)
    {
        if (popupText == null) {
            return;
        }

        popupText.text = message;

        if (activeRoutine != null) {
            StopCoroutine(activeRoutine);
        }
        activeRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        SetAlpha(1f);

        float elapsed = 0f;
        while (elapsed < displayDuration) {
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            SetAlpha(1f - Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(0f);
        activeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (popupText == null) {
            return;
        }

        Color color = popupText.color;
        color.a = alpha;
        popupText.color = color;
    }
}
