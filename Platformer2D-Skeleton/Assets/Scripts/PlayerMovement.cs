using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    float moveSpeed = 5f;
    Rigidbody2D rb;
    float jumpForce = 5f;
    SpriteRenderer spriteRenderer;
    int maxJumps = 2;
    int jumpsRemaining;

    [Header("UI Elements")]
    public GameObject winTextObject; 
    public TextMeshProUGUI feedbackText;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        jumpsRemaining = maxJumps;

        if (winTextObject != null) winTextObject.SetActive(false);
    }

    void Update()
    {
        float moveInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput = -1f;
        } else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput = 1f;
        }
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (Keyboard.current.spaceKey.wasPressedThisFrame && jumpsRemaining > 0)
        {
            rb.linearVelocity = new Vector2(rb.velocity.x , jumpForce);
            jumpsRemaining--;
        }
        if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
        } else if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpsRemaining = maxJumps;
        }
    }   

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water") || other.CompareTag("Enemy"))
        {
            RestartLevel();
        }
        if (other.CompareTag("Goal"))
        {
            if (GameObject.FindGameObjectsWithTag("Trunk").Length == 0)
            {
                if (winTextObject != null)
                {
                    if (feedbackText != null)
                    {
                        feedbackText.text = "You Win!";
                        feedbackText.color = new Color(1f, 0.84f, 0f); // Gold
                    }
                    winTextObject.SetActive(true);
                    Invoke("RestartLevel", 2f); // Wait 2 seconds, then restart
                }
            }
            else
            {
                if (winTextObject != null)
                {
                    if (feedbackText != null)
                    {
                        feedbackText.text = "Destroy all Trees!";
                        feedbackText.color = Color.red;
                    }
                    winTextObject.SetActive(true);
                    Invoke("HideUI", 2f);
                }
            }
        }
        if (other.CompareTag("Trunk"))
        {
            Destroy(other.gameObject);
        }
    }

    void HideUI()
    {
        if (winTextObject != null) winTextObject.SetActive(false);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}