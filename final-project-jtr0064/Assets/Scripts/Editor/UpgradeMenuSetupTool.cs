using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using StarterAssets;

public class UpgradeMenuSetupTool : EditorWindow
{
    private Canvas targetCanvas;

    [MenuItem("Tools/UI/Setup Upgrade Menu")]
    public static void ShowWindow() {
        GetWindow<UpgradeMenuSetupTool>("Upgrade Menu Setup");
    }

    private void OnEnable() {
        if (targetCanvas == null) {
            targetCanvas = FindObjectOfType<Canvas>();
        }
    }

    private void OnGUI() {
        GUILayout.Label("Setup Upgrade Menu (M key)", EditorStyles.boldLabel);

        targetCanvas = (Canvas)EditorGUILayout.ObjectField(
            "Target Canvas",
            targetCanvas,
            typeof(Canvas),
            true
        );

        if (targetCanvas == null) {
            EditorGUILayout.HelpBox("No Canvas assigned. Leave this empty and a Canvas (and EventSystem, if needed) will be created automatically.", MessageType.Info);
        }

        if (GUILayout.Button("Setup Upgrade Menu")) {
            SetupUpgradeMenu();
        }
    }

    private void SetupUpgradeMenu() {
        Canvas canvas = GetOrCreateCanvas();
        targetCanvas = canvas;

        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction == null) {
            Debug.LogError("Could not find a PlayerInteraction component in the scene. Make sure the player is present and the scene is loaded.");
            return;
        }

        GameObject playerObject = playerInteraction.gameObject;
        ThirdPersonController controller = playerObject.GetComponent<ThirdPersonController>();
        StarterAssetsInputs starterInputs = playerObject.GetComponent<StarterAssetsInputs>();

        if (controller == null || starterInputs == null) {
            Debug.LogError("Player object is missing ThirdPersonController or StarterAssetsInputs. Cannot wire up the upgrade menu.");
            return;
        }

        PlayerUpgrades playerUpgrades = GetOrAddComponent<PlayerUpgrades>(playerObject);
        PlayerPoints playerPoints = GetOrAddComponent<PlayerPoints>(playerObject);

        RectTransform panelRect = GetOrCreateChild(canvas.transform, "UpgradeMenuPanel");
        GameObject panel = panelRect.gameObject;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(360f, 410f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = GetOrAddComponent<Image>(panel);
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);

        UpgradeMenuUI menuUI = GetOrAddComponent<UpgradeMenuUI>(panel);

        RectTransform titleRect = GetOrCreateChild(panelRect, "TitleText");
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(320f, 30f);
        titleRect.anchoredPosition = new Vector2(0f, -15f);

        TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleRect.gameObject);
        titleText.text = "Upgrades";
        titleText.fontSize = 22;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        RectTransform pointsRect = GetOrCreateChild(panelRect, "PointsLabel");
        pointsRect.anchorMin = new Vector2(0.5f, 1f);
        pointsRect.anchorMax = new Vector2(0.5f, 1f);
        pointsRect.pivot = new Vector2(0.5f, 1f);
        pointsRect.sizeDelta = new Vector2(320f, 25f);
        pointsRect.anchoredPosition = new Vector2(0f, -50f);

        TextMeshProUGUI pointsLabel = GetOrAddComponent<TextMeshProUGUI>(pointsRect.gameObject);
        pointsLabel.fontSize = 16;
        pointsLabel.alignment = TextAlignmentOptions.Center;
        pointsLabel.color = Color.white;

        TextMeshProUGUI speedLabel = CreateLabel(panelRect, "SpeedLabel", new Vector2(-85f, 20f));
        Button speedButton = CreateButton(panelRect, "SpeedButton", "Upgrade Speed", new Vector2(110f, 20f));

        TextMeshProUGUI gatherLabel = CreateLabel(panelRect, "GatherLabel", new Vector2(-85f, -30f));
        Button gatherButton = CreateButton(panelRect, "GatherButton", "Upgrade Gather Distance", new Vector2(110f, -30f));

        TextMeshProUGUI gatherSpeedLabel = CreateLabel(panelRect, "GatherSpeedLabel", new Vector2(-85f, -80f));
        Button gatherSpeedButton = CreateButton(panelRect, "GatherSpeedButton", "Upgrade Gather Speed", new Vector2(110f, -80f));

        TextMeshProUGUI autoCollectLabel = CreateLabel(panelRect, "AutoCollectLabel", new Vector2(-85f, -130f));
        Button autoCollectButton = CreateButton(panelRect, "AutoCollectButton", "Unlock Auto-Collect", new Vector2(110f, -130f));

        TextMeshProUGUI highlightLabel = CreateLabel(panelRect, "HighlightLabel", new Vector2(-85f, -180f));
        Button highlightButton = CreateButton(panelRect, "HighlightButton", "Unlock Highlight", new Vector2(110f, -180f));

        SerializedObject serializedMenu = new SerializedObject(menuUI);
        serializedMenu.FindProperty("panelRoot").objectReferenceValue = panel;
        serializedMenu.FindProperty("pointsLabel").objectReferenceValue = pointsLabel;
        serializedMenu.FindProperty("speedLabel").objectReferenceValue = speedLabel;
        serializedMenu.FindProperty("gatherLabel").objectReferenceValue = gatherLabel;
        serializedMenu.FindProperty("gatherSpeedLabel").objectReferenceValue = gatherSpeedLabel;
        serializedMenu.FindProperty("autoCollectLabel").objectReferenceValue = autoCollectLabel;
        serializedMenu.FindProperty("highlightLabel").objectReferenceValue = highlightLabel;
        serializedMenu.FindProperty("upgradeSpeedButton").objectReferenceValue = speedButton;
        serializedMenu.FindProperty("upgradeGatherButton").objectReferenceValue = gatherButton;
        serializedMenu.FindProperty("upgradeGatherSpeedButton").objectReferenceValue = gatherSpeedButton;
        serializedMenu.FindProperty("unlockAutoCollectButton").objectReferenceValue = autoCollectButton;
        serializedMenu.FindProperty("unlockHighlightButton").objectReferenceValue = highlightButton;
        serializedMenu.FindProperty("playerUpgrades").objectReferenceValue = playerUpgrades;
        serializedMenu.FindProperty("playerPoints").objectReferenceValue = playerPoints;
        serializedMenu.FindProperty("controller").objectReferenceValue = controller;
        serializedMenu.FindProperty("starterInputs").objectReferenceValue = starterInputs;
        serializedMenu.ApplyModifiedProperties();

        panel.SetActive(false);

        EditorUtility.SetDirty(menuUI);
        EditorUtility.SetDirty(playerUpgrades);
        EditorUtility.SetDirty(playerPoints);
        EditorSceneManager.MarkSceneDirty(playerObject.scene);

        Debug.Log("Upgrade menu setup complete. Press M in Play mode to open it.");
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

    private TextMeshProUGUI CreateLabel(Transform parent, string name, Vector2 anchoredPosition) {
        RectTransform rect = GetOrCreateChild(parent, name);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(180f, 30f);
        rect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI label = GetOrAddComponent<TextMeshProUGUI>(rect.gameObject);
        label.fontSize = 16;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Left;
        return label;
    }

    private Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition) {
        RectTransform rect = GetOrCreateChild(parent, name);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(140f, 36f);
        rect.anchoredPosition = anchoredPosition;

        Image buttonImage = GetOrAddComponent<Image>(rect.gameObject);
        buttonImage.color = new Color(1f, 1f, 1f, 0.15f);

        Button button = GetOrAddComponent<Button>(rect.gameObject);
        button.targetGraphic = buttonImage;

        RectTransform textRect = GetOrCreateChild(rect, "Text");
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI buttonText = GetOrAddComponent<TextMeshProUGUI>(textRect.gameObject);
        buttonText.text = label;
        buttonText.fontSize = 13;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.enableWordWrapping = true;

        return button;
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
