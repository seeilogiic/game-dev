using UnityEngine;

public class TrophySpawner : MonoBehaviour
{
    public Transform player;
    public GameObject trophy;
    public TargetArrow targetArrow;
    public WorldSpawner worldSpawner;
    public TargetSpawner targetSpawner;
    public GameManager gameManager;

    public AudioSource audioSource;
    public AudioClip successSound;

    float minSpawnDistance = 10f;
    float collectDistance = 1.5f;

    void Awake()
    {
        if (player == null) player = GameObject.FindWithTag("Player")?.transform;
        if (worldSpawner == null) worldSpawner = Object.FindAnyObjectByType<WorldSpawner>();
        if (targetArrow == null) targetArrow = Object.FindAnyObjectByType<TargetArrow>();
        if (targetSpawner == null) targetSpawner = Object.FindAnyObjectByType<TargetSpawner>();
        if (gameManager == null) gameManager = Object.FindAnyObjectByType<GameManager>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Ensure trophy is hidden at the very start
        if (trophy != null) trophy.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (trophy != null) trophy.SetActive(false);
    }

    public void SpawnTrophy()
    {
        Vector3? pos = worldSpawner.GetRandomRoadPosition(minSpawnDistance);
        if (pos.HasValue)
        {
            trophy.transform.position = pos.Value;
            trophy.SetActive(true);
            if (targetArrow != null) targetArrow.target = trophy.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null || worldSpawner == null || trophy == null || !trophy.activeSelf)
        {
            return;
        }

        float distance = Vector3.Distance(player.position, trophy.transform.position);
        if (distance < collectDistance)
        {
            trophy.SetActive(false);
            if (audioSource != null && successSound != null) audioSource.PlayOneShot(successSound);
            
            if (gameManager != null)
            {
                gameManager.AddScore(1);
            }

            targetSpawner.SpawnTarget();
            return;
        }

        // Check if chunk is still active
        Vector2Int coord = worldSpawner.WorldToGrid(trophy.transform.position);
        if (!worldSpawner.IsChunkActive(coord))
        {
            SpawnTrophy();
        }
    }
}
