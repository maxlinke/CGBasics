using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ModelPicker : MonoBehaviour {

    [SerializeField] DefaultMesh[] presetMeshes;

    static List<DefaultMesh> loadedMeshes;

    void Awake () {
        if(loadedMeshes != null){
            Debug.LogError("Apparent singleton violation! Aborting...", this.gameObject);
            return;
        }
        loadedMeshes = new List<DefaultMesh>();
        foreach(var defaultMesh in presetMeshes){
            loadedMeshes.Add(defaultMesh);
        }
        // TODO streamingassets maybe
    }

    void OnDestroy () {
        loadedMeshes = null;
    }

    public static void Open (System.Action<Mesh> onMeshPicked, float scale) {
        var buttonSetups = new List<Foldout.ButtonSetup>();
        foreach(var defaultMesh in loadedMeshes){
            string meshName = defaultMesh.name;
            Mesh mesh = defaultMesh.mesh;
            buttonSetups.Add(new Foldout.ButtonSetup(
                buttonName: meshName,
                buttonHoverMessage: meshName,
                buttonClickAction: () => {onMeshPicked.Invoke(mesh);},
                buttonInteractable: true
            ));
        }
        Foldout.Create(buttonSetups, () => {onMeshPicked.Invoke(null);}, scale);
    }
	
}

[System.Serializable]
public class DefaultMesh {
    [SerializeField] string m_name;
    [SerializeField] Mesh m_mesh;
    public string name => m_name;
    public Mesh mesh => m_mesh;
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(DefaultMesh))]
public class DefaultMeshDrawer : PropertyDrawer {

    const float margin = 5f;

    float halfMargin => margin / 2f;

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);
        float remainingWidth = position.width;
        float fieldWidth = position.width / 2 - halfMargin;
        Rect leftRect = new  Rect(position.x, position.y, fieldWidth, position.height);
        Rect rightRect = new Rect(position.x + fieldWidth + halfMargin, position.y, fieldWidth, position.height);
        EditorGUI.PropertyField(leftRect, property.FindPropertyRelative("m_name"), GUIContent.none);
        EditorGUI.PropertyField(rightRect, property.FindPropertyRelative("m_mesh"), GUIContent.none);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        return EditorGUIUtility.singleLineHeight;
    }

}

#endif