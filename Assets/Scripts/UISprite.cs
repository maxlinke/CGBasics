using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class UISprite {

    [SerializeField] UISprites.ID m_id;
    [SerializeField] Sprite m_sprite;

    public UISprites.ID id => m_id;
    public Sprite sprite => m_sprite;
	
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(UISprite))]
public class UISpriteDrawer : PropertyDrawer {

    const float margin = 5f;
    const float preferredFieldWidth = 200f;

    float halfMargin => margin / 2;

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        float remainingWidth = position.width;
        Rect enumRect, spriteRect;
        if(remainingWidth < (2 * preferredFieldWidth + margin)){
            remainingWidth -= margin;
            enumRect = new Rect(position.x, position.y, remainingWidth / 2, position.height);
            spriteRect = new Rect(position.x + margin + enumRect.width, position.y, remainingWidth / 2, position.height);
        }else{
            enumRect = new Rect(position.x, position.y, preferredFieldWidth, position.height);
            spriteRect = new Rect(position.x + enumRect.width + margin, position.y, preferredFieldWidth, position.height);
        }
        EditorGUI.PropertyField(enumRect, property.FindPropertyRelative("m_id"), GUIContent.none);
        EditorGUI.PropertyField(spriteRect, property.FindPropertyRelative("m_sprite"), GUIContent.none);
        EditorGUI.EndProperty();
    }

}

#endif