using UnityEngine;

public class Enemy : MonoBehaviour
{

    public float moveSpeed;
    public float waveAmount;
    public float waveSpeed;
    public AudioClip explosionClip;

    float startY;

    public GameObject enemyBulletPrefab;
    public Transform firePoint;

    float fireRate = 1.5f;
    float nextFireTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveSpeed = Random.Range(1.5f, 3f);
        waveAmount = Random.Range(0.2f, 1f);
        waveSpeed = Random.Range(1f, 3f);
        startY = transform.position.y;
        nextFireTime = Time.time + Random.Range(0.5f, fireRate);

    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        float Y = startY + Mathf.Sin(Time.time * waveSpeed) * waveAmount;

        transform.position = new Vector3(transform.position.x, Y, transform.position.z);
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
        if (transform.position.x > 4f || transform.position.x < -4f)
        {
            Destroy(gameObject);
        }
    }

    void Shoot()
    {
        Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);
    }
    
    void PlayExplosion()
    {
        if (explosionClip != null)
        {
            AudioSource.PlayClipAtPoint(explosionClip, transform.position);
        }
    }
    
    public void Explode()
    {
        PlayExplosion();
        Destroy(gameObject);
    }
}
