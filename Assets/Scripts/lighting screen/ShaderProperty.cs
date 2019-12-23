using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Shader Property", fileName = "New Shader Property")]
public class ShaderProperty : ScriptableObject {

    public enum Type {
        Float,
        Color
    }

    public enum SpecialIdentifier {
        None,
        DiffuseColor,
        SpecularColor
    }

    [SerializeField] Type m_type;
    [SerializeField] SpecialIdentifier m_specialIdentifier;
    [SerializeField] string m_niceName;
    [SerializeField] Color m_defaultColor;
    [SerializeField] float m_minValue;
    [SerializeField] float m_maxValue;
    [SerializeField] float m_defaultValue;

    public Type type => m_type;
    public SpecialIdentifier specialIdentifier => m_specialIdentifier;
    public string niceName => m_niceName;
    public Color defaultColor => TypeCheckedColor(m_defaultColor);
    public float minValue => TypeCheckedNumber(m_minValue);
    public float maxValue => TypeCheckedNumber(m_maxValue);
    public float defaultValue => TypeCheckedNumber(m_defaultValue);

    bool TypeCheck (Type inputType) {
        if(this.type == inputType){
            return true;
        }
        Debug.LogError($"Asked for {inputType.ToString().ToLower()} on {nameof(ShaderProperty)} of type \"{m_type}\"!");
        return false;
    }

    Color TypeCheckedColor (Color actualValue) {
        if(TypeCheck(Type.Color)){
            return actualValue;
        }
        return Color.magenta;
    }

    float TypeCheckedNumber (float actualValue) {
        if(TypeCheck(Type.Float)){
            return actualValue;
        }
        return float.NaN;
    }
    
}

#if UNITY_EDITOR

[CustomEditor(typeof(ShaderProperty))]
public class ShaderPropertyEditor : Editor {

    ShaderProperty prop;

    void OnEnable () {
        prop = target as ShaderProperty;
    }

    public override void OnInspectorGUI () {
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(prop), typeof(ShaderProperty), false);
        GUI.enabled = true;
        var typeProp = serializedObject.FindProperty("m_type");
        EditorGUILayout.PropertyField(typeProp);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_specialIdentifier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_niceName"));
        switch(typeProp.enumValueIndex){
            case (int)(ShaderProperty.Type.Color):
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_defaultColor"));
                break;
            case (int)(ShaderProperty.Type.Float):
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_minValue"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_maxValue"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_defaultValue"));
                break;
            default:
                string errorText = $"INVALID PROPERTY \"{(ShaderProperty.Type)typeProp.enumValueIndex}\"!";
                EditorGUILayout.LabelField(errorText);
                Debug.LogError(errorText);
                break;
        }
        serializedObject.ApplyModifiedProperties();     // i didn't think i'd have to do this but APPARENTLY i do...
    }

}

#endif