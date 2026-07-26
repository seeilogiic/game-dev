using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

// Automates swapping the player's model (e.g. a Mixamo FBX) into a new
// PlayerArmature_<Model> prefab, following the same steps as a manual swap:
// rig the model Humanoid, duplicate the base player prefab with the model
// replacing its old Geometry/Skeleton children, assign the new Avatar, then
// (optionally) replace the player in the open scene and re-link every scene
// script that referenced the old player (Cinemachine follow camera included)
// to the new instance.
public class PlayerModelSwapTool : EditorWindow
{
    private const string DefaultBasePrefabPath = "Assets/StarterAssets/ThirdPersonController/Prefabs/PlayerArmature.prefab";
    private const string PlayerCameraRootName = "PlayerCameraRoot";

    private GameObject sourceModel;
    private GameObject basePlayerPrefab;
    private bool replaceInScene = true;

    [MenuItem("Tools/Player/Model Swap Tool")]
    public static void ShowWindow() {
        GetWindow<PlayerModelSwapTool>("Player Model Swap");
    }

    private void OnEnable() {
        if (basePlayerPrefab == null) {
            basePlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultBasePrefabPath);
        }
    }

    private void OnGUI() {
        GUILayout.Label("Swap Player Character Model", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Rigs the source model as Humanoid, builds a PlayerArmature_<Model> prefab from " +
            "the base player prefab with the model swapped in, and (optionally) replaces the " +
            "player in the currently open scene, re-linking the Cinemachine follow camera.",
            MessageType.None);

        sourceModel = (GameObject)EditorGUILayout.ObjectField(
            "Source Model (FBX)", sourceModel, typeof(GameObject), false);

        basePlayerPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Base Player Prefab", basePlayerPrefab, typeof(GameObject), false);

        replaceInScene = EditorGUILayout.Toggle("Replace Player In Open Scene", replaceInScene);

        if (sourceModel == null || basePlayerPrefab == null) {
            EditorGUILayout.HelpBox("Assign both the source model and the base player prefab.", MessageType.Warning);
        }

        GUILayout.Space(8);
        using (new EditorGUI.DisabledScope(sourceModel == null || basePlayerPrefab == null)) {
            if (GUILayout.Button("Swap Player Model")) {
                SwapPlayerModel();
            }
        }
    }

    private void SwapPlayerModel() {
        string modelPath = AssetDatabase.GetAssetPath(sourceModel);
        ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer == null) {
            Debug.LogError("Source Model is not an imported model asset (no ModelImporter found) at " + modelPath);
            return;
        }

        if (importer.animationType != ModelImporterAnimationType.Human) {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.SaveAndReimport();
            sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        }

        Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault();
        if (avatar == null) {
            Debug.LogError("Could not find/generate a Humanoid Avatar on " + modelPath + " - check the model's Rig import settings manually.");
            return;
        }
        if (!avatar.isValid || !avatar.isHuman) {
            Debug.LogWarning("Avatar on " + modelPath + " is not a valid Humanoid avatar - Unity may not have been able to auto-map its bones. Check Configure Avatar on the model's Rig tab.");
        }

        string basePrefabPath = AssetDatabase.GetAssetPath(basePlayerPrefab);
        string folder = Path.GetDirectoryName(basePrefabPath);
        string newPrefabName = "PlayerArmature_" + sourceModel.name;
        string newPrefabPath = (folder + "/" + newPrefabName + ".prefab").Replace('\\', '/');

        if (AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPath) != null) {
            Debug.LogWarning("Overwriting existing prefab at " + newPrefabPath);
            AssetDatabase.DeleteAsset(newPrefabPath);
        }

        if (!AssetDatabase.CopyAsset(basePrefabPath, newPrefabPath)) {
            Debug.LogError("Failed to duplicate " + basePrefabPath + " to " + newPrefabPath);
            return;
        }
        AssetDatabase.Refresh();

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(newPrefabPath);
        try {
            Transform[] children = prefabRoot.transform.Cast<Transform>().ToArray();
            foreach (Transform child in children) {
                if (child.name != PlayerCameraRootName) {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(sourceModel);
            modelInstance.transform.SetParent(prefabRoot.transform, false);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one;

            Animator animator = prefabRoot.GetComponent<Animator>();
            if (animator == null) {
                Debug.LogWarning("Base player prefab has no Animator component on its root - avatar was not assigned.");
            } else {
                animator.avatar = avatar;
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, newPrefabPath);
        } finally {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.Refresh();
        GameObject newPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPath);
        Debug.Log("Created " + newPrefabPath + " with " + sourceModel.name + "'s model and Humanoid avatar assigned.");

        if (replaceInScene) {
            ReplacePlayerInScene(newPrefabAsset);
        }

        Debug.LogWarning("Double-check the Character Controller (Height/Center) and " + PlayerCameraRootName +
            " position on the new prefab against " + sourceModel.name + "'s actual proportions - both were " +
            "carried over unchanged from the base prefab and may need manual tuning.");
    }

    private void ReplacePlayerInScene(GameObject newPrefabAsset) {
        PlayerInteraction[] players = FindObjectsByType<PlayerInteraction>(FindObjectsSortMode.None);
        if (players.Length == 0) {
            Debug.LogWarning("No PlayerInteraction found in the open scene - skipped scene swap. Drag " + newPrefabAsset.name + " into the scene manually.");
            return;
        }
        if (players.Length > 1) {
            Debug.LogWarning("Multiple PlayerInteraction objects found in the scene - swapping only the first one (" + players[0].name + ").");
        }

        GameObject oldPlayer = players[0].gameObject;
        Transform oldTransform = oldPlayer.transform;

        Transform parent = oldTransform.parent;
        int siblingIndex = oldTransform.GetSiblingIndex();
        string originalName = oldPlayer.name;

        GameObject newPlayer = (GameObject)PrefabUtility.InstantiatePrefab(newPrefabAsset);
        Undo.RegisterCreatedObjectUndo(newPlayer, "Swap Player Model");
        newPlayer.transform.SetParent(parent, false);
        newPlayer.transform.localPosition = oldTransform.localPosition;
        newPlayer.transform.localRotation = oldTransform.localRotation;
        newPlayer.transform.localScale = oldTransform.localScale;
        newPlayer.transform.SetSiblingIndex(siblingIndex);
        newPlayer.name = originalName;

        CopyExtraComponents(oldPlayer, newPlayer);
        int relinkedFields = RelinkExternalReferences(oldPlayer, newPlayer);

        Undo.DestroyObjectImmediate(oldPlayer);

        Selection.activeGameObject = newPlayer;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        if (relinkedFields == 0) {
            Debug.LogWarning("No other scene object had a reference to the old player (e.g. a Cinemachine follow camera) - if something should have followed/tracked it, re-point that manually.");
        }

        Debug.Log("Replaced " + originalName + " in the scene with an instance of " + newPrefabAsset.name +
            " and re-linked " + relinkedFields + " scene reference(s) to it.");
    }

    // Any scene script that had a serialized reference to the old player, its Transform, one
    // of its components, or a named child (e.g. PlayerCameraRoot for a Cinemachine follow
    // camera) gets that reference repointed at the equivalent object on the new player. This
    // is generic on purpose - the player accumulates new consumer scripts over time (compass,
    // minimap, UI, ability bars...) and this shouldn't need updating every time one is added.
    private int RelinkExternalReferences(GameObject oldPlayer, GameObject newPlayer) {
        Dictionary<Object, Object> oldToNew = new Dictionary<Object, Object>();
        oldToNew[oldPlayer] = newPlayer;
        oldToNew[oldPlayer.transform] = newPlayer.transform;

        foreach (Component oldComponent in oldPlayer.GetComponents<Component>()) {
            if (oldComponent == null || oldComponent is Transform) {
                continue;
            }
            Component newComponent = newPlayer.GetComponent(oldComponent.GetType());
            if (newComponent != null) {
                oldToNew[oldComponent] = newComponent;
            }
        }

        foreach (Transform oldChild in oldPlayer.GetComponentsInChildren<Transform>(true)) {
            if (oldChild == oldPlayer.transform) {
                continue;
            }
            Transform newChild = newPlayer.transform.Find(oldChild.name);
            if (newChild != null) {
                oldToNew[oldChild] = newChild;
                oldToNew[oldChild.gameObject] = newChild.gameObject;
            }
        }

        int relinkedFields = 0;
        MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MonoBehaviour behaviour in allBehaviours) {
            if (behaviour == null || behaviour.transform == oldPlayer.transform || behaviour.transform.IsChildOf(newPlayer.transform)) {
                continue;
            }

            SerializedObject so = new SerializedObject(behaviour);
            SerializedProperty prop = so.GetIterator();
            bool changed = false;
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren)) {
                enterChildren = true;
                if (prop.propertyType != SerializedPropertyType.ObjectReference) {
                    continue;
                }
                if (prop.objectReferenceValue != null && oldToNew.TryGetValue(prop.objectReferenceValue, out Object replacement)) {
                    prop.objectReferenceValue = replacement;
                    changed = true;
                    relinkedFields++;
                }
            }
            if (changed) {
                so.ApplyModifiedProperties();
                // Some consumers (e.g. the follow camera) live inside a nested prefab that
                // bakes its reference to the old player at the asset level rather than the
                // scene level - a plain property set doesn't necessarily persist across that
                // nested-prefab boundary on save without explicitly recording the override.
                PrefabUtility.RecordPrefabInstancePropertyModifications(behaviour);
            }
        }

        return relinkedFields;
    }

    // The base player prefab doesn't necessarily carry every gameplay script the scene's
    // player instance has - some (e.g. ones with scene-only UI references) may only ever
    // have been added directly to the scene instance. Carry those across so swapping models
    // doesn't silently drop them. CopyComponent/PasteComponentAsNew preserves all serialized
    // fields, including private ones and scene object references, without needing to know
    // the component's fields ahead of time.
    private void CopyExtraComponents(GameObject oldPlayer, GameObject newPlayer) {
        Component[] oldComponents = oldPlayer.GetComponents<Component>();
        foreach (Component oldComponent in oldComponents) {
            if (oldComponent == null || oldComponent is Transform) {
                continue;
            }
            if (newPlayer.GetComponent(oldComponent.GetType()) != null) {
                continue;
            }

            ComponentUtility.CopyComponent(oldComponent);
            if (ComponentUtility.PasteComponentAsNew(newPlayer)) {
                Debug.Log("Carried over " + oldComponent.GetType().Name + " (not part of the base prefab) from the old player.");
            } else {
                Debug.LogWarning("Failed to copy " + oldComponent.GetType().Name + " from the old player onto the new one - re-add and re-configure it manually.");
            }
        }
    }
}
