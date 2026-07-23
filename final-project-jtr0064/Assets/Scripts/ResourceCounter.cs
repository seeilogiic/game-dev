using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;

public class ResourceCounter : MonoBehaviour
{
    [FormerlySerializedAs("appleText")]
    public TextMeshProUGUI treeText;
    [FormerlySerializedAs("oreText")]
    public TextMeshProUGUI hayText;
    [FormerlySerializedAs("poppyText")]
    public TextMeshProUGUI grassText;

    public Image progressFillImage;
    public TextMeshProUGUI progressPercentText;

    public WinScreenUI winScreenUI;

    private int trees;
    private int hay;
    private int grass;
    private int totalTrees;
    private int totalHay;
    private int totalGrass;
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
        totalTrees = 0;
        totalHay = 0;
        totalGrass = 0;

        InteractableResource[] resources = FindObjectsOfType<InteractableResource>();
        foreach (InteractableResource resource in resources)
        {
            if (resource == null) continue;

            string type = resource.resourceName.ToLower();
            int amount = resource.usesRemaining * resource.amountPerCollect;

            if (type == "tree")
            {
                totalTrees += amount;
            }
            else if (type == "hay")
            {
                totalHay += amount;
            }
            else if (type == "grass")
            {
                totalGrass += amount;
            }
        }
    }

    private void UpdateUI()
    {
        if (treeText != null)
        {
            treeText.text = "Trees: " + trees + "/" + totalTrees;
        }
        if (hayText != null)
        {
            hayText.text = "Hay: " + hay + "/" + totalHay;
        }
        if (grassText != null)
        {
            grassText.text = "Grass: " + grass + "/" + totalGrass;
        }

        int totalCollected = trees + hay + grass;
        int totalAvailable = totalTrees + totalHay + totalGrass;
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
            case "tree":
                trees += amount;
                break;
            case "hay":
                hay += amount;
                break;
            case "grass":
                grass += amount;
                break;
            default:
                Debug.LogWarning("Unknown resource type: " + resourceType);
                return;
        }
        UpdateUI();
    }
}
