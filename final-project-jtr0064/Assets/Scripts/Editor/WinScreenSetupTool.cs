using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using StarterAssets;

public class WinScreenSetupTool : EditorWindow
{
    private Canvas targetCanvas;

    [MenuItem("Tools/UI/Setup Win Screen")]
    public static void ShowWindow() {
        GetWindow<WinScreenSetupTool>("Win Screen Setup");
    }

    private void OnEnable() {
        if (targetCanvas == null) {
            targetCanvas = FindObjectOfType<Canvas>();
        }
    }

    private void OnGUI() {
        GUILayout.Label("Setup Win Screen (shown when all resources are deposited)", EditorStyles.boldLabel);

        targetCanvas = (Canvas)EditorGUILayout.ObjectField(
            "Target Canvas",
            targetCanvas,
            typeof(Canvas),
            true
        );

        if (targetCanvas == null) {
            EditorGUILayout.HelpBox("No Canvas assigned. Leave this empty and a Canvas (and EventSystem, if needed) will be created automatically.", MessageType.Info);
        }

        if (GUILayout.Button("Setup Win Screen")) {
            SetupWinScreen();
        }
    }

    private void SetupWinScreen() {
        Canvas canvas = GetOrCreateCanvas();
        targetCanvas = canvas;

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
            Debug.LogError("Player object is missing ThirdPersonController, StarterAssetsInputs, or PlayerInput. Cannot wire up the win screen.");
            return;
        }

        ResourceCounter resourceCounter = FindObjectOfType<ResourceCounter>();
        if (resourceCounter == null) {
            Debug.LogError("Could not find a ResourceCounter in the scene. Run \"Tools/UI/Setup Resource Progress Bar\" first (or add one manually), then re-run this tool.");
            return;
        }

        // Rebuild fresh each time rather than patch in place, same as the other UI setup tools.
        Transform existingPanel = canvas.transform.Find("WinScreenPanel");
        if (existingPanel != null) {
            Undo.DestroyObjectImmediate(existingPanel.gameObject);
        }

        RectTransform panelRect = GetOrCreateChild(canvas.transform, "WinScreenPanel");
        GameObject panel = panelRect.gameObject;
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // Full-bleed dark backdrop so the game world reads as blocked, not just dimmed.
        UIStyle.RoundedImage(panel, new Color(0f, 0f, 0f, 0.85f));

        WinScreenUI winUI = GetOrAddComponent<WinScreenUI>(panel);

        RectTransform cardRect = GetOrCreateChild(panelRect, "PanelCard");
        GameObject card = cardRect.gameObject;
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(480f, 0f);
        cardRect.anchoredPosition = Vector2.zero;

        UIStyle.RoundedImage(card, UIStyle.PanelBackground);

        // childControlHeight + a ContentSizeFitter below let each child (and the card itself)
        // size to its own real rendered content, same reasoning as IntroScreenSetupTool.
        VerticalLayoutGroup cardLayout = GetOrAddComponent<VerticalLayoutGroup>(card);
        cardLayout.padding = new RectOffset(24, 24, 20, 20);
        cardLayout.spacing = 14f;
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
        titleText.text = "You Win!";
        UIStyle.ApplyText(titleText, 28f, UIStyle.Accent, TextAlignmentOptions.Center, FontStyles.Bold);

        // --- Subtitle ---
        RectTransform subtitleRect = GetOrCreateChild(cardRect, "SubtitleText");
        TextMeshProUGUI subtitleText = GetOrAddComponent<TextMeshProUGUI>(subtitleRect.gameObject);
        subtitleText.text = "You collected and deposited every resource.";
        subtitleText.enableWordWrapping = true;
        UIStyle.ApplyText(subtitleText, 15f, UIStyle.TextPrimary, TextAlignmentOptions.Center);

        // --- Restart button ---
        RectTransform buttonRect = GetOrCreateChild(cardRect, "RestartButton");
        LayoutElement buttonLayoutElement = GetOrAddComponent<LayoutElement>(buttonRect.gameObject);
        buttonLayoutElement.minHeight = 40f;
        buttonLayoutElement.preferredHeight = 40f;
        buttonLayoutElement.flexibleHeight = 0f;

        Image buttonImage = GetOrAddComponent<Image>(buttonRect.gameObject);
        Button restartButton = GetOrAddComponent<Button>(buttonRect.gameObject);
        UIStyle.StyleButton(restartButton, buttonImage);

        RectTransform buttonTextRect = GetOrCreateChild(buttonRect, "Text");
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI buttonText = GetOrAddComponent<TextMeshProUGUI>(buttonTextRect.gameObject);
        buttonText.text = "Restart";
        UIStyle.ApplyText(buttonText, 16f, UIStyle.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);

        // Layout groups only recompute lazily (next dirty pass); force it now so the card is
        // already the right height in the Editor without needing to enter Play mode first.
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardRect);

        SerializedObject serializedWin = new SerializedObject(winUI);
        serializedWin.FindProperty("restartButton").objectReferenceValue = restartButton;
        serializedWin.FindProperty("controller").objectReferenceValue = controller;
        serializedWin.FindProperty("starterInputs").objectReferenceValue = starterInputs;
        serializedWin.FindProperty("playerInput").objectReferenceValue = playerInput;
        serializedWin.ApplyModifiedProperties();

        SerializedObject serializedCounter = new SerializedObject(resourceCounter);
        serializedCounter.FindProperty("winScreenUI").objectReferenceValue = winUI;
        serializedCounter.ApplyModifiedProperties();

        // Draw on top of any other UI created earlier (ability bar, upgrade menu, intro, ...).
        panelRect.SetAsLastSibling();
        panel.SetActive(false);

        EditorUtility.SetDirty(winUI);
        EditorUtility.SetDirty(resourceCounter);
        EditorSceneManager.MarkSceneDirty(playerObject.scene);

        Debug.Log("Win screen setup complete. It will show automatically once every resource has been collected and deposited.");
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
