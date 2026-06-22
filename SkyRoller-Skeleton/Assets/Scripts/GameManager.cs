using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    [Tooltip("The GameOver UI Panel that contains the restart and home buttons.")]
    public GameObject gameOverPanel;

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Hide the game over screen at the beginning of the level
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Make sure time scale is normal when a new game starts
        Time.timeScale = 1f;
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Pause the game physics and updates
        Time.timeScale = 0f;

        // Display the Game Over screen
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        // Unpause time before reloading
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToHome()
    {
        // Unpause time and return to main menu
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
