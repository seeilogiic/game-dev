using UnityEngine;
using UnityEditor;

public class NightWispSetupTool : EditorWindow
{
    private DayNightCycle dayNightCycle;
    private Transform player;
    private Terrain targetTerrain;
    private Transform parentObject;

    private int count = 3;
    private float minDistance = 15f;

    private float wanderRadius = 6f;
    private float detectionRadius = 8f;
    private float safeZoneRadius = 6f;
    private Color glowColor = new Color(0.4f, 0.85f, 1f);

    [MenuItem("Tools/Hazards/Night Wisp Setup Tool")]
    public static void ShowWindow() {
        GetWindow<NightWispSetupTool>("Night Wisp Setup");
    }

    private void OnGUI() {
        GUILayout.Label("Night Wisp Hazard Setup", EditorStyles.boldLabel);

        dayNightCycle = (DayNightCycle)EditorGUILayout.ObjectField(
            "Day/Night Cycle",
            dayNightCycle,
            typeof(DayNightCycle),
            true
        );

        player = (Transform)EditorGUILayout.ObjectField(
            "Player (optional)",
            player,
            typeof(Transform),
            true
        );

        targetTerrain = (Terrain)EditorGUILayout.ObjectField(
            "Target Terrain (optional)",
            targetTerrain,
            typeof(Terrain),
            true
        );

        parentObject = (Transform)EditorGUILayout.ObjectField(
            "Parent Object",
            parentObject,
            typeof(Transform),
            true
        );

        if (parentObject == null) {
            EditorGUILayout.HelpBox("No Parent Object assigned. One named 'NightWisps' will be created/reused automatically.", MessageType.Info);
        }

        if (targetTerrain == null) {
            EditorGUILayout.HelpBox("No terrain assigned - wisps will be placed in a ring around the parent object instead of scattered on terrain.", MessageType.Info);
        }

        EditorGUILayout.Space();
        count = EditorGUILayout.IntField("Wisp Count", count);
        minDistance = EditorGUILayout.FloatField("Minimum Distance Apart", minDistance);

        EditorGUILayout.Space();
        wanderRadius = EditorGUILayout.FloatField("Wander Radius", wanderRadius);
        detectionRadius = EditorGUILayout.FloatField("Detection Radius", detectionRadius);
        safeZoneRadius = EditorGUILayout.FloatField("Dropoff Safe Radius", safeZoneRadius);
        glowColor = EditorGUILayout.ColorField("Glow Color", glowColor);

        if (dayNightCycle == null) {
            EditorGUILayout.HelpBox("Assign the scene's DayNightCycle so wisps know when to activate.", MessageType.Warning);
        }

        if (GUILayout.Button("Create Night Wisps")) {
            CreateWisps();
        }
    }

    private void CreateWisps() {
        if (dayNightCycle == null) {
            Debug.LogError("Assign a DayNightCycle before creating wisps.");
            return;
        }

        if (parentObject == null) {
            GameObject defaultParent = GameObject.Find("NightWisps");
            if (defaultParent == null) {
                defaultParent = new GameObject("NightWisps");
                Undo.RegisterCreatedObjectUndo(defaultParent, "Create NightWisps Parent");
            }
            parentObject = defaultParent.transform;
        }

        if (player == null) {
            PlayerPoints playerPoints = FindObjectOfType<PlayerPoints>();
            if (playerPoints != null) {
                player = playerPoints.transform;
            } else {
                Debug.LogWarning("No PlayerPoints found in the scene - wisps will fall back to finding the player at runtime.");
            }
        }

        int placedCount = 0;
        int attempts = 0;
        int maxAttempts = count * 50;

        while (placedCount < count && attempts < maxAttempts) {
            attempts++;

            Vector3 spawnPosition = targetTerrain != null
                ? RandomTerrainPosition(targetTerrain)
                : parentObject.position + Quaternion.Euler(0, Random.Range(0, 360), 0) * (Vector3.forward * minDistance);

            if (!IsFarEnough(spawnPosition)) {
                continue;
            }

            GameObject wisp = BuildWisp(spawnPosition);
            wisp.transform.SetParent(parentObject, true);
            placedCount++;
        }

        Debug.Log("Night Wisp Setup: created " + placedCount + " wisp(s) under '" + parentObject.name + "'.");
    }

    private Vector3 RandomTerrainPosition(Terrain terrain) {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        float randomX = Random.Range(0, terrainSize.x);
        float randomZ = Random.Range(0, terrainSize.z);
        float y = terrainData.GetInterpolatedHeight(randomX / terrainSize.x, randomZ / terrainSize.z);

        return new Vector3(terrainPosition.x + randomX, terrainPosition.y + y + 1.5f, terrainPosition.z + randomZ);
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

    private GameObject BuildWisp(Vector3 position) {
        GameObject root = new GameObject("NightWisp");
        Undo.RegisterCreatedObjectUndo(root, "Create Night Wisp");
        root.transform.position = position;

        SphereCollider collider = root.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 1f;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);

        ParticleSystem particles = visual.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.startLifetime = 1.5f;
        main.startSpeed = 0.3f;
        main.startSize = 0.25f;
        main.startColor = glowColor;
        main.maxParticles = 40;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 20f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.4f;

        Light glowLight = visual.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = glowColor;
        glowLight.range = 6f;
        glowLight.intensity = 2f;

        visual.SetActive(false);

        NightWisp wisp = root.AddComponent<NightWisp>();
        wisp.dayNightCycle = dayNightCycle;
        wisp.player = player;
        wisp.visualRoot = visual;
        wisp.wanderRadius = wanderRadius;
        wisp.detectionRadius = detectionRadius;
        wisp.safeZoneRadius = safeZoneRadius;

        return root;
    }
}
