using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{

    float acceleration = 5f;
    float deceleration = 5f;
    float maxSpeed = 6f;
    float turnSpeed = 200f;
    float currentSpeed = 0f;
    float speedMultiplier = 1f;
    Vector2 moveInput;
    Rigidbody2D rb;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip collisionSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        float moveAmount = moveInput.y;
        if (moveAmount != 0)
        {
            currentSpeed += moveAmount * acceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
        }
        else 
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }
        float turnAmount = moveInput.x * turnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation - turnAmount);

        // Apply speed multiplier (e.g., 0.5 when on oil)
        rb.linearVelocity = transform.up * currentSpeed * speedMultiplier;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we hit an obstacle (like a cone)
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.name.Contains("Cone"))
        {
            // Kill the speed
            currentSpeed = 0f;

            // Play collision sound
            if (audioSource != null && collisionSound != null)
            {
                audioSource.PlayOneShot(collisionSound);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Oil") || other.gameObject.name.Contains("Oil"))
        {
            speedMultiplier = 0.5f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Oil") || other.gameObject.name.Contains("Oil"))
        {
            speedMultiplier = 1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
