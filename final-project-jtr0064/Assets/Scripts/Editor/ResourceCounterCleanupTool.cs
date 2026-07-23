using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// One-off cleanup: running a UI setup tool (e.g. Setup Resource Progress Bar) more than once,
// or in more than one Canvas, can leave multiple ResourceCounters in the scene, each with its
// own bound progress bar. DropoffLocation/AddResource only ever update whichever instance
// FindObjectOfType happens to return, so the bar actually on screen can end up being a
// different, never-updated instance - which looks like the progress bar being stuck at 0%.
// This merges any wired references onto one surviving ResourceCounter and removes the rest,
// including their now-orphaned "ResourceProgressBar" visuals.
public static class ResourceCounterCleanupTool
{
    [MenuItem("Tools/Debug/Fix Duplicate Resource Counters")]
    public static void FixDuplicates()
    {
        ResourceCounter[] counters = Object.FindObjectsOfType<ResourceCounter>(true);
        if (counters.Length <= 1) {
            Debug.Log("No duplicate ResourceCounters found (" + counters.Length + " in scene). Nothing to do.");
            return;
        }

        ResourceCounter keeper = counters[0];
        Debug.Log("Found " + counters.Length + " ResourceCounters in the scene. Keeping \"" + keeper.gameObject.name +
            "\", merging any wired references from the rest, then removing the duplicates and their progress bar UI.");

        Undo.RecordObject(keeper, "Merge Resource Counters");

        for (int i = 1; i < counters.Length; i++) {
            ResourceCounter duplicate = counters[i];

            if (keeper.treeText == null) keeper.treeText = duplicate.treeText;
            if (keeper.hayText == null) keeper.hayText = duplicate.hayText;
            if (keeper.grassText == null) keeper.grassText = duplicate.grassText;
            if (keeper.progressFillImage == null) keeper.progressFillImage = duplicate.progressFillImage;
            if (keeper.progressPercentText == null) keeper.progressPercentText = duplicate.progressPercentText;
            if (keeper.winScreenUI == null) keeper.winScreenUI = duplicate.winScreenUI;

            if (duplicate.progressFillImage != null && duplicate.progressFillImage != keeper.progressFillImage) {
                Transform barRoot = duplicate.progressFillImage.transform.parent;
                if (barRoot != null) {
                    Debug.Log("Removing duplicate progress bar UI: " + barRoot.name);
                    Undo.DestroyObjectImmediate(barRoot.gameObject);
                }
            }

            Debug.Log("Removing duplicate ResourceCounter: " + duplicate.gameObject.name);
            Undo.DestroyObjectImmediate(duplicate.gameObject);
        }

        EditorUtility.SetDirty(keeper);
        EditorSceneManager.MarkSceneDirty(keeper.gameObject.scene);

        Debug.Log("Done. Only \"" + keeper.gameObject.name + "\" remains - save the scene.");
    }

    // Batch-mode entry point: opens SampleScene, runs the cleanup, and saves - so this can be
    // driven from the command line without opening the Editor GUI:
    // Unity -batchmode -quit -projectPath <proj> -executeMethod ResourceCounterCleanupTool.RunFromCommandLine
    public static void RunFromCommandLine()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        FixDuplicates();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Scene saved.");
    }
}
