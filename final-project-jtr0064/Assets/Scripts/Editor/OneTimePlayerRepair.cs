using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using StarterAssets;
using Unity.Cinemachine;

// One-time fix for the Mixamo model swap: PlayerModelSwapTool duplicated
// Assets/StarterAssets/.../PlayerArmature.prefab, but PlayerInteraction,
// PlayerAbilities, PlayerUpgrades, PlayerInventory and PlayerPoints were only ever
// added directly to the scene's player instance, never saved into that prefab -
// so swapping the player for a new prefab instance dropped all five, breaking
// inventory, gathering, the compass, the minimap, and the ability bar.
// Run this once (Tools/Player/One-Time Repair (Mixamo Swap)) to restore them with
// the exact values/references the old instance had, then delete this file - it
// has no further use once the scene is fixed and saved.
public static class OneTimePlayerRepair
{
    [MenuItem("Tools/Player/One-Time Repair (Mixamo Swap)")]
    public static void Repair() {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) {
            Debug.LogError("No GameObject tagged 'Player' found in the open scene.");
            return;
        }

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) {
            Debug.LogError("No 'Canvas' GameObject found in the open scene - cannot re-link UI references.");
            return;
        }
        Transform canvasT = canvas.transform;

        RestorePlayerInteraction(player, canvasT);
        RestorePlayerAbilities(player, canvasT);
        RestorePlayerUpgrades(player);
        RestorePlayerInventory(player);
        RestorePlayerPoints(player);

        RelinkSceneReferences(player);
        RelinkCameraFollow(player);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Repaired " + player.name + ": restored PlayerInteraction/PlayerAbilities/PlayerUpgrades/PlayerInventory/PlayerPoints, re-linked NightWisp/CompassMarkerBar/MinimapFollow/CarriedInventoryUI/IntroScreenUI/WinScreenUI/UpgradeMenuUI, and re-pointed the Cinemachine follow camera. Save the scene, then delete OneTimePlayerRepair.cs.");
    }

    // The follow camera's Follow target isn't a scene reference at all - it's baked into
    // NestedParentArmature_Unpack.prefab, which nests the camera and the *old* PlayerArmature
    // together and wires Follow to that specific embedded instance's PlayerCameraRoot. Since
    // the swap tool replaced the player with a free-standing instance outside that nested
    // structure, the baked reference now resolves to nothing. RecordPrefabInstancePropertyModifications
    // is required (not just setting the property) so the override actually persists across
    // that nested-prefab boundary when the scene is saved.
    private static void RelinkCameraFollow(GameObject player) {
        Transform cameraRoot = player.transform.Find("PlayerCameraRoot");
        if (cameraRoot == null) {
            Debug.LogWarning("Player has no PlayerCameraRoot child - cannot re-link the follow camera.");
            return;
        }

        CinemachineVirtualCameraBase[] cameras = Object.FindObjectsByType<CinemachineVirtualCameraBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (cameras.Length == 0) {
            Debug.LogWarning("No Cinemachine camera found in the scene - nothing to re-link.");
            return;
        }

        foreach (CinemachineVirtualCameraBase cam in cameras) {
            Undo.RecordObject(cam, "Repair Player References");
            cam.Follow = cameraRoot;
            EditorUtility.SetDirty(cam);
            PrefabUtility.RecordPrefabInstancePropertyModifications(cam);
        }
    }

    private static Transform Find(Transform root, string path) {
        Transform t = root.Find(path);
        if (t == null) {
            Debug.LogWarning("Could not find '" + path + "' under " + root.name + " - that reference was left unassigned.");
        }
        return t;
    }

    private static void RestorePlayerInteraction(GameObject player, Transform canvasT) {
        if (player.GetComponent<PlayerInteraction>() != null) {
            return;
        }

        PlayerInteraction interaction = Undo.AddComponent<PlayerInteraction>(player);
        interaction.interactionRange = 3f;
        interaction.gatherSpeedMultiplier = 1f;

        Transform gatherPrompt = Find(canvasT, "GatherPrompt");
        Transform gatherLabel = Find(canvasT, "GatherPrompt/GatherLabel");
        Transform gatherProgress = Find(canvasT, "GatherProgress");
        Transform gatherPopup = Find(canvasT, "GatherPopup");

        interaction.promptText = gatherLabel != null ? gatherLabel.GetComponent<TextMeshProUGUI>() : null;
        interaction.gatherProgressImage = gatherProgress != null ? gatherProgress.GetComponent<Image>() : null;
        interaction.gatherPopup = gatherPopup != null ? gatherPopup.GetComponent<GatherPopupUI>() : null;

        SerializedObject so = new SerializedObject(interaction);
        so.FindProperty("promptRoot").objectReferenceValue = gatherPrompt != null ? gatherPrompt.gameObject : null;
        so.ApplyModifiedProperties();
    }

    private static void RestorePlayerAbilities(GameObject player, Transform canvasT) {
        if (player.GetComponent<PlayerAbilities>() != null) {
            return;
        }

        PlayerAbilities abilities = Undo.AddComponent<PlayerAbilities>(player);

        Transform slot1 = Find(canvasT, "AbilitySlot1");
        Transform slot2 = Find(canvasT, "AbilitySlot2");
        Transform slot3 = Find(canvasT, "AbilitySlot3");
        Transform slot4 = Find(canvasT, "AbilitySlot4");

        abilities.autoCollectCooldownFill = ChildImage(slot1, "CooldownFill");
        abilities.autoCollectLockedOverlay = ChildObject(slot1, "LockedOverlay");
        abilities.highlightCooldownFill = ChildImage(slot2, "CooldownFill");
        abilities.highlightLockedOverlay = ChildObject(slot2, "LockedOverlay");
        abilities.autoDropoffCooldownFill = ChildImage(slot3, "CooldownFill");
        abilities.autoDropoffLockedOverlay = ChildObject(slot3, "LockedOverlay");
        abilities.dropoffHighlightCooldownFill = ChildImage(slot4, "CooldownFill");
        abilities.dropoffHighlightLockedOverlay = ChildObject(slot4, "LockedOverlay");
    }

    private static Image ChildImage(Transform slot, string childName) {
        if (slot == null) return null;
        Transform t = slot.Find(childName);
        return t != null ? t.GetComponent<Image>() : null;
    }

    private static GameObject ChildObject(Transform slot, string childName) {
        if (slot == null) return null;
        Transform t = slot.Find(childName);
        return t != null ? t.gameObject : null;
    }

    private static void RestorePlayerUpgrades(GameObject player) {
        if (player.GetComponent<PlayerUpgrades>() != null) {
            return;
        }
        // All fields left at their script defaults exactly match what the old
        // instance had (fresh/unlevelled upgrades) - no further setup needed.
        Undo.AddComponent<PlayerUpgrades>(player);
    }

    private static void RestorePlayerInventory(GameObject player) {
        if (player.GetComponent<PlayerInventory>() != null) {
            return;
        }
        // maxTotalCarry's script default (10) matches the old instance - nothing else to set.
        Undo.AddComponent<PlayerInventory>(player);
    }

    private static void RestorePlayerPoints(GameObject player) {
        if (player.GetComponent<PlayerPoints>() != null) {
            return;
        }
        PlayerPoints points = Undo.AddComponent<PlayerPoints>(player);
        points.points = 1000;
    }

    private static void RelinkSceneReferences(GameObject player) {
        Transform playerT = player.transform;

        NightWisp[] wisps = Object.FindObjectsByType<NightWisp>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (NightWisp wisp in wisps) {
            Undo.RecordObject(wisp, "Repair Player References");
            wisp.player = playerT;
        }

        CompassMarkerBar[] compasses = Object.FindObjectsByType<CompassMarkerBar>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (CompassMarkerBar compass in compasses) {
            Undo.RecordObject(compass, "Repair Player References");
            compass.player = playerT;
        }

        MinimapFollow[] minimapFollows = Object.FindObjectsByType<MinimapFollow>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MinimapFollow follow in minimapFollows) {
            Undo.RecordObject(follow, "Repair Player References");
            follow.target = playerT;
        }

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        CarriedInventoryUI[] inventoryUIs = Object.FindObjectsByType<CarriedInventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (CarriedInventoryUI ui in inventoryUIs) {
            Undo.RecordObject(ui, "Repair Player References");
            ui.inventory = inventory;
        }

        ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
        StarterAssetsInputs starterInputs = player.GetComponent<StarterAssetsInputs>();
        PlayerInput playerInput = player.GetComponent<PlayerInput>();

        IntroScreenUI[] introScreens = Object.FindObjectsByType<IntroScreenUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (IntroScreenUI intro in introScreens) {
            Undo.RecordObject(intro, "Repair Player References");
            intro.controller = controller;
            intro.starterInputs = starterInputs;
            intro.playerInput = playerInput;
        }

        WinScreenUI[] winScreens = Object.FindObjectsByType<WinScreenUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (WinScreenUI win in winScreens) {
            Undo.RecordObject(win, "Repair Player References");
            win.controller = controller;
            win.starterInputs = starterInputs;
            win.playerInput = playerInput;
        }

        PlayerUpgrades upgrades = player.GetComponent<PlayerUpgrades>();
        PlayerPoints points = player.GetComponent<PlayerPoints>();
        PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();

        UpgradeMenuUI[] upgradeMenus = Object.FindObjectsByType<UpgradeMenuUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (UpgradeMenuUI menu in upgradeMenus) {
            Undo.RecordObject(menu, "Repair Player References");
            menu.playerUpgrades = upgrades;
            menu.playerPoints = points;
            menu.controller = controller;
            menu.starterInputs = starterInputs;
            menu.interaction = interaction;
        }
    }
}
