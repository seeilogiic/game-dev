using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

// Generates the Apple/Ore/Poppy dropoff prefabs (each with DropoffLocation already attached
// and configured) and wires up PlayerInventory + the carried-resources UI. Placing the
// prefabs on the terrain is a manual step - drag them into Tools > Terrain > Resource Scatter
// Tool (Amount 1-3 per type is plenty; dropoffs are meant to be sparse, unlike gatherable
// resources) rather than something this tool does automatically.
public class DropoffSetupTool : EditorWindow
{
    private Canvas targetCanvas;

    // Leave a prefab slot empty to auto-generate a colored placeholder for that type.
    private GameObject applePrefabOverride;
    private GameObject orePrefabOverride;
    private GameObject poppyPrefabOverride;

    private const string PrefabFolder = "Assets/Prefabs/Dropoffs";

    [MenuItem("Tools/Gameplay/Setup Dropoffs")]
    public static void ShowWindow() {
        GetWindow<DropoffSetupTool>("Dropoff Setup");
    }

    private void OnEnable() {
        if (targetCanvas == null) {
            targetCanvas = FindObjectOfType<Canvas>();
        }
    }

    private void OnGUI() {
        GUILayout.Label("Setup Carry + Dropoff Mechanic", EditorStyles.boldLabel);

        targetCanvas = (Canvas)EditorGUILayout.ObjectField(
            "Target Canvas",
            targetCanvas,
            typeof(Canvas),
            true
        );

        if (targetCanvas == null) {
            EditorGUILayout.HelpBox("No Canvas assigned. Leave this empty and a Canvas (and EventSystem, if needed) will be created automatically.", MessageType.Info);
        }

        GUILayout.Space(8);
        GUILayout.Label("Dropoff Prefabs (optional - leave empty to auto-generate a placeholder)", EditorStyles.boldLabel);

        applePrefabOverride = (GameObject)EditorGUILayout.ObjectField("Apple Dropoff Prefab", applePrefabOverride, typeof(GameObject), false);
        orePrefabOverride = (GameObject)EditorGUILayout.ObjectField("Ore Dropoff Prefab", orePrefabOverride, typeof(GameObject), false);
        poppyPrefabOverride = (GameObject)EditorGUILayout.ObjectField("Poppy Dropoff Prefab", poppyPrefabOverride, typeof(GameObject), false);

        GUILayout.Space(8);

        if (GUILayout.Button("Setup Dropoffs")) {
            SetupDropoffs();
        }

        EditorGUILayout.HelpBox("This only creates the prefabs (with DropoffLocation attached) and wires the player/UI. Place the prefabs on the terrain yourself with Tools > Terrain > Resource Scatter Tool - use a small Amount (1-3) per type, since dropoffs should be sparse.", MessageType.None);
    }

    private void SetupDropoffs() {
        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction == null) {
            Debug.LogError("Could not find a PlayerInteraction component in the scene. Make sure the player is present and the scene is loaded.");
            return;
        }

        GameObject playerObject = playerInteraction.gameObject;
        PlayerInventory inventory = GetOrAddComponent<PlayerInventory>(playerObject);

        Canvas canvas = GetOrCreateCanvas();
        targetCanvas = canvas;

        GameObject applePrefab = GetOrCreatePlaceholderPrefab("apple", applePrefabOverride);
        GameObject orePrefab = GetOrCreatePlaceholderPrefab("ore", orePrefabOverride);
        GameObject poppyPrefab = GetOrCreatePlaceholderPrefab("poppy", poppyPrefabOverride);

        CarriedInventoryUI carriedUI = SetupCarriedInventoryUI(canvas, inventory);

        EditorUtility.SetDirty(inventory);
        EditorUtility.SetDirty(carriedUI);
        EditorSceneManager.MarkSceneDirty(playerObject.scene);

        Debug.Log("Dropoff setup complete: PlayerInventory wired onto the player, a \"Carrying: ...\" label was added to the canvas, and Dropoff_Apple/Ore/Poppy prefabs are ready at " + PrefabFolder + ". Use Tools > Terrain > Resource Scatter Tool to place them (small Amount per type).");
    }

    private GameObject GetOrCreatePlaceholderPrefab(string resourceType, GameObject overridePrefab) {
        if (overridePrefab != null) {
            return overridePrefab;
        }

        EnsureFolder(PrefabFolder);

        string typeName = Capitalize(resourceType);
        string path = PrefabFolder + "/Dropoff_" + typeName + ".prefab";

        GameObject existingAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existingAsset != null) {
            return existingAsset;
        }

        // Root sits at ground level (this is the transform Resource Scatter Tool positions/rotates).
        // DropoffLocation lives here so it's found no matter which child collider gets hit.
        GameObject root = new GameObject("Dropoff_" + typeName);
        DropoffLocation dropoff = root.AddComponent<DropoffLocation>();
        dropoff.acceptedResourceType = resourceType;

        // Visual is a separate child, lifted so its base sits on the ground instead of being
        // buried half-underground (a centered-pivot primitive would otherwise poke half into
        // the terrain) - tall/wide enough to spot from a distance while scattering.
        const float diameter = 4f;
        const float height = 3f;

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = new Vector3(0f, height / 2f, 0f);
        // Unity's built-in Cylinder is 2 units tall and 1 unit across at scale (1,1,1).
        visual.transform.localScale = new Vector3(diameter, height / 2f, diameter);

        MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
        Material material = new Material(GetDefaultShader());
        SetMaterialColor(material, GetColorForType(resourceType));
        AssetDatabase.CreateAsset(material, PrefabFolder + "/Dropoff_" + typeName + "_Mat.mat");
        renderer.sharedMaterial = material;

        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);

        Debug.Log("Generated placeholder dropoff prefab at " + path + " (a " + diameter + "x" + height + "m marker) - replace its mesh/material with real art whenever you have some.");

        return prefabAsset;
    }

    private Shader GetDefaultShader() {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) {
            shader = Shader.Find("Standard");
        }
        return shader;
    }

    private void SetMaterialColor(Material material, Color color) {
        if (material.HasProperty("_BaseColor")) {
            material.SetColor("_BaseColor", color);
        } else if (material.HasProperty("_Color")) {
            material.SetColor("_Color", color);
        }
    }

    private Color GetColorForType(string resourceType) {
        switch (resourceType.ToLower()) {
            case "ore":
                return new Color(0.55f, 0.55f, 0.6f);
            case "poppy":
                return new Color(0.9f, 0.25f, 0.55f);
            case "apple":
                return new Color(0.8f, 0.15f, 0.15f);
            default:
                return Color.white;
        }
    }

    private void EnsureFolder(string path) {
        if (AssetDatabase.IsValidFolder(path)) {
            return;
        }

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++) {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    private CarriedInventoryUI SetupCarriedInventoryUI(Canvas canvas, PlayerInventory inventory) {
        RectTransform carriedRect = GetOrCreateChild(canvas.transform, "CarriedInventoryText");
        carriedRect.anchorMin = new Vector2(0f, 0f);
        carriedRect.anchorMax = new Vector2(0f, 0f);
        carriedRect.pivot = new Vector2(0f, 0f);
        carriedRect.sizeDelta = new Vector2(420f, 30f);
        carriedRect.anchoredPosition = new Vector2(20f, 20f);

        TextMeshProUGUI carriedText = GetOrAddComponent<TextMeshProUGUI>(carriedRect.gameObject);
        carriedText.fontSize = 16;
        carriedText.color = Color.white;
        carriedText.alignment = TextAlignmentOptions.Left;

        CarriedInventoryUI carriedUI = GetOrAddComponent<CarriedInventoryUI>(carriedRect.gameObject);
        SerializedObject serializedCarriedUI = new SerializedObject(carriedUI);
        serializedCarriedUI.FindProperty("inventory").objectReferenceValue = inventory;
        serializedCarriedUI.FindProperty("carriedText").objectReferenceValue = carriedText;
        serializedCarriedUI.ApplyModifiedProperties();

        return carriedUI;
    }

    private Canvas GetOrCreateCanvas() {
        if (targetCanvas != null) {
            return targetCanvas;
        }

        Canvas existing = FindObjectOfType<Canvas>();
        if (existing != null) {
            return existing;
        }

        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.referenceResolution = new Vector2(800f, 600f);

        EnsureEventSystem();

        Debug.Log("No Canvas found in the scene - created a new one.");

        return canvas;
    }

    private void EnsureEventSystem() {
        if (FindObjectOfType<EventSystem>() != null) {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");

        InputSystemUIInputModule uiModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
        uiModule.AssignDefaultActions();
    }

    private RectTransform GetOrCreateChild(Transform parent, string name) {
        Transform existing = parent.Find(name);
        if (existing != null) {
            return existing.GetComponent<RectTransform>();
        }

        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private T GetOrAddComponent<T>(GameObject go) where T : Component {
        T component = go.GetComponent<T>();
        if (component == null) {
            component = Undo.AddComponent<T>(go);
        }
        return component;
    }

    private static string Capitalize(string s) {
        if (string.IsNullOrEmpty(s)) {
            return s;
        }
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}
