using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Color Scheme", fileName = "new Color Scheme")]
public class ColorScheme : ScriptableObject {

    private static ColorScheme m_current;
    public static ColorScheme current {
        get {
            if(m_current == null){
                var first = Resources.FindObjectsOfTypeAll<ColorScheme>()[0];
                m_current = first;
            }
            return m_current;
        }
    }

    public static void SwitchTo (ColorScheme newColorScheme) {
        m_current = newColorScheme;
        onChange.Invoke(newColorScheme);
    }

    public static event System.Action<ColorScheme> onChange = delegate {};

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

    public Color VertRenderBackground => vertRenderBackground;
    public Color VertRenderWireGridFloor => vertRenderWireGridFloor;
    public Color VertRenderCameraFrustum => vertRenderCameraFrustum;
    public Color VertRenderClipSpaceBox => vertRenderClipSpaceBox;
    public Color VertRenderOriginXAxis => vertRenderOriginXAxis;
    public Color VertRenderOriginYAxis => vertRenderOriginYAxis;
    public Color VertRenderOriginZAxis => vertRenderOriginZAxis;
    public Color VertRenderPivot => vertRenderPivot;
    public Color VertRenderPivotOutline => vertRenderPivotOutline;
	
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