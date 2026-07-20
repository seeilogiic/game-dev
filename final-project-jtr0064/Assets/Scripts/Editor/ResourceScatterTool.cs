using UnityEngine;
using UnityEditor;

public class ResourceScatterTool : EditorWindow
{
    private Terrain targetTerrain;
    private GameObject prefabToScatter;
    private Transform parentObject;

    private int amount = 20;
    private float minDistance = 8f;

    private bool testingMode = false;
    private float testingRadius = 10f;

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

        if (parentObject == null) {
            EditorGUILayout.HelpBox("No Parent Object assigned. Note: The minimum distance check will not work unless a parent is assigned to group and check the resources.", MessageType.Info);
            if (GUILayout.Button("Create & Assign New Parent")) {
                string parentName = prefabToScatter != null ? "Scattered_" + prefabToScatter.name : "ScatteredResources";
                GameObject newParent = new GameObject(parentName);
                Undo.RegisterCreatedObjectUndo(newParent, "Create Parent Object");
                parentObject = newParent.transform;
            }
        }

        amount = EditorGUILayout.IntField("Amount", amount);
        minDistance = EditorGUILayout.FloatField("Minimum Distance", minDistance);

        GUILayout.Space(8);
        testingMode = EditorGUILayout.Toggle("Testing Mode", testingMode);
        if (testingMode) {
            testingRadius = EditorGUILayout.FloatField("Testing Radius (m)", testingRadius);
            EditorGUILayout.HelpBox("Testing Mode: scatters within " + testingRadius + "m of the player (found via PlayerInteraction) and ignores Minimum Distance. Leave off for normal level layout.", MessageType.Warning);
        }

        if (GUILayout.Button("Scatter Resources")) {
            ScatterResources();
        }
    }

    private void ScatterResources() {
        if (targetTerrain == null || prefabToScatter == null) {
            Debug.LogError("Please assign both a target terrain and a prefab to scatter.");
            return;
        }

        if (parentObject == null) {
            string parentName = "Scattered_" + prefabToScatter.name;
            GameObject defaultParent = GameObject.Find(parentName);
            if (defaultParent == null) {
                defaultParent = new GameObject(parentName);
                Undo.RegisterCreatedObjectUndo(defaultParent, "Create " + parentName + " Parent");
            }
            parentObject = defaultParent.transform;
            Debug.Log("Automatically created and assigned parent object: " + parentName);
        }

        TerrainData terrainData = targetTerrain.terrainData;
        Vector3 terrainPosition = targetTerrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        Vector3 playerPosition = Vector3.zero;
        if (testingMode) {
            PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
            if (playerInteraction == null) {
                Debug.LogError("Testing Mode is on but no PlayerInteraction was found in the scene. Make sure the player is present and the scene is loaded.");
                return;
            }
            playerPosition = playerInteraction.transform.position;
        }

        int placedCount = 0;
        int attempts = 0;
        int maxAttempts = amount * 50;

        while (placedCount < amount && attempts < maxAttempts) {
            attempts++;

            float randomX;
            float randomZ;

            if (testingMode) {
                Vector2 offset = Random.insideUnitCircle * testingRadius;
                randomX = playerPosition.x + offset.x - terrainPosition.x;
                randomZ = playerPosition.z + offset.y - terrainPosition.z;

                if (randomX < 0 || randomX > terrainSize.x || randomZ < 0 || randomZ > terrainSize.z) {
                    continue;
                }
            } else {
                randomX = Random.Range(0, terrainSize.x);
                randomZ = Random.Range(0, terrainSize.z);
            }

            float y = terrainData.GetInterpolatedHeight(
                randomX / terrainSize.x,
                randomZ / terrainSize.z
            );

            Vector3 spawnPosition = new Vector3(
                terrainPosition.x + randomX,
                terrainPosition.y + y,
                terrainPosition.z + randomZ
            );

            if (!testingMode && !IsFarEnough(spawnPosition)) {
                continue;
            }

            GameObject newResource = (GameObject)PrefabUtility.InstantiatePrefab(prefabToScatter);

            Undo.RegisterCreatedObjectUndo(newResource, "Scatter Resource");

            newResource.transform.position = spawnPosition;
            newResource.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            newResource.transform.SetParent(parentObject, true);

            placedCount++;
        }

        Debug.Log("Scattered " + placedCount + " resources." + (testingMode ? " (Testing Mode: within " + testingRadius + "m of player, min distance ignored)" : ""));
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
