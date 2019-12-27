using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LightingModelGraphPropNameResolver : ScriptableObject {

    [SerializeField] LMNameLink[] links;

    public IEnumerator<LMNameLink> GetEnumerator () {
        foreach(var link in links){
            yield return link;
        }
    }
	
}

[System.Serializable]
public class LMNameLink {

    [SerializeField] LightingModel m_lm;
    [SerializeField] string m_propName;

    public LightingModel lm => m_lm;
    public string propName => m_propName;

}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(LMNameLink))]
public class LMNameLinkEditor : PropertyDrawer {

    const float margin = 5f;
    const float maxLMFieldWidth = 150f;

    float halfMargin => margin / 2f;

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        // position = EditorGUI.PrefixLabel(position, label);

        float remainingWidth = position.width;
        float lmFieldWidth, nameRectWidth;
        if(remainingWidth > (2 * maxLMFieldWidth) + margin){
            lmFieldWidth = maxLMFieldWidth;
            nameRectWidth = remainingWidth - maxLMFieldWidth - margin;
        }else{
            lmFieldWidth = remainingWidth / 2f - halfMargin;
            nameRectWidth = remainingWidth / 2f - halfMargin;
        }
        Rect nameRect = new Rect(position.x, position.y, nameRectWidth, position.height);
        Rect lmFieldRect = new Rect(position.x + nameRect.width + margin, position.y, lmFieldWidth, position.height);

        EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("m_propName"), GUIContent.none);
        EditorGUI.PropertyField(lmFieldRect, property.FindPropertyRelative("m_lm"), GUIContent.none);

        EditorGUI.EndProperty();
    }

}

#endif
