using UnityEngine;
using System.Collections.Generic;

public class ConeSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject conePrefab;
    public WorldSpawner worldSpawner;
    
    public int maxCones = 15;
    public float minSpawnDistance = 10f;
    
    private List<GameObject> spawnedCones = new List<GameObject>();

    void Awake()
    {
        if (player == null) player = GameObject.FindWithTag("Player")?.transform;
        if (worldSpawner == null) worldSpawner = Object.FindAnyObjectByType<WorldSpawner>();
    }

    void Start()
    {
        // Start spawning after a short delay
        InvokeRepeating(nameof(SpawnBatch), 1f, 1f);
    }

    void SpawnBatch()
    {
        // Cleanup missing/destroyed cones
        spawnedCones.RemoveAll(cone => cone == null);

        if (spawnedCones.Count < maxCones)
        {
            SpawnCone();
        }
    }

    public void SpawnCone()
    {
        if (worldSpawner == null || player == null || conePrefab == null) return;

        Vector3? pos = worldSpawner.GetRandomRoadPosition(minSpawnDistance);
        if (pos.HasValue)
        {
            // Simple overlap check
            if (IsPositionClear(pos.Value))
            {
                GameObject newCone = Instantiate(conePrefab, pos.Value, Quaternion.identity);
                spawnedCones.Add(newCone);
            }
        }
    }

    bool IsPositionClear(Vector3 position)
    {
        foreach (GameObject cone in spawnedCones)
        {
            if (cone != null && Vector2.Distance(cone.transform.position, position) < 1.5f)
                return false;
        }

        if (player != null && Vector2.Distance(player.position, position) < 4f)
            return false;

        return true;
    }

    void Update()
    {
        if (worldSpawner == null) return;

        // Cleanup cones that are on inactive chunks (same as target logic)
        for (int i = spawnedCones.Count - 1; i >= 0; i--)
        {
            GameObject cone = spawnedCones[i];
            if (cone != null)
            {
                Vector2Int coord = worldSpawner.WorldToGrid(cone.transform.position);
                if (!worldSpawner.IsChunkActive(coord))
                {
                    Destroy(cone);
                    spawnedCones.RemoveAt(i);
                }
            }
        }
    }
}
