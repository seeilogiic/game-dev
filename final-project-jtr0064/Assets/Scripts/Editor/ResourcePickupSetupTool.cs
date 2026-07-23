using UnityEngine;
using UnityEditor;

// Wraps a raw art prefab (mesh + optional collider) with an InteractableResource component,
// prefilled with the same attributes already used by the game's gatherable resources (1 use,
// 1 per collect, destroy when empty) - so new art can be scattered with Tools > Terrain >
// Resource Scatter Tool without hand-wiring InteractableResource on each prefab.
public class ResourcePickupSetupTool : EditorWindow
{
    private GameObject sourcePrefab;
    private string resourceName = "";
    private string animationTrigger = "GatherOre";
    private int amountPerCollect = 1;
    private int usesRemaining = 1;
    private bool destroyWhenEmpty = true;
    private string outputFolder = "Assets/Prefabs/Resources";

    [MenuItem("Tools/Gameplay/Setup Resource Pickup")]
    public static void ShowWindow() {
        GetWindow<ResourcePickupSetupTool>("Resource Pickup Setup");
    }

    private void OnGUI() {
        GUILayout.Label("Wrap Art Prefab As a Gatherable Resource", EditorStyles.boldLabel);

        sourcePrefab = (GameObject)EditorGUILayout.ObjectField("Source Prefab", sourcePrefab, typeof(GameObject), false);

        GUILayout.Space(8);
        resourceName = EditorGUILayout.TextField("Resource Name", resourceName);
        animationTrigger = EditorGUILayout.TextField("Animation Trigger", animationTrigger);
        amountPerCollect = EditorGUILayout.IntField("Amount Per Collect", amountPerCollect);
        usesRemaining = EditorGUILayout.IntField("Uses Remaining", usesRemaining);
        destroyWhenEmpty = EditorGUILayout.Toggle("Destroy When Empty", destroyWhenEmpty);

        GUILayout.Space(8);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        GUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "Defaults match the existing Tree/Hay/Grass resources (1 use, 1 per collect, destroy " +
            "when empty). Animation Trigger must match a state name on the player's Animator " +
            "Controller - \"PickFruit\" for tree-like pickups, \"GatherOre\" for ground pickups, " +
            "or a new state you've added yourself. Produces a prefab with InteractableResource " +
            "attached (plus a fitted BoxCollider if the source has no collider of its own), ready " +
            "for Tools > Terrain > Resource Scatter Tool.",
            MessageType.None
        );

        GUILayout.Space(8);

        GUI.enabled = sourcePrefab != null && !string.IsNullOrEmpty(resourceName);
        if (GUILayout.Button("Create Pickup Prefab")) {
            CreatePickupPrefab();
        }
        GUI.enabled = true;
    }

    private void CreatePickupPrefab() {
        EnsureFolder(outputFolder);

        string outputPath = AssetDatabase.GenerateUniqueAssetPath(outputFolder + "/PT_" + sourcePrefab.name + ".prefab");
        string sourcePath = AssetDatabase.GetAssetPath(sourcePrefab);

        if (!AssetDatabase.CopyAsset(sourcePath, outputPath)) {
            Debug.LogError("Failed to copy " + sourcePath + " to " + outputPath);
            return;
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(outputPath);

        InteractableResource resource = contents.GetComponent<InteractableResource>();
        if (resource == null) {
            resource = contents.AddComponent<InteractableResource>();
        }
        resource.resourceName = resourceName;
        resource.amountPerCollect = amountPerCollect;
        resource.usesRemaining = usesRemaining;
        resource.animationTrigger = animationTrigger;
        resource.destroyWhenEmpty = destroyWhenEmpty;

        if (contents.GetComponentInChildren<Collider>() == null) {
            AddFittedBoxCollider(contents);
        }

        PrefabUtility.SaveAsPrefabAsset(contents, outputPath);
        PrefabUtility.UnloadPrefabContents(contents);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created gatherable resource prefab at " + outputPath + " (resourceName=" + resourceName + "). Use Tools > Terrain > Resource Scatter Tool to place it.");
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(outputPath));
    }

    // Falls back to a collider sized to the combined renderer bounds when the source art has
    // none of its own - assumes the root has no rotation, true for typical env prop prefabs.
    private void AddFittedBoxCollider(GameObject root) {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        BoxCollider collider = root.AddComponent<BoxCollider>();

        if (renderers.Length == 0) {
            collider.size = new Vector3(1f, 1f, 1f);
            return;
        }

        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) {
            worldBounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 scale = root.transform.lossyScale;
        collider.center = root.transform.InverseTransformPoint(worldBounds.center);
        collider.size = new Vector3(
            Mathf.Approximately(scale.x, 0f) ? worldBounds.size.x : worldBounds.size.x / scale.x,
            Mathf.Approximately(scale.y, 0f) ? worldBounds.size.y : worldBounds.size.y / scale.y,
            Mathf.Approximately(scale.z, 0f) ? worldBounds.size.z : worldBounds.size.z / scale.z
        );
    }

    private void EnsureFolder(string path) {
        if (AssetDatabase.IsValidFolder(path)) return;

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
}
