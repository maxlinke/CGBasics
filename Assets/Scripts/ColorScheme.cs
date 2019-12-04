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

    [Header("Vert Render View")]
    [SerializeField] Color vertRenderBackground;
    [SerializeField] Color vertRenderWireGridFloor;
    [SerializeField] Color vertRenderCameraFrustum;
    [SerializeField] Color vertRenderClipSpaceBox;
    [SerializeField] Color vertRenderOriginXAxis;
    [SerializeField] Color vertRenderOriginYAxis;
    [SerializeField] Color vertRenderOriginZAxis;
    [SerializeField] Color vertRenderPivot;
    [SerializeField] Color vertRenderPivotOutline;
    [SerializeField] Color vertRenderWireObject;

    public Color VertRenderBackground => vertRenderBackground;
    public Color VertRenderWireGridFloor => vertRenderWireGridFloor;
    public Color VertRenderCameraFrustum => vertRenderCameraFrustum;
    public Color VertRenderClipSpaceBox => vertRenderClipSpaceBox;
    public Color VertRenderOriginXAxis => vertRenderOriginXAxis;
    public Color VertRenderOriginYAxis => vertRenderOriginYAxis;
    public Color VertRenderOriginZAxis => vertRenderOriginZAxis;
    public Color VertRenderPivot => vertRenderPivot;
    public Color VertRenderPivotOutline => vertRenderPivotOutline;
    public Color VertRenderWireObject => vertRenderWireObject;

    [Header("Vert Render View Object")]
    [SerializeField] Color vertRenderObjectColor;
    [SerializeField] Color vertRenderObjectBackfaceColor;
    [SerializeField] Color vertRenderLight1;
    [SerializeField] Color vertRenderLight2;
    [SerializeField] Color vertRenderAmbientLight;
    [SerializeField] Color vertRenderClippingOverlay;
	
    public Color VertRenderObjectColor => vertRenderObjectColor;
    public Color VertRenderObjectBackfaceColor => vertRenderObjectBackfaceColor;
    public Color VertRenderLight1 => vertRenderLight1;
    public Color VertRenderLight2 => vertRenderLight2;
    public Color VertRenderAmbientLight => vertRenderAmbientLight;
    public Color VertRenderClippingOverlay => vertRenderClippingOverlay;

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

    [Header("UI Matrix Config Picker")]
    [SerializeField] Color uiMatrixConfigPickerBackground;
    [SerializeField] Color[] uiMatrixConfigPickerButtons;
    [SerializeField] Color uiMatrixConfigPickerButtonsHover;
    [SerializeField] Color uiMatrixConfigPickerButtonsClick;
    [SerializeField] Color uiMatrixConfigPickerText;

    public Color UiMatrixConfigPickerBackground => uiMatrixConfigPickerBackground;
    public Color[] UiMatrixConfigPickerButtons => uiMatrixConfigPickerButtons;
    public Color UiMatrixConfigPickerButtonsHover => uiMatrixConfigPickerButtonsHover;
    public Color UiMatrixConfigPickerButtonsClick => uiMatrixConfigPickerButtonsClick;
    public Color UiMatrixConfigPickerText => uiMatrixConfigPickerText;

    [Header("UI Matrix Field Viewer/Editor")]
    [SerializeField] Color uiMatrixFieldViewerBackground;
    [SerializeField] Color uiMatrixFieldViewerDoneButton;
    [SerializeField] Color uiMatrixFieldViewerDoneButtonHover;
    [SerializeField] Color uiMatrixFieldViewerDoneButtonClick;
    [SerializeField] Color uiMatrixFieldViewerDoneButtonDisabled;
    [SerializeField] Color uiMatrixFieldViewerDoneButtonText;

    public Color UiMatrixFieldViewerBackground => uiMatrixFieldViewerBackground;
    public Color UiMatrixFieldViewerDoneButton => uiMatrixFieldViewerDoneButton;
    public Color UiMatrixFieldViewerDoneButtonHover => uiMatrixFieldViewerDoneButtonHover;
    public Color UiMatrixFieldViewerDoneButtonClick => uiMatrixFieldViewerDoneButtonClick;
    public Color UiMatrixFieldViewerDoneButtonDisabled => uiMatrixFieldViewerDoneButtonDisabled;
    public Color UiMatrixFieldViewerDoneButtonText => uiMatrixFieldViewerDoneButtonText;

    [Header("Bottom Log")]
    [SerializeField] Color bottomLogBackground;
    [SerializeField] Color bottomLogExpandedBackground;
    [SerializeField] Color bottomLogRegularText;
    [SerializeField] Color bottomLogWarningText;
    [SerializeField] Color bottomLogErrorText;

    public Color BottomLogBackground => bottomLogBackground;
    public Color BottomLogExpandedBackground => bottomLogExpandedBackground;
    public Color BottomLogRegularText => bottomLogRegularText;
    public Color BottomLogWarningText => bottomLogWarningText;
    public Color BottomLogErrorText => bottomLogErrorText;
    
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