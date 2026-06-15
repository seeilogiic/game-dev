using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject target;
    public TargetArrow targetArrow;
    public WorldSpawner worldSpawner;
    public TrophySpawner trophySpawner;

    public AudioSource audioSource;
    public AudioClip pickupSound;

    float minSpawnDistance = 10f;
    float collectDistance = 1.5f;

    void Awake()
    {
        if (player == null) player = GameObject.FindWithTag("Player")?.transform;
        if (worldSpawner == null) worldSpawner = Object.FindAnyObjectByType<WorldSpawner>();
        if (targetArrow == null) targetArrow = Object.FindAnyObjectByType<TargetArrow>();
        if (trophySpawner == null) trophySpawner = Object.FindAnyObjectByType<TrophySpawner>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Ensure both are hidden initially until the world is ready
        if (target != null) target.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Give the WorldSpawner a moment to generate the first chunks
        Invoke(nameof(SpawnTargetInitial), 0.2f);
    }

    void SpawnTargetInitial()
    {
        SpawnTarget();
    }

    public void SpawnTarget()
    {
        Vector3? pos = worldSpawner.GetRandomRoadPosition(minSpawnDistance);
        if (pos.HasValue)
        {
            target.transform.position = pos.Value;
            target.SetActive(true);
            if (targetArrow != null) targetArrow.target = target.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null || worldSpawner == null || target == null || !target.activeSelf)
        {
            return;
        }

        float distance = Vector3.Distance(player.position, target.transform.position);
        if (distance < collectDistance)
        {
            target.SetActive(false);
            if (audioSource != null && pickupSound != null) audioSource.PlayOneShot(pickupSound);
            
            if (trophySpawner != null)
            {
                trophySpawner.SpawnTrophy();
            }
            else
            {
                Debug.LogError("TrophySpawner is not assigned to TargetSpawner!");
            }
            return;
        }

        // Check if chunk is still active
        Vector2Int coord = worldSpawner.WorldToGrid(target.transform.position);
        if (!worldSpawner.IsChunkActive(coord))
        {
            SpawnTarget();
        }
    }
}
