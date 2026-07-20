using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

// Shared visual theme for the editor UI setup tools (UpgradeMenuSetupTool, AbilityBarSetupTool, ...)
// so menus and HUD elements built by different tools still look like one cohesive design.
// Editor-only: lives under Assets/Scripts/Editor/ and is not compiled into player builds.
public static class UIStyle
{
    private const string RobotoFontPath = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset";

    public static readonly Color PanelBackground = new Color(0.078f, 0.086f, 0.109f, 0.92f);   // #14161C @ 0.92
    public static readonly Color RowBackground = new Color(1f, 1f, 1f, 0.05f);
    public static readonly Color Divider = new Color(1f, 1f, 1f, 0.14f);
    public static readonly Color Accent = new Color(1f, 0.82f, 0.302f, 1f);                     // #FFD24D, matches ResourceHighlightRing gold
    public static readonly Color TextPrimary = new Color(0.949f, 0.953f, 0.961f, 1f);            // #F2F3F5
    public static readonly Color TextSecondary = new Color(0.663f, 0.690f, 0.741f, 1f);          // #A9B0BD
    public static readonly Color ButtonNormal = new Color(1f, 0.82f, 0.302f, 0.18f);
    public static readonly Color ButtonHighlighted = new Color(1f, 0.82f, 0.302f, 0.32f);
    public static readonly Color ButtonPressed = new Color(1f, 0.82f, 0.302f, 0.5f);
    public static readonly Color ButtonDisabled = new Color(1f, 1f, 1f, 0.06f);

    private static TMP_FontAsset _font;
    public static TMP_FontAsset Font {
        get {
            if (_font == null) {
                _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RobotoFontPath);
            }
            return _font;
        }
    }

    // Loads a builtin rounded-corner sprite usable with Image.Type.Sliced.
    public static Sprite RoundedSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

    public static Sprite RadialKnobSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

    // Applies a rounded, sliced background image to the given GameObject.
    public static Image RoundedImage(GameObject go, Color color) {
        Image image = go.GetComponent<Image>();
        if (image == null) {
            image = go.AddComponent<Image>();
        }
        image.sprite = RoundedSprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        return image;
    }

    public static void ApplyText(TextMeshProUGUI tmp, float fontSize, Color color, TextAlignmentOptions alignment, FontStyles style = FontStyles.Normal) {
        if (Font != null) {
            tmp.font = Font;
        }
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = style;
    }

    // Styles a Button's target graphic + color states so hover/press/disabled are visually distinct.
    // The Image itself stays white; the ColorBlock states carry the actual theme tints (standard
    // Unity "Color Tint" transition), so RefreshUI()'s button.interactable toggling is respected.
    public static void StyleButton(Button button, Image targetImage) {
        targetImage.sprite = RoundedSprite;
        targetImage.type = Image.Type.Sliced;
        targetImage.color = Color.white;
        button.targetGraphic = targetImage;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = ButtonNormal;
        colors.highlightedColor = ButtonHighlighted;
        colors.pressedColor = ButtonPressed;
        colors.selectedColor = ButtonNormal;
        colors.disabledColor = ButtonDisabled;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
    }
}
