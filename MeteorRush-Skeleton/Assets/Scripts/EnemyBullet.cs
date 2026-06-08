using UnityEngine;

public class EnemyBullet : MonoBehaviour
{

    float speed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(1);
            }
            else
            {
                Debug.LogWarning("PlayerHealth component not found on Player.");
            }
            Destroy(gameObject);
        }
    }
}
