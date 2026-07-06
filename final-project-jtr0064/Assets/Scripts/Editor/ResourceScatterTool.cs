using UnityEngine;
using UnityEditor;

public class ResourceScatterTool : EditorWindow
{
    private Terrain targetTerrain;
    private GameObject prefabToScatter;
    private Transform parentObject;

    private int amount = 20;
    private float minDistance = 8f;

    [MenuItem("Tools/Terrain/Resource Scatter Tool")]
    public static void ShowWindow() {
        
        GetWindow<ResourceScatterTool>("Resource Scatter Tool");
    }

    private void OnGUI() {
        GUILayout.Label("Scatter Resources on Terrain", EditorStyles.boldLabel);
        targetTerrain = (Terrain)EditorGUILayout.ObjectField(
            "Target Terrain", 
            targetTerrain, 
            typeof(Terrain), 
            true
        );
        
        prefabToScatter = (GameObject)EditorGUILayout.ObjectField(
            "Prefab to Scatter", 
            prefabToScatter, 
            typeof(GameObject), 
            false
        );

        parentObject = (Transform)EditorGUILayout.ObjectField(
            "Parent Object", 
            parentObject, 
            typeof(Transform), 
            true
        );

        amount = EditorGUILayout.IntField("Amount", amount);
        minDistance = EditorGUILayout.FloatField("Minimum Distance", minDistance);

        if (GUILayout.Button("Scatter Resources")) {
            ScatterResources();
        }
    }

    private void ScatterResources() {
        if (targetTerrain == null || prefabToScatter == null) {
            Debug.LogError("Please assign both a target terrain and a prefab to scatter.");
            return;
        }

        TerrainData terrainData = targetTerrain.terrainData;
        Vector3 terrainPosition = targetTerrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        int placedCount = 0;
        int attempts = 0;
        int maxAttempts = amount * 50;

        while (placedCount < amount && attempts < maxAttempts) {
            attempts++;
            float randomX = Random.Range(0, terrainSize.x);
            float randomZ = Random.Range(0, terrainSize.z);
            
            float y = terrainData.GetInterpolatedHeight(
                randomX / terrainSize.x, 
                randomZ / terrainSize.z
            );

            Vector3 spawnPosition = new Vector3(
                terrainPosition.x + randomX, 
                terrainPosition.y + y, 
                terrainPosition.z + randomZ
            );

            if (!IsFarEnough(spawnPosition)) {
                continue;
            }

            GameObject newResource = (GameObject)PrefabUtility.InstantiatePrefab(prefabToScatter);

            Undo.RegisterCreatedObjectUndo(newResource, "Scatter Resource");

            newResource.transform.position = spawnPosition;
            newResource.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            newResource.transform.SetParent(parentObject, true);

            placedCount++;
        }

        Debug.Log("Scattered " + placedCount + " resources.");
    }

    private bool IsFarEnough(Vector3 position) {
        if (parentObject == null) return true;
        foreach (Transform child in parentObject) {
            if (Vector3.Distance(child.position, position) < minDistance) {
                return false;
            }
        }
        return true;
    }
}
