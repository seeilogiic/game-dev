using UnityEngine;
using System.Collections.Generic;

public class OilSpawner : MonoBehaviour
{
    public Transform player;
    public GameObject oilPrefab;
    public WorldSpawner worldSpawner;
    
    public int maxOilSpills = 8;
    public float minSpawnDistance = 15f;
    
    private List<GameObject> spawnedOil = new List<GameObject>();

    void Awake()
    {
        if (player == null) player = GameObject.FindWithTag("Player")?.transform;
        if (worldSpawner == null) worldSpawner = Object.FindAnyObjectByType<WorldSpawner>();
    }

    void Start()
    {
        InvokeRepeating(nameof(SpawnBatch), 1.5f, 2f);
    }

    void SpawnBatch()
    {
        spawnedOil.RemoveAll(oil => oil == null);

        if (spawnedOil.Count < maxOilSpills)
        {
            SpawnOil();
        }
    }

    public void SpawnOil()
    {
        if (worldSpawner == null || player == null || oilPrefab == null) return;

        Vector3? pos = worldSpawner.GetRandomRoadPosition(minSpawnDistance);
        if (pos.HasValue)
        {
            if (IsPositionClear(pos.Value))
            {
                GameObject newOil = Instantiate(oilPrefab, pos.Value, Quaternion.identity);
                spawnedOil.Add(newOil);
            }
        }
    }

    bool IsPositionClear(Vector3 position)
    {
        foreach (GameObject oil in spawnedOil)
        {
            if (oil != null && Vector2.Distance(oil.transform.position, position) < 3f)
                return false;
        }

        if (player != null && Vector2.Distance(player.position, position) < 5f)
            return false;

        return true;
    }

    void Update()
    {
        if (worldSpawner == null) return;

        for (int i = spawnedOil.Count - 1; i >= 0; i--)
        {
            GameObject oil = spawnedOil[i];
            if (oil != null)
            {
                Vector2Int coord = worldSpawner.WorldToGrid(oil.transform.position);
                if (!worldSpawner.IsChunkActive(coord))
                {
                    Destroy(oil);
                    spawnedOil.RemoveAt(i);
                }
            }
        }
    }
}
