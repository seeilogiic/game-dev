using UnityEngine;
using System.Collections.Generic;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform player;

    [Header("Platform Prefabs")]
    [Tooltip("The simple start platform to spawn under the player at the beginning.")]
    public GameObject startingPlatformPrefab;
    [Tooltip("List of random platform prefabs (obstacles, ramps, speeds, gaps) that can be generated.")]
    public GameObject[] platformPrefabs;

    [Header("Generation Settings")]
    [Tooltip("How far ahead of the player we want to keep platforms spawned.")]
    public float drawDistance = 80f;
    [Tooltip("How far behind the player a platform must be before it is cleaned up.")]
    public float cleanupDistance = 20f;

    // Track the Z coordinate where the next platform should be spawned
    private float nextSpawnZ = 0f;
    // Track the current X alignment of the track connection
    private float currentOffsetX = 0f;

    // List of active platforms in the scene
    private List<GameObject> activePlatforms = new List<GameObject>();

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
        }

        // Align the starting spawn position with the player's actual position in the editor
        if (player != null)
        {
            nextSpawnZ = player.position.z;
            currentOffsetX = player.position.x;
        }

        // 1. Spawn starting platform
        if (startingPlatformPrefab != null)
        {
            SpawnPlatform(startingPlatformPrefab);
        }
        else if (platformPrefabs.Length > 0)
        {
            SpawnPlatform(platformPrefabs[0]);
        }
    }

    void Update()
    {
        if (player == null) return;

        // 2. Continuous Spawning
        // Spawn platforms as long as the next spawn Z position is within the draw distance of the player
        while (nextSpawnZ < player.position.z + drawDistance)
        {
            SpawnRandomPlatform();
        }

        // 3. Continuous Cleanup
        // Recycle or destroy platforms that have fallen far behind the player
        CleanupOldPlatforms();
    }

    void SpawnRandomPlatform()
    {
        if (platformPrefabs == null || platformPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, platformPrefabs.Length);
        GameObject selectedPrefab = platformPrefabs[randomIndex];
        SpawnPlatform(selectedPrefab);
    }

    void SpawnPlatform(GameObject prefab)
    {
        PlatformData data = prefab.GetComponent<PlatformData>();
        float length = 10f;
        float entryOffset = 0f;
        float exitOffset = 0f;

        if (data != null)
        {
            length = data.platformLength;
            entryOffset = data.entryOffsetX;
            exitOffset = data.exitOffsetX;
        }

        // Calculate the correct spawn position
        // Align X coordinate so the entry port of the new platform matches the exit port of the previous one
        float spawnX = currentOffsetX - entryOffset;
        Vector3 spawnPosition = new Vector3(spawnX, 0f, nextSpawnZ);

        // Instantiate platform chunk
        GameObject newPlatform = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
        activePlatforms.Add(newPlatform);

        // Update tracking variables for the next platform chunk
        nextSpawnZ += length;
        currentOffsetX = spawnX + exitOffset;
    }

    void CleanupOldPlatforms()
    {
        // Check platforms from the oldest (first in list)
        while (activePlatforms.Count > 0)
        {
            GameObject platform = activePlatforms[0];
            PlatformData data = platform.GetComponent<PlatformData>();
            float length = data != null ? data.platformLength : 10f;

            // If the platform's far edge is behind the player's position minus the cleanup distance, remove it
            if (platform.transform.position.z + length < player.position.z - cleanupDistance)
            {
                activePlatforms.RemoveAt(0);
                Destroy(platform);
            }
            else
            {
                // Since platforms are spawned in Z-order, if the oldest is not ready for cleanup, none of the rest are.
                break;
            }
        }
    }
}
