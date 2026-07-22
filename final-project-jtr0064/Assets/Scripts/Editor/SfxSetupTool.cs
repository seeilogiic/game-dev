using UnityEngine;
using UnityEditor;

// One-click setup for the SFX layer: creates (or reuses) an "SfxManager" GameObject with an
// AudioSource + SfxManager component wired together. Clip fields are optional here - leave
// any empty and assign them later directly on the SfxManager component once you have real
// clips; PlayOneShot calls for a missing clip are silently skipped, so nothing breaks either
// way.
public class SfxSetupTool : EditorWindow
{
    // Default clip picks from the imported HintsStarsLite pack - pre-filled here so the
    // window opens ready to go; still editable/overridable before clicking the button.
    private const string DefaultGatherClipPath = "Assets/HintsStarsLite/Bell Star 2.wav";
    private const string DefaultDepositClipPath = "Assets/HintsStarsLite/Coin Bag Reward.wav";
    private const string DefaultUpgradeClipPath = "Assets/HintsStarsLite/Harp Money 1.wav";

    private AudioClip gatherClip;
    private AudioClip depositClip;
    private AudioClip upgradeClip;
    private AudioClip wispHitClip;
    private AudioClip uiClickClip;

    [MenuItem("Tools/Gameplay/Setup SFX")]
    public static void ShowWindow() {
        GetWindow<SfxSetupTool>("SFX Setup");
    }

    private void OnEnable() {
        if (gatherClip == null) gatherClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultGatherClipPath);
        if (depositClip == null) depositClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultDepositClipPath);
        if (upgradeClip == null) upgradeClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultUpgradeClipPath);
    }

    private void OnGUI() {
        GUILayout.Label("Setup SFX Manager", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Creates (or reuses) an SfxManager in the scene. Clips below are optional - leave empty and assign them later on the SfxManager component once you have real audio.", MessageType.None);

        GUILayout.Space(8);
        gatherClip = (AudioClip)EditorGUILayout.ObjectField("Gather Clip", gatherClip, typeof(AudioClip), false);
        depositClip = (AudioClip)EditorGUILayout.ObjectField("Deposit Clip", depositClip, typeof(AudioClip), false);
        upgradeClip = (AudioClip)EditorGUILayout.ObjectField("Upgrade Clip", upgradeClip, typeof(AudioClip), false);
        wispHitClip = (AudioClip)EditorGUILayout.ObjectField("Wisp Hit Clip", wispHitClip, typeof(AudioClip), false);
        uiClickClip = (AudioClip)EditorGUILayout.ObjectField("UI Click Clip", uiClickClip, typeof(AudioClip), false);

        GUILayout.Space(8);
        if (GUILayout.Button("Setup SFX Manager")) {
            SetupSfxManager();
        }
    }

    private void SetupSfxManager() {
        SfxManager manager = FindObjectOfType<SfxManager>();
        GameObject managerObject = manager != null ? manager.gameObject : new GameObject("SfxManager");

        if (manager == null) {
            Undo.RegisterCreatedObjectUndo(managerObject, "Create SfxManager");
            manager = managerObject.AddComponent<SfxManager>();
        }

        AudioSource source = managerObject.GetComponent<AudioSource>();
        if (source == null) {
            source = Undo.AddComponent<AudioSource>(managerObject);
        }
        source.playOnAwake = false;
        source.loop = false;
        manager.source = source;

        if (gatherClip != null) manager.gatherClip = gatherClip;
        if (depositClip != null) manager.depositClip = depositClip;
        if (upgradeClip != null) manager.upgradeClip = upgradeClip;
        if (wispHitClip != null) manager.wispHitClip = wispHitClip;
        if (uiClickClip != null) manager.uiClickClip = uiClickClip;

        EditorUtility.SetDirty(manager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(managerObject.scene);

        Debug.Log("SfxManager ready on \"" + managerObject.name + "\". Assign any remaining clips directly on the component once you have real audio.");
    }
}
