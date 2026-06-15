using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    [Header("Audio Settings")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    private int score = 0;
    public float timeLimit = 30f;
    private float currentTime;
    private bool isGameOver = false;

    void Start()
    {
        currentTime = timeLimit;
        SetupMusic();
        UpdateUI();
    }

    void SetupMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.playOnAwake = true;
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    void Update()
    {
        if (isGameOver) return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                isGameOver = true;
                RestartGame();
            }
            UpdateUI();
        }
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Packages Delivered: " + score;
        }
        
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(currentTime).ToString();
        }
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
