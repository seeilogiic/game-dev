using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("The TextMeshPro UI element that will display the score.")]
    public TextMeshProUGUI scoreText;

    private float score = 0f;
    private int currentDisplayScore = 0;

    void Start()
    {
        UpdateScoreUI();
    }

    void Update()
    {
        // Add 10 points every second
        score += 10f * Time.deltaTime;

        // Round down to the nearest integer
        int roundedScore = Mathf.FloorToInt(score);

        // Update the screen text only when the integer changes
        if (roundedScore != currentDisplayScore)
        {
            currentDisplayScore = roundedScore;
            UpdateScoreUI();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentDisplayScore;
        }
    }
}
