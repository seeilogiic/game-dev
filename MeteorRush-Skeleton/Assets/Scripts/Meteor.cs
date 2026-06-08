using UnityEngine;

public class Meteor : MonoBehaviour
{
    float speed = 2f;
    public AudioClip explosionClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
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
                ph.TakeDamage(ph.maxHealth);
            }
            else
            {
                Debug.LogWarning("PlayerHealth component not found on Player.");
            }
            PlayExplosion();
            Destroy(gameObject);
        }
    }
    
    public void PlayExplosion()
    {
        if (explosionClip != null)
        {
            AudioSource.PlayClipAtPoint(explosionClip, transform.position);
        }
    }
}
