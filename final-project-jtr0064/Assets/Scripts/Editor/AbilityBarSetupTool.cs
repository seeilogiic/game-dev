using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class AbilityBarSetupTool : EditorWindow
{
    private Canvas targetCanvas;

    [MenuItem("Tools/UI/Setup Ability Bar")]
    public static void ShowWindow() {
        GetWindow<AbilityBarSetupTool>("Ability Bar Setup");
    }

    private void OnEnable() {
        if (targetCanvas == null) {
            targetCanvas = FindObjectOfType<Canvas>();
        }
    }

    private void OnGUI() {
        GUILayout.Label("Setup Ability Bar (1 = Auto-Collect, 2 = Highlight)", EditorStyles.boldLabel);

        targetCanvas = (Canvas)EditorGUILayout.ObjectField(
            "Target Canvas",
            targetCanvas,
            typeof(Canvas),
            true
        );

        if (targetCanvas == null) {
            EditorGUILayout.HelpBox("No Canvas assigned. Leave this empty and a Canvas (and EventSystem, if needed) will be created automatically.", MessageType.Info);
        }

        if (GUILayout.Button("Setup Ability Bar")) {
            SetupAbilityBar();
        }
    }

    private void SetupAbilityBar() {
        Canvas canvas = GetOrCreateCanvas();
        targetCanvas = canvas;

        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction == null) {
            Debug.LogError("Could not find a PlayerInteraction component in the scene. Make sure the player is present and the scene is loaded.");
            return;
        }

        GameObject playerObject = playerInteraction.gameObject;
        PlayerAbilities abilities = GetOrAddComponent<PlayerAbilities>(playerObject);

        // Bottom-left, clear of the bottom-center "[E] gather" prompt and the bottom-right
        // menu hint. Slots sit side by side, left to right, 8px apart.
        const float slotSize = 64f;
        const float gap = 8f;
        Image slot1Cooldown = BuildSlot(canvas.transform, "AbilitySlot1", "1", new Vector2(20f, 30f), out GameObject slot1Locked);
        Image slot2Cooldown = BuildSlot(canvas.transform, "AbilitySlot2", "2", new Vector2(20f + slotSize + gap, 30f), out GameObject slot2Locked);

        SerializedObject serializedAbilities = new SerializedObject(abilities);
        serializedAbilities.FindProperty("autoCollectCooldownFill").objectReferenceValue = slot1Cooldown;
        serializedAbilities.FindProperty("autoCollectLockedOverlay").objectReferenceValue = slot1Locked;
        serializedAbilities.FindProperty("highlightCooldownFill").objectReferenceValue = slot2Cooldown;
        serializedAbilities.FindProperty("highlightLockedOverlay").objectReferenceValue = slot2Locked;
        serializedAbilities.ApplyModifiedProperties();

        EditorUtility.SetDirty(abilities);
        EditorSceneManager.MarkSceneDirty(playerObject.scene);

        Debug.Log("Ability bar setup complete. Press 1 for Auto-Collect, 2 for Highlight, once each is unlocked.");
    }

    // Builds one ability slot (backing image, keycap label, radial cooldown fill, locked
    // overlay) and returns its cooldown Image (plus the locked overlay via out) so the
    // caller can wire both into PlayerAbilities.
    private Image BuildSlot(Transform canvasTransform, string slotName, string keycap, Vector2 anchoredPosition, out GameObject lockedOverlay) {
        RectTransform slotRect = GetOrCreateChild(canvasTransform, slotName);
        GameObject slot = slotRect.gameObject;
        slotRect.anchorMin = new Vector2(0f, 0f);
        slotRect.anchorMax = new Vector2(0f, 0f);
        slotRect.pivot = new Vector2(0f, 0f);
        slotRect.sizeDelta = new Vector2(64f, 64f);
        slotRect.anchoredPosition = anchoredPosition;

        Image slotImage = GetOrAddComponent<Image>(slot);
        slotImage.color = new Color(1f, 1f, 1f, 0.15f);

        RectTransform keycapRect = GetOrCreateChild(slotRect, "Keycap");
        keycapRect.anchorMin = new Vector2(0f, 1f);
        keycapRect.anchorMax = new Vector2(0f, 1f);
        keycapRect.pivot = new Vector2(0f, 1f);
        keycapRect.sizeDelta = new Vector2(20f, 20f);
        keycapRect.anchoredPosition = new Vector2(4f, -4f);

        TextMeshProUGUI keycapText = GetOrAddComponent<TextMeshProUGUI>(keycapRect.gameObject);
        keycapText.text = keycap;
        keycapText.fontSize = 14;
        keycapText.color = Color.white;
        keycapText.alignment = TextAlignmentOptions.Center;

        // Radial overlay that sweeps from full (just used) to empty (ready) as the
        // cooldown elapses - PlayerAbilities drives fillAmount every frame.
        RectTransform cooldownRect = GetOrCreateChild(slotRect, "CooldownFill");
        cooldownRect.anchorMin = Vector2.zero;
        cooldownRect.anchorMax = Vector2.one;
        cooldownRect.sizeDelta = Vector2.zero;
        cooldownRect.anchoredPosition = Vector2.zero;

        // Same sprite/settings as the gather-progress radial in GatheringUISetupTool - a
        // plain Image with no sprite ignores fillAmount/Type entirely and just draws a full
        // rectangle, so this must have a sprite assigned for the radial wipe to actually work.
        Image cooldownImage = GetOrAddComponent<Image>(cooldownRect.gameObject);
        cooldownImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        cooldownImage.color = Color.white;
        cooldownImage.type = Image.Type.Filled;
        cooldownImage.fillMethod = Image.FillMethod.Radial360;
        cooldownImage.fillOrigin = (int)Image.Origin360.Top;
        cooldownImage.fillClockwise = true;
        cooldownImage.fillAmount = 0f;
        cooldownRect.gameObject.SetActive(false);

        // Dims the slot and shows "Locked" until the ability is purchased in the upgrade menu.
        RectTransform lockedRect = GetOrCreateChild(slotRect, "LockedOverlay");
        lockedRect.anchorMin = Vector2.zero;
        lockedRect.anchorMax = Vector2.one;
        lockedRect.sizeDelta = Vector2.zero;
        lockedRect.anchoredPosition = Vector2.zero;

        Image lockedImage = GetOrAddComponent<Image>(lockedRect.gameObject);
        lockedImage.color = new Color(0f, 0f, 0f, 0.75f);

        RectTransform lockedTextRect = GetOrCreateChild(lockedRect, "LockedText");
        lockedTextRect.anchorMin = Vector2.zero;
        lockedTextRect.anchorMax = Vector2.one;
        lockedTextRect.sizeDelta = Vector2.zero;
        lockedTextRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI lockedText = GetOrAddComponent<TextMeshProUGUI>(lockedTextRect.gameObject);
        lockedText.text = "Locked";
        lockedText.fontSize = 11;
        lockedText.color = Color.white;
        lockedText.alignment = TextAlignmentOptions.Center;
        lockedText.enableWordWrapping = true;

        lockedOverlay = lockedRect.gameObject;
        return cooldownImage;
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
}
