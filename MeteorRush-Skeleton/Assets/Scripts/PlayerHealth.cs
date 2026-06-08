using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    // Changed to public so the UI script can read it
    [HideInInspector] public int currentHealth = 5;
    
    public AudioSource audioSource;
    public AudioClip explosionClip; 

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        
        // Clamp health so it doesn't go below 0
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        Debug.Log("Player health: " + currentHealth + "/" + maxHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died.");
        audioSource.PlayOneShot(explosionClip);
        Invoke("ReloadScene", 1f);
    }
    
    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}