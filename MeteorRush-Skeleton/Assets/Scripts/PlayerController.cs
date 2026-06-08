using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    float moveSPeed = 5f;
    Vector2 moveInput;

    float monX = -2.5f;
    float maxX = 2.5f;
    float minY = -4.5f;
    float maxY = 4.5f;

    public GameObject bulletPrefab;
    public Transform firePoint;

    float fireRate = 0.25f;
    float nextFireTime = 0f;

    public AudioSource audioSource;
    public AudioClip shootClip;

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnAttack()
    {

        if (Time.time >= nextFireTime)
        {
            audioSource.PlayOneShot(shootClip);
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            nextFireTime = Time.time + fireRate;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0f);
        transform.position += movement * moveSPeed * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, monX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}
