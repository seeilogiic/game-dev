using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class GatheringUISetupTool : EditorWindow
{
    private Canvas targetCanvas;

    [MenuItem("Tools/UI/Setup Gathering UI")]
    public static void ShowWindow() {
        GetWindow<GatheringUISetupTool>("Gathering UI Setup");
    }

    private void OnEnable() {
        if (targetCanvas == null) {
            targetCanvas = FindObjectOfType<Canvas>();
        }
    }

    private void OnGUI() {
        GUILayout.Label("Setup Gathering UI (timer + E prompt + M hint)", EditorStyles.boldLabel);

        targetCanvas = (Canvas)EditorGUILayout.ObjectField(
            "Target Canvas",
            targetCanvas,
            typeof(Canvas),
            true
        );

        if (targetCanvas == null) {
            EditorGUILayout.HelpBox("No Canvas assigned. Leave this empty and a Canvas (and EventSystem, if needed) will be created automatically.", MessageType.Info);
        }

        if (GUILayout.Button("Setup Gathering UI")) {
            SetupGatheringUI();
        }
    }

    private void SetupGatheringUI() {
        Canvas canvas = GetOrCreateCanvas();
        targetCanvas = canvas;

        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction == null) {
            Debug.LogError("Could not find a PlayerInteraction component in the scene. Make sure the player is present and the scene is loaded.");
            return;
        }

        // Screen-center radial gather timer.
        RectTransform progressRect = GetOrCreateChild(canvas.transform, "GatherProgress");
        progressRect.anchorMin = new Vector2(0.5f, 0.5f);
        progressRect.anchorMax = new Vector2(0.5f, 0.5f);
        progressRect.pivot = new Vector2(0.5f, 0.5f);
        progressRect.sizeDelta = new Vector2(96f, 96f);
        progressRect.anchoredPosition = Vector2.zero;

        Image gatherProgressImage = GetOrAddComponent<Image>(progressRect.gameObject);
        gatherProgressImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        gatherProgressImage.type = Image.Type.Filled;
        gatherProgressImage.fillMethod = Image.FillMethod.Radial360;
        gatherProgressImage.fillOrigin = (int)Image.Origin360.Top;
        gatherProgressImage.fillClockwise = true;
        gatherProgressImage.fillAmount = 0f;
        gatherProgressImage.color = Color.white;
        progressRect.gameObject.SetActive(false);

        // Bottom-center "[E] gather {item}" prompt.
        RectTransform gatherPromptRect = GetOrCreateChild(canvas.transform, "GatherPrompt");
        gatherPromptRect.anchorMin = new Vector2(0.5f, 0f);
        gatherPromptRect.anchorMax = new Vector2(0.5f, 0f);
        gatherPromptRect.pivot = new Vector2(0.5f, 0f);
        gatherPromptRect.sizeDelta = new Vector2(260f, 40f);
        gatherPromptRect.anchoredPosition = new Vector2(0f, 40f);
        GameObject gatherPrompt = gatherPromptRect.gameObject;

        CreateKeycap(gatherPromptRect, "KeyE", "E", new Vector2(10f, 0f));

        RectTransform gatherLabelRect = GetOrCreateChild(gatherPromptRect, "GatherLabel");
        gatherLabelRect.anchorMin = new Vector2(0f, 0.5f);
        gatherLabelRect.anchorMax = new Vector2(0f, 0.5f);
        gatherLabelRect.pivot = new Vector2(0f, 0.5f);
        gatherLabelRect.sizeDelta = new Vector2(190f, 36f);
        gatherLabelRect.anchoredPosition = new Vector2(54f, 0f);

        TextMeshProUGUI gatherLabel = GetOrAddComponent<TextMeshProUGUI>(gatherLabelRect.gameObject);
        gatherLabel.fontSize = 18;
        gatherLabel.color = Color.white;
        gatherLabel.alignment = TextAlignmentOptions.Left;

        gatherPrompt.SetActive(false);

        // Bottom-right "[M] Menu" hint (cosmetic - the M -> ToggleMenu binding already exists).
        RectTransform menuHintRect = GetOrCreateChild(canvas.transform, "MenuHint");
        menuHintRect.anchorMin = new Vector2(1f, 0f);
        menuHintRect.anchorMax = new Vector2(1f, 0f);
        menuHintRect.pivot = new Vector2(1f, 0f);
        menuHintRect.sizeDelta = new Vector2(140f, 40f);
        menuHintRect.anchoredPosition = new Vector2(-20f, 20f);

        CreateKeycap(menuHintRect, "KeyM", "M", new Vector2(10f, 0f));

        RectTransform menuLabelRect = GetOrCreateChild(menuHintRect, "MenuLabel");
        menuLabelRect.anchorMin = new Vector2(0f, 0.5f);
        menuLabelRect.anchorMax = new Vector2(0f, 0.5f);
        menuLabelRect.pivot = new Vector2(0f, 0.5f);
        menuLabelRect.sizeDelta = new Vector2(76f, 36f);
        menuLabelRect.anchoredPosition = new Vector2(54f, 0f);

        TextMeshProUGUI menuLabel = GetOrAddComponent<TextMeshProUGUI>(menuLabelRect.gameObject);
        menuLabel.text = "Menu";
        menuLabel.fontSize = 18;
        menuLabel.color = Color.white;
        menuLabel.alignment = TextAlignmentOptions.Left;

        // The old generic prompt (if present) would otherwise show alongside the new one.
        Transform oldPrompt = canvas.transform.Find("ResourcePromptText");
        if (oldPrompt != null) {
            oldPrompt.gameObject.SetActive(false);
        }

        SerializedObject serializedInteraction = new SerializedObject(playerInteraction);
        serializedInteraction.FindProperty("gatherProgressImage").objectReferenceValue = gatherProgressImage;
        serializedInteraction.FindProperty("promptText").objectReferenceValue = gatherLabel;
        serializedInteraction.FindProperty("promptRoot").objectReferenceValue = gatherPrompt;
        serializedInteraction.ApplyModifiedProperties();

        EditorUtility.SetDirty(playerInteraction);
        EditorSceneManager.MarkSceneDirty(playerInteraction.gameObject.scene);

        Debug.Log("Gathering UI setup complete: screen-center gather timer, bottom-center \"[E] gather {item}\" prompt, and bottom-right \"[M] Menu\" hint are wired up. Tune sprite/colors/positions to taste, then save the scene.");
    }

    private GameObject CreateKeycap(Transform parent, string name, string letter, Vector2 anchoredPosition) {
        RectTransform rect = GetOrCreateChild(parent, name);
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = new Vector2(36f, 36f);
        rect.anchoredPosition = anchoredPosition;

        Image keyImage = GetOrAddComponent<Image>(rect.gameObject);
        keyImage.color = new Color(1f, 1f, 1f, 0.15f);

        RectTransform letterRect = GetOrCreateChild(rect, "Letter");
        letterRect.anchorMin = Vector2.zero;
        letterRect.anchorMax = Vector2.one;
        letterRect.sizeDelta = Vector2.zero;
        letterRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI letterText = GetOrAddComponent<TextMeshProUGUI>(letterRect.gameObject);
        letterText.text = letter;
        letterText.fontSize = 20;
        letterText.fontStyle = FontStyles.Bold;
        letterText.color = Color.white;
        letterText.alignment = TextAlignmentOptions.Center;

        return rect.gameObject;
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
