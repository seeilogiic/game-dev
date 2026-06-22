using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZone : MonoBehaviour
{
    private Transform player;

    void Start()
    {
        // Find the player in the scene
        player = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Keep the Death Zone centered under the player on the Z-axis (forward movement)
        // Maintain the original X and Y position (height below track)
        Vector3 newPos = transform.position;
        newPos.z = player.position.z;
        transform.position = newPos;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
