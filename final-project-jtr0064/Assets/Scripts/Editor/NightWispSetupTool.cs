using UnityEngine;
using UnityEditor;

public class NightWispSetupTool : EditorWindow
{
    // [SerializeField] so these survive the domain reload triggered by any script
    // recompile - otherwise every field silently resets to null/default and the next
    // click of "Create Night Wisps" runs against unassigned references.
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private Transform player;
    [SerializeField] private Terrain targetTerrain;
    [SerializeField] private Transform parentObject;

    [SerializeField] private int count = 500;
    [SerializeField] private float minDistance = 15f;
    [SerializeField] private bool clearExisting = true;

    [SerializeField] private float wanderRadius = 6f;
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float safeZoneRadius = 6f;
    [SerializeField] private Color glowColor = new Color(0.4f, 0.85f, 1f);

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
        clearExisting = EditorGUILayout.Toggle("Clear Existing Before Creating", clearExisting);
        if (count > 100) {
            EditorGUILayout.HelpBox("Large counts create one real-time Light + ParticleSystem per wisp, and all of them activate together at night. Watch the frame rate at night with this many.", MessageType.Warning);
        }

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

        if (targetTerrain == null) {
            targetTerrain = FindObjectOfType<Terrain>();
            if (targetTerrain != null) {
                Debug.Log("Night Wisp Setup: no Target Terrain assigned - auto-found '" + targetTerrain.name + "' in the scene.");
            } else {
                Debug.LogWarning("No Terrain found in the scene - wisps will be scattered in a disc around the parent object instead of following terrain height.");
            }
        }

        if (clearExisting) {
            for (int i = parentObject.childCount - 1; i >= 0; i--) {
                Undo.DestroyObjectImmediate(parentObject.GetChild(i).gameObject);
            }
        }

        // Radius of a disc that can fit `count` circles of radius minDistance/2 at roughly
        // 50% packing density - big enough that the minDistance check below doesn't starve
        // placement the way a fixed-radius ring did (that capped out at ~6 points total).
        float discRadius = minDistance * Mathf.Sqrt(count) * 0.75f;

        int placedCount = 0;
        int attempts = 0;
        int maxAttempts = count * 50;

        while (placedCount < count && attempts < maxAttempts) {
            attempts++;

            Vector3 spawnPosition = targetTerrain != null
                ? RandomTerrainPosition(targetTerrain)
                : parentObject.position + Quaternion.Euler(0, Random.Range(0, 360), 0) * (Vector3.forward * Random.Range(minDistance, discRadius));

            if (!IsFarEnough(spawnPosition)) {
                continue;
            }

            GameObject wisp = BuildWisp(spawnPosition);
            wisp.transform.SetParent(parentObject, true);
            placedCount++;
        }

        if (placedCount < count) {
            Debug.LogWarning("Night Wisp Setup: only placed " + placedCount + "/" + count + " wisp(s) before running out of attempts - lower Minimum Distance Apart or assign a bigger Target Terrain to fit more.");
        } else {
            Debug.Log("Night Wisp Setup: created " + placedCount + " wisp(s) under '" + parentObject.name + "'.");
        }
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

        // AddComponent<ParticleSystem>() leaves the renderer on the built-in default
        // material, whose shader URP can't resolve - that's the "big and pink" missing-
        // shader placeholder. Assign a URP-compatible unlit material explicitly instead.
        Material particleMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        particleMaterial.color = glowColor;
        particles.GetComponent<ParticleSystemRenderer>().material = particleMaterial;

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
