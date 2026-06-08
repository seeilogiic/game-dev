using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{

    public GameObject meteorPrefab;
    float spawnRate = 4f;
    float minY = -1f;
    float maxY = 1f;
    float nextSpawnTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnMeteor();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnMeteor()
    {
        float spawnY = Random.Range(minY, maxY);
        float spawnX = Random.Range(-2.2f, 2.5f);
        Instantiate(meteorPrefab, new Vector3(spawnX, spawnY, 0f), Quaternion.Euler(0f, 0f, 0f));
    }
}
