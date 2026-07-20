using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using StarterAssets;

public class IntroScreenSetupTool : EditorWindow
{
    private Canvas targetCanvas;

    [MenuItem("Tools/UI/Setup Intro Screen")]
    public static void ShowWindow() {
        GetWindow<IntroScreenSetupTool>("Intro Screen Setup");
    }

    private void OnEnable() {
        if (targetCanvas == null) {
            targetCanvas = FindObjectOfType<Canvas>();
        }
    }

    private void OnGUI() {
        GUILayout.Label("Setup Intro Screen (Enter to dismiss)", EditorStyles.boldLabel);

        targetCanvas = (Canvas)EditorGUILayout.ObjectField(
            "Target Canvas",
            targetCanvas,
            typeof(Canvas),
            true
        );

        if (targetCanvas == null) {
            EditorGUILayout.HelpBox("No Canvas assigned. Leave this empty and a Canvas (and EventSystem, if needed) will be created automatically.", MessageType.Info);
        }

        if (GUILayout.Button("Setup Intro Screen")) {
            SetupIntroScreen();
        }
    }

    private void SetupIntroScreen() {
        Canvas canvas = GetOrCreateCanvas();
        targetCanvas = canvas;

        // Explicitly reset this every run (not just on canvas creation) so it also fixes an
        // existing canvas that may have been left in a different scale mode.
        CanvasScaler scaler = GetOrAddComponent<CanvasScaler>(canvas.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.referenceResolution = new Vector2(800f, 600f);

        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction == null) {
            Debug.LogError("Could not find a PlayerInteraction component in the scene. Make sure the player is present and the scene is loaded.");
            return;
        }

        GameObject playerObject = playerInteraction.gameObject;
        ThirdPersonController controller = playerObject.GetComponent<ThirdPersonController>();
        StarterAssetsInputs starterInputs = playerObject.GetComponent<StarterAssetsInputs>();
        PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();

        if (controller == null || starterInputs == null || playerInput == null) {
            Debug.LogError("Player object is missing ThirdPersonController, StarterAssetsInputs, or PlayerInput. Cannot wire up the intro screen.");
            return;
        }

        // Rebuild fresh each time rather than patch in place, same as the other UI setup tools.
        Transform existingPanel = canvas.transform.Find("IntroScreenPanel");
        if (existingPanel != null) {
            Undo.DestroyObjectImmediate(existingPanel.gameObject);
        }

        RectTransform panelRect = GetOrCreateChild(canvas.transform, "IntroScreenPanel");
        GameObject panel = panelRect.gameObject;
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // Full-bleed dark backdrop so the game world reads as blocked, not just dimmed.
        UIStyle.RoundedImage(panel, new Color(0f, 0f, 0f, 0.85f));

        IntroScreenUI introUI = GetOrAddComponent<IntroScreenUI>(panel);

        RectTransform cardRect = GetOrCreateChild(panelRect, "PanelCard");
        GameObject card = cardRect.gameObject;
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(640f, 0f);
        cardRect.anchoredPosition = Vector2.zero;

        UIStyle.RoundedImage(card, UIStyle.PanelBackground);

        // childControlHeight + a ContentSizeFitter below let each child (and the card itself)
        // size to its own real rendered content instead of hand-guessed pixel heights, so the
        // card always fits however many lines the text actually wraps to.
        VerticalLayoutGroup cardLayout = GetOrAddComponent<VerticalLayoutGroup>(card);
        cardLayout.padding = new RectOffset(24, 24, 20, 20);
        cardLayout.spacing = 10f;
        cardLayout.childAlignment = TextAnchor.UpperCenter;
        cardLayout.childControlWidth = true;
        cardLayout.childControlHeight = true;
        cardLayout.childForceExpandWidth = true;
        cardLayout.childForceExpandHeight = false;

        ContentSizeFitter cardFitter = GetOrAddComponent<ContentSizeFitter>(card);
        cardFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        cardFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // --- Title ---
        RectTransform titleRect = GetOrCreateChild(cardRect, "TitleText");
        TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleRect.gameObject);
        titleText.text = "Welcome!";
        UIStyle.ApplyText(titleText, 22f, UIStyle.Accent, TextAlignmentOptions.Center, FontStyles.Bold);

        // --- Premise ---
        RectTransform premiseRect = GetOrCreateChild(cardRect, "PremiseText");
        TextMeshProUGUI premiseText = GetOrAddComponent<TextMeshProUGUI>(premiseRect.gameObject);
        premiseText.text = "Explore the world, collect resources from trees and rocks, and spend the points you earn to upgrade your speed, gather range, and unlock new abilities.";
        premiseText.enableWordWrapping = true;
        UIStyle.ApplyText(premiseText, 14f, UIStyle.TextPrimary, TextAlignmentOptions.Center);

        // --- Controls section: header + two side-by-side key/action columns ---
        CreateSectionHeader(cardRect, "ControlsHeader", "CONTROLS");

        RectTransform controlsRowRect = GetOrCreateChild(cardRect, "ControlsRow");
        HorizontalLayoutGroup controlsRowGroup = GetOrAddComponent<HorizontalLayoutGroup>(controlsRowRect.gameObject);
        controlsRowGroup.padding = new RectOffset(0, 0, 0, 0);
        controlsRowGroup.spacing = 24f;
        controlsRowGroup.childAlignment = TextAnchor.UpperLeft;
        controlsRowGroup.childControlWidth = true;
        controlsRowGroup.childControlHeight = true;
        controlsRowGroup.childForceExpandWidth = true;
        controlsRowGroup.childForceExpandHeight = false;

        CreateControlsColumn(controlsRowRect, "ControlsLeft",
            "WASD  —  Move\nMouse  —  Look\nSpace  —  Jump\nShift  —  Sprint");
        CreateControlsColumn(controlsRowRect, "ControlsRight",
            "Z  —  Zoom Camera\nE  —  Interact / Gather\nM  —  Upgrade Menu");

        // --- Prompt ---
        RectTransform promptRect = GetOrCreateChild(cardRect, "PromptText");
        TextMeshProUGUI promptText = GetOrAddComponent<TextMeshProUGUI>(promptRect.gameObject);
        promptText.text = "Press ENTER to begin";
        UIStyle.ApplyText(promptText, 16f, UIStyle.Accent, TextAlignmentOptions.Center, FontStyles.Bold);

        // Layout groups only recompute lazily (next dirty pass); force it now so the card is
        // already the right height in the Editor without needing to enter Play mode first.
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardRect);

        SerializedObject serializedIntro = new SerializedObject(introUI);
        serializedIntro.FindProperty("controller").objectReferenceValue = controller;
        serializedIntro.FindProperty("starterInputs").objectReferenceValue = starterInputs;
        serializedIntro.FindProperty("playerInput").objectReferenceValue = playerInput;
        serializedIntro.ApplyModifiedProperties();

        // Draw on top of any other UI created earlier (ability bar, upgrade menu, ...).
        panelRect.SetAsLastSibling();
        panel.SetActive(true);

        EditorUtility.SetDirty(introUI);
        EditorSceneManager.MarkSceneDirty(playerObject.scene);

        Debug.Log("Intro screen setup complete. It will show automatically when the scene starts; press Enter in Play mode to dismiss it.");
    }

    private void CreateControlsColumn(Transform rowParent, string name, string text) {
        RectTransform columnRect = GetOrCreateChild(rowParent, name);
        TextMeshProUGUI column = GetOrAddComponent<TextMeshProUGUI>(columnRect.gameObject);
        column.text = text;
        column.enableWordWrapping = false;
        UIStyle.ApplyText(column, 13f, UIStyle.TextPrimary, TextAlignmentOptions.TopLeft);
    }

    private void CreateSectionHeader(Transform parent, string name, string label) {
        RectTransform headerRect = GetOrCreateChild(parent, name);
        LayoutElement headerLayoutElement = GetOrAddComponent<LayoutElement>(headerRect.gameObject);
        headerLayoutElement.minHeight = 20f;
        headerLayoutElement.preferredHeight = 20f;

        VerticalLayoutGroup headerLayout = GetOrAddComponent<VerticalLayoutGroup>(headerRect.gameObject);
        headerLayout.padding = new RectOffset(2, 2, 0, 0);
        headerLayout.spacing = 2f;
        headerLayout.childControlWidth = true;
        // Must be true - otherwise the Text/Divider children below keep Unity's default 100x100
        // RectTransform height instead of the 15/2 set via LayoutElement, and the Divider renders
        // as a tall light-gray box instead of a thin line.
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = true;
        headerLayout.childForceExpandHeight = false;

        RectTransform textRect = GetOrCreateChild(headerRect, "Text");
        LayoutElement textLayoutElement = GetOrAddComponent<LayoutElement>(textRect.gameObject);
        textLayoutElement.minHeight = 15f;
        textLayoutElement.preferredHeight = 15f;

        TextMeshProUGUI headerText = GetOrAddComponent<TextMeshProUGUI>(textRect.gameObject);
        UIStyle.ApplyText(headerText, 13f, UIStyle.Accent, TextAlignmentOptions.Left, FontStyles.Bold);
        headerText.text = label;

        RectTransform dividerRect = GetOrCreateChild(headerRect, "Divider");
        LayoutElement dividerLayoutElement = GetOrAddComponent<LayoutElement>(dividerRect.gameObject);
        dividerLayoutElement.minHeight = 2f;
        dividerLayoutElement.preferredHeight = 2f;
        UIStyle.RoundedImage(dividerRect.gameObject, UIStyle.Divider);
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
