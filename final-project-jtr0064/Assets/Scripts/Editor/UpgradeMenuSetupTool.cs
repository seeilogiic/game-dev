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

        if (controller == null || starterInputs == null) {
            Debug.LogError("Player object is missing ThirdPersonController or StarterAssetsInputs. Cannot wire up the upgrade menu.");
            return;
        }

        PlayerUpgrades playerUpgrades = GetOrAddComponent<PlayerUpgrades>(playerObject);
        PlayerPoints playerPoints = GetOrAddComponent<PlayerPoints>(playerObject);

        // The panel hierarchy changed (flat absolute rows -> sectioned layout-group rows), so
        // rebuild it fresh each time rather than trying to patch the old structure in place.
        Transform existingPanel = canvas.transform.Find("UpgradeMenuPanel");
        if (existingPanel != null) {
            Undo.DestroyObjectImmediate(existingPanel.gameObject);
        }

        RectTransform panelRect = GetOrCreateChild(canvas.transform, "UpgradeMenuPanel");
        GameObject panel = panelRect.gameObject;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        // Wider than tall so it fits comfortably in a short/wide Game view (Free Aspect)
        // instead of running off the bottom of the screen.
        panelRect.sizeDelta = new Vector2(600f, 365f);
        panelRect.anchoredPosition = Vector2.zero;

        UIStyle.RoundedImage(panel, UIStyle.PanelBackground);

        UpgradeMenuUI menuUI = GetOrAddComponent<UpgradeMenuUI>(panel);

        VerticalLayoutGroup panelLayout = GetOrAddComponent<VerticalLayoutGroup>(panel);
        panelLayout.padding = new RectOffset(18, 18, 14, 14);
        panelLayout.spacing = 8f;
        panelLayout.childAlignment = TextAnchor.UpperCenter;
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = false;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childForceExpandHeight = false;

        // --- Top bar: title + points pill ---
        RectTransform titleRect = GetOrCreateChild(panelRect, "TitleText");
        LayoutElement titleLayout = GetOrAddComponent<LayoutElement>(titleRect.gameObject);
        titleLayout.minHeight = 24f;
        titleLayout.preferredHeight = 24f;

        TextMeshProUGUI titleText = GetOrAddComponent<TextMeshProUGUI>(titleRect.gameObject);
        titleText.text = "Upgrades & Abilities";
        UIStyle.ApplyText(titleText, 18f, UIStyle.Accent, TextAlignmentOptions.Center, FontStyles.Bold);

        RectTransform pointsRect = GetOrCreateChild(panelRect, "PointsLabel");
        LayoutElement pointsLayout = GetOrAddComponent<LayoutElement>(pointsRect.gameObject);
        pointsLayout.minHeight = 18f;
        pointsLayout.preferredHeight = 18f;

        TextMeshProUGUI pointsLabel = GetOrAddComponent<TextMeshProUGUI>(pointsRect.gameObject);
        UIStyle.ApplyText(pointsLabel, 13f, UIStyle.Accent, TextAlignmentOptions.Center);

        // --- Upgrades section: header + a row of cards, one per upgrade ---
        CreateSectionHeader(panelRect, "UpgradesHeader", "UPGRADES");

        RectTransform upgradesRow = CreateCardRow(panelRect, "UpgradesRow");
        TextMeshProUGUI speedLabel = CreateCard(upgradesRow, "Speed", "Upgrade Speed", out Button speedButton);
        TextMeshProUGUI gatherLabel = CreateCard(upgradesRow, "Gather", "Upgrade Gather Distance", out Button gatherButton);
        TextMeshProUGUI gatherSpeedLabel = CreateCard(upgradesRow, "GatherSpeed", "Upgrade Gather Speed", out Button gatherSpeedButton);

        // --- Abilities section: header + a row of cards, one per ability ---
        CreateSectionHeader(panelRect, "AbilitiesHeader", "ABILITIES");

        RectTransform abilitiesRow = CreateCardRow(panelRect, "AbilitiesRow");
        TextMeshProUGUI autoCollectLabel = CreateCard(abilitiesRow, "AutoCollect", "Unlock Auto-Collect", out Button autoCollectButton);
        TextMeshProUGUI highlightLabel = CreateCard(abilitiesRow, "Highlight", "Unlock Highlight", out Button highlightButton);

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

    private void CreateSectionHeader(Transform parent, string name, string label) {
        RectTransform headerRect = GetOrCreateChild(parent, name);
        LayoutElement headerLayoutElement = GetOrAddComponent<LayoutElement>(headerRect.gameObject);
        headerLayoutElement.minHeight = 20f;
        headerLayoutElement.preferredHeight = 20f;

        VerticalLayoutGroup headerLayout = GetOrAddComponent<VerticalLayoutGroup>(headerRect.gameObject);
        headerLayout.padding = new RectOffset(2, 2, 3, 0);
        headerLayout.spacing = 2f;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = false;
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

    // Creates the horizontal strip that a section's cards sit in, side by side.
    private RectTransform CreateCardRow(Transform parent, string name) {
        RectTransform rowRect = GetOrCreateChild(parent, name);
        LayoutElement rowLayoutElement = GetOrAddComponent<LayoutElement>(rowRect.gameObject);
        rowLayoutElement.minHeight = 100f;
        rowLayoutElement.preferredHeight = 100f;

        HorizontalLayoutGroup rowLayout = GetOrAddComponent<HorizontalLayoutGroup>(rowRect.gameObject);
        rowLayout.padding = new RectOffset(0, 0, 0, 0);
        rowLayout.spacing = 10f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = true;

        return rowRect;
    }

    // Creates one upgrade/ability card: a fixed-width rounded tile with the stat label on top
    // (word-wrapped, grows to fill available space) and the action button pinned at the
    // bottom. Cards sit side by side in a CreateCardRow container instead of stacking, so a
    // whole section only costs one row of vertical space no matter how many items it has.
    private TextMeshProUGUI CreateCard(Transform rowParent, string baseName, string buttonLabel, out Button button) {
        RectTransform cardRect = GetOrCreateChild(rowParent, baseName + "Card");
        LayoutElement cardLayoutElement = GetOrAddComponent<LayoutElement>(cardRect.gameObject);
        cardLayoutElement.minWidth = 178f;
        cardLayoutElement.preferredWidth = 178f;
        cardLayoutElement.flexibleWidth = 0f;

        UIStyle.RoundedImage(cardRect.gameObject, UIStyle.RowBackground);

        VerticalLayoutGroup cardLayout = GetOrAddComponent<VerticalLayoutGroup>(cardRect.gameObject);
        cardLayout.padding = new RectOffset(8, 8, 6, 6);
        cardLayout.spacing = 4f;
        cardLayout.childAlignment = TextAnchor.UpperCenter;
        cardLayout.childControlWidth = true;
        cardLayout.childControlHeight = false;
        cardLayout.childForceExpandWidth = true;
        cardLayout.childForceExpandHeight = false;

        RectTransform labelRect = GetOrCreateChild(cardRect, baseName + "Label");
        LayoutElement labelLayoutElement = GetOrAddComponent<LayoutElement>(labelRect.gameObject);
        labelLayoutElement.minHeight = 50f;
        labelLayoutElement.flexibleHeight = 1f;

        TextMeshProUGUI label = GetOrAddComponent<TextMeshProUGUI>(labelRect.gameObject);
        UIStyle.ApplyText(label, 11f, UIStyle.TextPrimary, TextAlignmentOptions.Left);
        label.enableWordWrapping = true;

        RectTransform buttonRect = GetOrCreateChild(cardRect, baseName + "Button");
        LayoutElement buttonLayoutElement = GetOrAddComponent<LayoutElement>(buttonRect.gameObject);
        buttonLayoutElement.minHeight = 26f;
        buttonLayoutElement.preferredHeight = 26f;
        buttonLayoutElement.flexibleHeight = 0f;

        Image buttonImage = GetOrAddComponent<Image>(buttonRect.gameObject);
        button = GetOrAddComponent<Button>(buttonRect.gameObject);
        UIStyle.StyleButton(button, buttonImage);

        RectTransform textRect = GetOrCreateChild(buttonRect, "Text");
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI buttonText = GetOrAddComponent<TextMeshProUGUI>(textRect.gameObject);
        buttonText.text = buttonLabel;
        UIStyle.ApplyText(buttonText, 11f, UIStyle.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
        buttonText.enableWordWrapping = true;

        return label;
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
