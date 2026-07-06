using UnityEngine;
using TMPro;

public class ResourceCounter : MonoBehaviour
{
    public TextMeshProUGUI appleText;
    public TextMeshProUGUI oreText;

    private int apples;
    private int ores;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateUI()
    {
        appleText.text = "Apples: " + apples;
        oreText.text = "Ores: " + ores;
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
            default:
                Debug.LogWarning("Unknown resource type: " + resourceType);
                return;
        }
        UpdateUI();
    }
}
