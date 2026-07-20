using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceCounter : MonoBehaviour
{
    public TextMeshProUGUI appleText;
    public TextMeshProUGUI oreText;
    public TextMeshProUGUI poppyText;

    public Image progressFillImage;
    public TextMeshProUGUI progressPercentText;

    public WinScreenUI winScreenUI;

    private int apples;
    private int ores;
    private int poppies;
    private int totalApples;
    private int totalOres;
    private int totalPoppies;
    private bool hasWon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CalculateTotals();
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CalculateTotals()
    {
        totalApples = 0;
        totalOres = 0;
        totalPoppies = 0;

        InteractableResource[] resources = FindObjectsOfType<InteractableResource>();
        foreach (InteractableResource resource in resources)
        {
            if (resource == null) continue;

            string type = resource.resourceName.ToLower();
            int amount = resource.usesRemaining * resource.amountPerCollect;

            if (type == "apple")
            {
                totalApples += amount;
            }
            else if (type == "ore")
            {
                totalOres += amount;
            }
            else if (type == "poppy" || type == "poppies")
            {
                totalPoppies += amount;
            }
        }
    }

    private void UpdateUI()
    {
        if (appleText != null)
        {
            appleText.text = "Apples: " + apples + "/" + totalApples;
        }
        if (oreText != null)
        {
            oreText.text = "Ores: " + ores + "/" + totalOres;
        }
        if (poppyText != null)
        {
            poppyText.text = "Poppies: " + poppies + "/" + totalPoppies;
        }

        int totalCollected = apples + ores + poppies;
        int totalAvailable = totalApples + totalOres + totalPoppies;
        float fraction = totalAvailable > 0 ? (float)totalCollected / totalAvailable : 0f;

        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = fraction;
        }
        if (progressPercentText != null)
        {
            progressPercentText.text = Mathf.RoundToInt(fraction * 100f) + "%";
        }

        if (!hasWon && totalAvailable > 0 && totalCollected >= totalAvailable)
        {
            hasWon = true;
            if (winScreenUI != null)
            {
                winScreenUI.Show();
            }
        }
    }

    public void AddResource(string resourceType, int amount)
    {
        switch (resourceType.ToLower())
        {
            case "apple":
                apples += amount;
                break;
            case "ore":
                ores += amount;
                break;
            case "poppy":
            case "poppies":
                poppies += amount;
                break;
            default:
                Debug.LogWarning("Unknown resource type: " + resourceType);
                return;
        }
        UpdateUI();
    }
}
