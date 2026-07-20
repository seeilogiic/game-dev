using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class ResourceProgressBarSetupTool : EditorWindow
{
    private Canvas targetCanvas;

    [MenuItem("Tools/UI/Setup Resource Progress Bar")]
    public static void ShowWindow() {
        GetWindow<ResourceProgressBarSetupTool>("Resource Progress Bar Setup");
    }

    private void OnEnable() {
        if (targetCanvas == null) {
            targetCanvas = FindObjectOfType<Canvas>();
        }
    }

    private void OnGUI() {
        GUILayout.Label("Setup top-left overall resource progress bar", EditorStyles.boldLabel);

        targetCanvas = (Canvas)EditorGUILayout.ObjectField(
            "Target Canvas",
            targetCanvas,
            typeof(Canvas),
            true
        );

        if (targetCanvas == null) {
            EditorGUILayout.HelpBox("No Canvas assigned. Leave this empty and a Canvas (and EventSystem, if needed) will be created automatically.", MessageType.Info);
        }

        if (GUILayout.Button("Setup Resource Progress Bar")) {
            SetupProgressBar();
        }
    }

    private void SetupProgressBar() {
        Canvas canvas = GetOrCreateCanvas();
        targetCanvas = canvas;

        ResourceCounter resourceCounter = FindObjectOfType<ResourceCounter>();
        if (resourceCounter == null) {
            Debug.LogError("Could not find a ResourceCounter component in the scene. Make sure the resource UI is present and the scene is loaded.");
            return;
        }

        RectTransform barRect = GetOrCreateChild(canvas.transform, "ResourceProgressBar");
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(0f, 1f);
        barRect.pivot = new Vector2(0f, 1f);
        barRect.sizeDelta = new Vector2(260f, 24f);
        barRect.anchoredPosition = new Vector2(20f, -20f);

        RectTransform trackRect = GetOrCreateChild(barRect, "Track");
        trackRect.anchorMin = Vector2.zero;
        trackRect.anchorMax = Vector2.one;
        trackRect.sizeDelta = Vector2.zero;
        trackRect.anchoredPosition = Vector2.zero;

        Image trackImage = GetOrAddComponent<Image>(trackRect.gameObject);
        trackImage.color = new Color(0f, 0f, 0f, 0.4f);

        RectTransform fillRect = GetOrCreateChild(barRect, "Fill");
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;

        // A plain Image with no sprite ignores fillAmount/Type entirely and just draws a
        // full rectangle, so this must have a sprite assigned for the horizontal wipe to
        // actually work (same reasoning as the radial fills in AbilityBarSetupTool.cs).
        Image fillImage = GetOrAddComponent<Image>(fillRect.gameObject);
        fillImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.color = new Color(0.3f, 0.85f, 0.4f);
        fillImage.fillAmount = 0f;

        RectTransform percentRect = GetOrCreateChild(barRect, "PercentText");
        percentRect.anchorMin = Vector2.zero;
        percentRect.anchorMax = Vector2.one;
        percentRect.sizeDelta = Vector2.zero;
        percentRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI percentText = GetOrAddComponent<TextMeshProUGUI>(percentRect.gameObject);
        percentText.fontSize = 14;
        percentText.fontStyle = FontStyles.Bold;
        percentText.color = Color.white;
        percentText.alignment = TextAlignmentOptions.Center;

        SerializedObject serializedCounter = new SerializedObject(resourceCounter);
        serializedCounter.FindProperty("progressFillImage").objectReferenceValue = fillImage;
        serializedCounter.FindProperty("progressPercentText").objectReferenceValue = percentText;
        serializedCounter.ApplyModifiedProperties();

        EditorUtility.SetDirty(resourceCounter);
        EditorSceneManager.MarkSceneDirty(resourceCounter.gameObject.scene);

        Debug.Log("Resource progress bar setup complete: top-left bar wired to ResourceCounter (fill + percent text update automatically as resources are gathered). Tune colors/size/position to taste, then save the scene.");
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
