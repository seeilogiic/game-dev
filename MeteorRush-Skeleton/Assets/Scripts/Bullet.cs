using UnityEngine;

public class Bullet : MonoBehaviour
{

    float speed = 7f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
        if (transform.position.y > 10f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            int scoreToAdd = 10;
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(scoreToAdd);
            }
            
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Explode();
            }
            else
            {
                Destroy(other.gameObject);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Meteor"))
        {
            int scoreToAdd = 20;
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(scoreToAdd);
            }
            
            var meteor = other.GetComponent<Meteor>();
            if (meteor != null)
            {
                meteor.PlayExplosion();
            }
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
