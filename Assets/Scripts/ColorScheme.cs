using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Color Scheme", fileName = "New Color Scheme")]
public class ColorScheme : ScriptableObject {

    public enum EnumName {
        DefaultDark,
        DefaultLight    // TODO make this one
    }

    private static ColorScheme m_current;
    private static Dictionary<EnumName, ColorScheme> map;
    
    public static ColorScheme current {
        get {
            EnsureEverythingLoaded();
            return m_current;
        }
    }

    public static ColorScheme Get (EnumName enumName) {
        EnsureEverythingLoaded();
        if(map.ContainsKey(enumName)){
            return map[enumName];
        }
        Debug.LogError($"Didn't find a {nameof(ColorScheme)} in the map for key \"{enumName}\"! That's a problem!");
        return current;
    }

    private static void EnsureEverythingLoaded () {
        if(map == null){
            map = new Dictionary<EnumName, ColorScheme>();
            var inRAM = Resources.FindObjectsOfTypeAll<ColorScheme>();
            foreach(var cs in inRAM){
                if(!map.ContainsKey(cs.enumName)){
                    map.Add(cs.enumName, cs);
                }else{
                    Debug.LogError($"Duplicate enum name. Key of {cs.name} is already in map!");
                }
            }
        }
        if(m_current == null){
            m_current = map[ColorScheme.EnumName.DefaultDark];      // TODO in future, load the key from the playerprefs or whatever so the colourscheme can stick around and only load default dark in case of an error
        }
    }



    public static void SwitchTo (ColorScheme newColorScheme) {
        m_current = newColorScheme;
        onChange.Invoke(newColorScheme);
    }

    public static event System.Action<ColorScheme> onChange = delegate {};

    [SerializeField] EnumName m_enumName;
    public EnumName enumName => m_enumName;

    [Header("General Settings")]
    [SerializeField] Color applicationBackground;
    [SerializeField] Color screenBorders;
    [SerializeField] Color renderWireFloor;
    [SerializeField] Color renderXAxis;
    [SerializeField] Color renderYAxis;
    [SerializeField] Color renderZAxis;
    [SerializeField] Color windowOverlayLabel;
    [SerializeField] Color windowOverlayDropShadow;
    [SerializeField] Color windowOverlayButtonIcon;
    [SerializeField] Color windowOverlayButtonIconDisabled;
    [SerializeField] Color windowOverlayButtonBackground;
    [SerializeField] Color windowOverlayButtonBackgroundHover;
    [SerializeField] Color windowOverlayButtonBackgroundClick;
    [SerializeField] Color windowOverlayButtonBackgroundDisabled;

    public Color ApplicationBackground => applicationBackground;
    public Color ScreenBorders => screenBorders;
    public Color RenderWireFloor => renderWireFloor;
    public Color RenderXAxis => renderXAxis;
    public Color RenderYAxis => renderYAxis;
    public Color RenderZAxis => renderZAxis;
    public Color WindowOverlayLabel => windowOverlayLabel;
    public Color WindowOverlayDropShadow => windowOverlayDropShadow;
    public Color WindowOverlayButtonIcon => windowOverlayButtonIcon;
    public Color WindowOverlayButtonIconDisabled => windowOverlayButtonIconDisabled;
    public Color WindowOverlayButtonBackground => windowOverlayButtonBackground;
    public Color WindowOverlayButtonBackgroundHover => windowOverlayButtonBackgroundHover;
    public Color WindowOverlayButtonBackgroundClick => windowOverlayButtonBackgroundClick;
    public Color WindowOverlayButtonBackgroundDisabled => windowOverlayButtonBackgroundDisabled;

    [Header("Main Menu")]
    [SerializeField] Color mainMenuBackgroundWireObject;
    [SerializeField] Color mainMenuTitle;
    [SerializeField] Color mainMenuTitleDropShadow;
    [SerializeField] Color mainMenuMainButtons;
    [SerializeField] Color mainMenuMainButtonsHover;
    [SerializeField] Color mainMenuMainButtonsClick;
    [SerializeField] Color mainMenuMainButtonsOutline;
    [SerializeField] Color mainMenuMainButtonsText;
    [SerializeField] Color mainMenuMainButtonsTextDropShadow;
    [SerializeField] Color mainMenuDownloadButtonText;
    [SerializeField] Color mainMenuDownloadButtonTextHover;
    [SerializeField] Color mainMenuDownloadButtonTextClick;

    public Color MainMenuBackgroundWireObject => mainMenuBackgroundWireObject;
    public Color MainMenuTitle => mainMenuTitle;
    public Color MainMenuTitleDropShadow => mainMenuTitleDropShadow;
    public Color MainMenuMainButtons => mainMenuMainButtons;
    public Color MainMenuMainButtonsHover => mainMenuMainButtonsHover;
    public Color MainMenuMainButtonsClick => mainMenuMainButtonsClick;
    public Color MainMenuMainButtonsOutline => mainMenuMainButtonsOutline;
    public Color MainMenuMainButtonsText => mainMenuMainButtonsText;
    public Color MainMenuMainButtonsTextDropShadow => mainMenuMainButtonsTextDropShadow;
    public Color MainMenuDownloadButtonText => mainMenuDownloadButtonText;
    public Color MainMenuDownloadButtonTextHover => mainMenuDownloadButtonTextHover;
    public Color MainMenuDownloadButtonTextClick => mainMenuDownloadButtonTextClick;

    [Header("Vert Render View")]
    [SerializeField] Color vertRenderCameraFrustum;
    [SerializeField] Color vertRenderClipSpaceBox;
    [SerializeField] Color vertRenderPivot;
    [SerializeField] Color vertRenderPivotOutline;
    [SerializeField] Color vertRenderVectorPoint;
    [SerializeField] Color vertRenderVectorPointOutline;
    [SerializeField] Color vertRenderWireObject;

    public Color VertRenderCameraFrustum => vertRenderCameraFrustum;
    public Color VertRenderClipSpaceBox => vertRenderClipSpaceBox;
    public Color VertRenderPivot => vertRenderPivot;
    public Color VertRenderPivotOutline => vertRenderPivotOutline;
    public Color VertRenderVectorPoint => vertRenderVectorPoint;
    public Color VertRenderVectorPointOutline => vertRenderVectorPointOutline;
    public Color VertRenderWireObject => vertRenderWireObject;

    [Header("Vert Render View Object")]
    [SerializeField] Color vertRenderObjectColor;
    [SerializeField] Color vertRenderObjectBackfaceColor;
    [SerializeField] bool vertRenderBackfacesSolid;
    [SerializeField] bool vertRenderBackfacesLit;
    [SerializeField] Color vertRenderLight1;
    [SerializeField] Color vertRenderLight2;
    [SerializeField] Color vertRenderAmbientLight;
    [SerializeField] Color vertRenderClippingOverlay;
	
    public Color VertRenderObjectColor => vertRenderObjectColor;
    public Color VertRenderObjectBackfaceColor => vertRenderObjectBackfaceColor;
    public bool VertRenderBackfacesSolid => vertRenderBackfacesSolid;
    public bool VertRenderBackfacesLit => vertRenderBackfacesLit;
    public Color VertRenderLight1 => vertRenderLight1;
    public Color VertRenderLight2 => vertRenderLight2;
    public Color VertRenderAmbientLight => vertRenderAmbientLight;
    public Color VertRenderClippingOverlay => vertRenderClippingOverlay;

    [Header("Matrix Screen")]
    [SerializeField] Color matrixScreenMatrixGroupBackground;
    [SerializeField] Color matrixScreenModelMatrixHeader;
    [SerializeField] Color matrixScreenCameraMatrixHeader;
    [SerializeField] Color matrixScreenMultiplicationSign;

    public Color MatrixScreenMatrixGroupBackground => matrixScreenMatrixGroupBackground;
    public Color MatrixScreenModelMatrixHeader => matrixScreenModelMatrixHeader;
    public Color MatrixScreenCameraMatrixHeader => matrixScreenCameraMatrixHeader;
    public Color MatrixScreenMultiplicationSign => matrixScreenMultiplicationSign;

    [Header("Matrix Screen Center Bottom Popup")]
    [SerializeField] Color matrixScreenBottomAreaBackground;
    [SerializeField] Color matrixScreenBottomAreaOutline;
    [SerializeField] Color matrixScreenBottomAreaForegroundElement;
    [SerializeField] Color matrixScreenBottomAreaTextDropShadow;
    [SerializeField] Color matrixScreenBottomAreaDivider;
    [SerializeField] Color matrixScreenSliderBackground;
    [SerializeField] Color matrixScreenSliderFill;
    [SerializeField] Color matrixScreenSliderHandle;
    [SerializeField] Color matrixScreenSliderHandleHover;
    [SerializeField] Color matrixScreenSliderHandleClick;
    [SerializeField] Color matrixScreenBottomAreaToggle;
    [SerializeField] Color matrixScreenBottomAreaToggleHover;
    [SerializeField] Color matrixScreenBottomAreaToggleClick;

    public Color MatrixScreenBottomAreaBackground => matrixScreenBottomAreaBackground;
    public Color MatrixScreenBottomAreaOutline => matrixScreenBottomAreaOutline;
    public Color MatrixScreenBottomAreaForegroundElement => matrixScreenBottomAreaForegroundElement;
    public Color MatrixScreenBottomAreaTextDropShadow => matrixScreenBottomAreaTextDropShadow;
    public Color MatrixScreenBottomAreaDivider => matrixScreenBottomAreaDivider;
    public Color MatrixScreenSliderBackground => matrixScreenSliderBackground;
    public Color MatrixScreenSliderFill => matrixScreenSliderFill;
    public Color MatrixScreenSliderHandle => matrixScreenSliderHandle;
    public Color MatrixScreenSliderHandleHover => matrixScreenSliderHandleHover;
    public Color MatrixScreenSliderHandleClick => matrixScreenSliderHandleClick;
    public Color MatrixScreenBottomAreaToggle => matrixScreenBottomAreaToggle;
    public Color MatrixScreenBottomAreaToggleHover => matrixScreenBottomAreaToggleHover;
    public Color MatrixScreenBottomAreaToggleClick => matrixScreenBottomAreaToggleClick;

    [Header("UI Matrix")]
    [SerializeField] Color[] uiMatrixHeaders;
    [SerializeField] Color uiMatrixBackground;
    [SerializeField] Color uiMatrixControlsBackground;
    [SerializeField] Color uiMatrixOutline;
    [SerializeField] Color uiMatrixLabel;
    [SerializeField] Color uiMatrixLabelDropShadow;
    [SerializeField] Color uiMatrixFieldBackground;
    [SerializeField] Color uiMatrixFieldBackgroundHighlighted;
    [SerializeField] Color uiMatrixFieldBackgroundClicked;
    [SerializeField] Color uiMatrixFieldBackgroundDisabled;
    [SerializeField] Color uiMatrixFieldFlash;
    [SerializeField] Color uiMatrixFieldText;
    [SerializeField] Color uiMatrixFieldTextInvalid;
    [SerializeField] Color uiMatrixNameLabelInputFieldSelection;

    public Color[] UiMatrixHeaders => uiMatrixHeaders;
    public Color UiMatrixBackground => uiMatrixBackground;
    public Color UiMatrixControlsBackground => uiMatrixControlsBackground;
    public Color UiMatrixOutline => uiMatrixOutline;
    public Color UiMatrixLabel => uiMatrixLabel;
    public Color UiMatrixLabelDropShadow => uiMatrixLabelDropShadow;
    public Color UiMatrixFieldBackground => uiMatrixFieldBackground;
    public Color UiMatrixFieldBackgroundHighlighted => uiMatrixFieldBackgroundHighlighted;
    public Color UiMatrixFieldBackgroundClicked => uiMatrixFieldBackgroundClicked;
    public Color UiMatrixFieldBackgroundDisabled => uiMatrixFieldBackgroundDisabled;
    public Color UiMatrixFieldFlash => uiMatrixFieldFlash;
    public Color UiMatrixFieldText => uiMatrixFieldText;
    public Color UiMatrixFieldTextInvalid => uiMatrixFieldTextInvalid;
    public Color UiMatrixNameLabelInputFieldSelection => uiMatrixNameLabelInputFieldSelection;

    [Header("UI Matrix Buttons")]
    [SerializeField] Color uiMatrixHeaderButtonBackgroundDefault;
    [SerializeField] Color uiMatrixHeaderButtonBackgroundHover;
    [SerializeField] Color uiMatrixHeaderButtonBackgroundClick;
    [SerializeField] Color uiMatrixHeaderButtonBackgroundDisabled;
    [SerializeField] Color uiMatrixHeaderButtonElement;
    [SerializeField] Color uiMatrixControlsButtonBackgroundDefault;
    [SerializeField] Color uiMatrixControlsButtonBackgroundHover;
    [SerializeField] Color uiMatrixControlsButtonBackgroundClick;
    [SerializeField] Color uiMatrixControlsButtonBackgroundDisabled;
    [SerializeField] Color uiMatrixControlsButtonElement;

    public Color UiMatrixHeaderButtonBackgroundDefault => uiMatrixHeaderButtonBackgroundDefault;
    public Color UiMatrixHeaderButtonBackgroundHover => uiMatrixHeaderButtonBackgroundHover;
    public Color UiMatrixHeaderButtonBackgroundClick => uiMatrixHeaderButtonBackgroundClick;
    public Color UiMatrixHeaderButtonBackgroundDisabled => uiMatrixHeaderButtonBackgroundDisabled;
    public Color UiMatrixHeaderButtonElement => uiMatrixHeaderButtonElement;
    public Color UiMatrixControlsButtonBackgroundDefault => uiMatrixControlsButtonBackgroundDefault;
    public Color UiMatrixControlsButtonBackgroundHover => uiMatrixControlsButtonBackgroundHover;
    public Color UiMatrixControlsButtonBackgroundClick => uiMatrixControlsButtonBackgroundClick;
    public Color UiMatrixControlsButtonBackgroundDisabled => uiMatrixControlsButtonBackgroundDisabled;
    public Color UiMatrixControlsButtonElement => uiMatrixControlsButtonElement;

    [Header("UI Matrix Variables")]
    [SerializeField] Color uiMatrixVariablesBackground;
    [SerializeField] Color uiMatrixVariablesLabelAndIcons;
    [SerializeField] Color uiMatrixVariablesLabelAndIconsDisabled;
    [SerializeField] Color uiMatrixVariablesFieldBackground;
    [SerializeField] Color uiMatrixVariablesFieldBackgroundHover;
    [SerializeField] Color uiMatrixVariablesFieldBackgroundClick;
    [SerializeField] Color uiMatrixVariablesFieldBackgroundDisabled; 
    [SerializeField] Color uiMatrixVariablesFieldElement;
    [SerializeField] Color uiMatrixVariablesFieldElementInvalid;
    [SerializeField] Color uiMatrixVariablesFieldSelection;

    public Color UiMatrixVariablesBackground => uiMatrixVariablesBackground;
    public Color UiMatrixVariablesLabelAndIcons => uiMatrixVariablesLabelAndIcons;
    public Color UiMatrixVariablesLabelAndIconsDisabled => uiMatrixVariablesLabelAndIconsDisabled;
    public Color UiMatrixVariablesFieldBackground => uiMatrixVariablesFieldBackground;
    public Color UiMatrixVariablesFieldBackgroundHover => uiMatrixVariablesFieldBackgroundHover;
    public Color UiMatrixVariablesFieldBackgroundClick => uiMatrixVariablesFieldBackgroundClick;
    public Color UiMatrixVariablesFieldBackgroundDisabled => uiMatrixVariablesFieldBackgroundDisabled; 
    public Color UiMatrixVariablesFieldElement => uiMatrixVariablesFieldElement;
    public Color UiMatrixVariablesFieldElementInvalid => uiMatrixVariablesFieldElementInvalid;
    public Color UiMatrixVariablesFieldSelection => uiMatrixVariablesFieldSelection;

    [Header("Foldouts")]
    [SerializeField] Color foldoutBackground;
    [SerializeField] Color[] foldoutButtons;
    [SerializeField] Color foldoutButtonsHover;
    [SerializeField] Color foldoutButtonsClick;
    [SerializeField] Color foldoutButtonsText;
    [SerializeField] Color foldoutButtonsTextDisabled;
    
    public Color FoldoutBackground => foldoutBackground;
    public Color[] FoldoutButtons => foldoutButtons;
    public Color FoldoutButtonsHover => foldoutButtonsHover;
    public Color FoldoutButtonsClick => foldoutButtonsClick;
    public Color FoldoutButtonsText => foldoutButtonsText;
    public Color FoldoutButtonsTextDisabled => foldoutButtonsTextDisabled;

    [Header("UI Matrix Field Viewer/Editor")]
    [SerializeField] Color uiMatrixFieldViewerBackground;
    [SerializeField] Color uiMatrixFieldViewerDoneButton;
    [SerializeField] Color uiMatrixFieldViewerDoneButtonHover;
    [SerializeField] Color uiMatrixFieldViewerDoneButtonClick;
    [SerializeField] Color uiMatrixFieldViewerDoneButtonDisabled;
    [SerializeField] Color uiMatrixFieldViewerDoneButtonText;
    [SerializeField] Color uiMatrixFieldEditorSolidBackground;
    [SerializeField] Color uiMatrixFieldEditorSelectionColor;

    public Color UiMatrixFieldViewerBackground => uiMatrixFieldViewerBackground;
    public Color UiMatrixFieldViewerDoneButton => uiMatrixFieldViewerDoneButton;
    public Color UiMatrixFieldViewerDoneButtonHover => uiMatrixFieldViewerDoneButtonHover;
    public Color UiMatrixFieldViewerDoneButtonClick => uiMatrixFieldViewerDoneButtonClick;
    public Color UiMatrixFieldViewerDoneButtonDisabled => uiMatrixFieldViewerDoneButtonDisabled;
    public Color UiMatrixFieldViewerDoneButtonText => uiMatrixFieldViewerDoneButtonText;
    public Color UiMatrixFieldEditorSolidBackground => uiMatrixFieldEditorSolidBackground;
    public Color UiMatrixFieldEditorSelectionColor => uiMatrixFieldEditorSelectionColor;

    [Header("UI Matrix Model Preview")]
    [SerializeField] Color uiMatrixModelPreviewHeader;
    [SerializeField] Color uiMatrixModelPreview;
    [SerializeField] Color uiMatrixModelPreviewHover;
    [SerializeField] Color uiMatrixModelPreviewClick;

    public Color UiMatrixModelPreviewHeader => uiMatrixModelPreviewHeader;
    public Color UiMatrixModelPreview => uiMatrixModelPreview;
    public Color UiMatrixModelPreviewHover => uiMatrixModelPreviewHover;
    public Color UiMatrixModelPreviewClick => uiMatrixModelPreviewClick;

    [Header("Bottom Log")]
    [SerializeField] Color bottomLogBackground;
    [SerializeField] Color bottomLogExpandedBackground;
    [SerializeField] Color bottomLogRegularText;
    [SerializeField] Color bottomLogWarningText;
    [SerializeField] Color bottomLogErrorText;
    [SerializeField] Color bottomLogMessageFlash;

    public Color BottomLogBackground => bottomLogBackground;
    public Color BottomLogExpandedBackground => bottomLogExpandedBackground;
    public Color BottomLogRegularText => bottomLogRegularText;
    public Color BottomLogWarningText => bottomLogWarningText;
    public Color BottomLogErrorText => bottomLogErrorText;
    public Color BottomLogMessageFlash => bottomLogMessageFlash;

    [Header("Color Picker")]
    [SerializeField] Color colorPickerBackground;
    [SerializeField] Color colorPickerDropShadows;
    [SerializeField] Color colorPickerColorOutlineInside;
    [SerializeField] Color colorPickerColorOutlineOutside;
    [SerializeField] Color colorPickerAlphaGridTint;
    [SerializeField] Color colorPickerSliderLabel;
    [SerializeField] Color colorPickerSliderFill;
    [SerializeField] Color colorPickerSliderBackground;
    [SerializeField] Color colorPickerSliderHandle;
    [SerializeField] Color colorPickerSliderHandleHover;
    [SerializeField] Color colorPickerSliderHandleClick;
    [SerializeField] Color colorPickerInputFieldText;
    [SerializeField] Color colorPickerInputFieldSelection;
    [SerializeField] Color colorPickerInputFieldBackground;
    [SerializeField] Color colorPickerInputFieldBackgroundHover;
    [SerializeField] Color colorPickerInputFieldBackgroundClick;

    public Color ColorPickerBackground => colorPickerBackground;
    public Color ColorPickerDropShadows => colorPickerDropShadows;
    public Color ColorPickerColorOutlineInside => colorPickerColorOutlineInside;
    public Color ColorPickerColorOutlineOutside => colorPickerColorOutlineOutside;
    public Color ColorPickerAlphaGridTint => colorPickerAlphaGridTint;
    public Color ColorPickerSliderLabel => colorPickerSliderLabel;
    public Color ColorPickerSliderFill => colorPickerSliderFill;
    public Color ColorPickerSliderBackground => colorPickerSliderBackground;
    public Color ColorPickerSliderHandle => colorPickerSliderHandle;
    public Color ColorPickerSliderHandleHover => colorPickerSliderHandleHover;
    public Color ColorPickerSliderHandleClick => colorPickerSliderHandleClick;
    public Color ColorPickerInputFieldText => colorPickerInputFieldText;
    public Color ColorPickerInputFieldSelection => colorPickerInputFieldSelection;
    public Color ColorPickerInputFieldBackground => colorPickerInputFieldBackground;
    public Color ColorPickerInputFieldBackgroundHover => colorPickerInputFieldBackgroundHover;
    public Color ColorPickerInputFieldBackgroundClick => colorPickerInputFieldBackgroundClick;

    [Header("Lighting Screen")]
    [SerializeField] Color lightingScreenPropGroupHeaders;
    [SerializeField] Color lightingScreenPropGroupPropertyLabels;
    [SerializeField] Color lightingScreenPropGroupBottomText;
    [SerializeField] Color lightingScreenPropGroupBottomImage;
    [SerializeField] Color lightingScreenColorPropOutlineInside;
    [SerializeField] Color lightingScreenColorPropOutlineOutside;
    [SerializeField] Color lightingScreenDropShadows;
    [SerializeField] Color lightingScreenScrollbar;
    [SerializeField] Color lightingScreenScrollbarHover;
    [SerializeField] Color lightingScreenScrollbarClick;
    [SerializeField] Color lightingScreenScrollbarBackground;
    [SerializeField] Color lightingScreenButtonIcon;
    [SerializeField] Color lightingScreenButton;
    [SerializeField] Color lightingScreenButtonHover;
    [SerializeField] Color lightingScreenButtonClick;
    [SerializeField] Color lightingScreenSliderHandle;
    [SerializeField] Color lightingScreenSliderHandleHover;
    [SerializeField] Color lightingScreenSliderHandleClick;
    [SerializeField] Color lightingScreenSliderFill;
    [SerializeField] Color lightingScreenSliderBackground;
    [SerializeField] Color lightingScreenInputFieldText;
    [SerializeField] Color lightingScreenInputField;
    [SerializeField] Color lightingScreenInputFieldHover;
    [SerializeField] Color lightingScreenInputFieldClick;
    [SerializeField] Color lightingScreenInputFieldSelection;

    public Color LightingScreenPropGroupHeaders => lightingScreenPropGroupHeaders;
    public Color LightingScreenPropGroupPropertyLabels => lightingScreenPropGroupPropertyLabels;
    public Color LightingScreenPropGroupBottomText => lightingScreenPropGroupBottomText;
    public Color LightingScreenPropGroupBottomImage => lightingScreenPropGroupBottomImage;
    public Color LightingScreenColorPropOutlineInside => lightingScreenColorPropOutlineInside;
    public Color LightingScreenColorPropOutlineOutside => lightingScreenColorPropOutlineOutside;
    public Color LightingScreenDropShadows => lightingScreenDropShadows;
    public Color LightingScreenScrollbar => lightingScreenScrollbar;
    public Color LightingScreenScrollbarHover => lightingScreenScrollbarHover;
    public Color LightingScreenScrollbarClick => lightingScreenScrollbarClick;
    public Color LightingScreenScrollbarBackground => lightingScreenScrollbarBackground;
    public Color LightingScreenButtonIcon => lightingScreenButtonIcon;
    public Color LightingScreenButton => lightingScreenButton;
    public Color LightingScreenButtonHover => lightingScreenButtonHover;
    public Color LightingScreenButtonClick => lightingScreenButtonClick;
    public Color LightingScreenSliderHandle => lightingScreenSliderHandle;
    public Color LightingScreenSliderHandleHover => lightingScreenSliderHandleHover;
    public Color LightingScreenSliderHandleClick => lightingScreenSliderHandleClick;
    public Color LightingScreenSliderFill => lightingScreenSliderFill;
    public Color LightingScreenSliderBackground => lightingScreenSliderBackground;
    public Color LightingScreenInputFieldText => lightingScreenInputFieldText;
    public Color LightingScreenInputField => lightingScreenInputField;
    public Color LightingScreenInputFieldHover => lightingScreenInputFieldHover;
    public Color LightingScreenInputFieldClick => lightingScreenInputFieldClick;
    public Color LightingScreenInputFieldSelection => lightingScreenInputFieldSelection;

    [Header("Lighting Screen Intensity Graph")]
    [SerializeField] Color lsigBackground;
    [SerializeField] Color lsigGraph;
    [SerializeField] Color lsigUIElements;
    [SerializeField] Color lsigUIElementDropShadow;

    public Color LSIGBackground => lsigBackground;
    public Color LSIGGraph => lsigGraph;
    public Color LSIGUIElements => lsigUIElements;
    public Color LSIGUIElementDropShadow => lsigUIElementDropShadow;
    
}

#if UNITY_EDITOR

[CustomEditor(typeof(ColorScheme))]
public class ColorSchemeEditor : Editor {

    ColorScheme cs;

    void OnEnable () {
        cs = target as ColorScheme;
    }

    public override void OnInspectorGUI () {
        DrawDefaultInspector();
        if(GUILayout.Button("Invoke onChange")){
            ColorScheme.SwitchTo(cs);
        }
    }

}

#endif