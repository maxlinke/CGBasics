using UnityEngine;

// #if UNITY_EDITOR
// using UnityEditor;
// #endif

public class CustomCamera : MonoBehaviour {

    [Header("Components")]
    [SerializeField] Camera realCam;

    [Header("Settings")]
    [SerializeField] bool isExternalCamera;
    // [SerializeField, HideInInspector] CustomCamera otherCamera;
    [SerializeField] CustomCamera otherCamera;

    public new Camera camera => realCam;    // new because "camera" is an old inherited monobehaviour thing

    public void GetMatrices (GameObject otherGO, out Matrix4x4 modelMatrix, out Matrix4x4 mvpMatrix) {
        var translationMatrix = Matrix4x4.Translate(otherGO.transform.position);
        var rotationMatrix = Matrix4x4.Rotate(otherGO.transform.rotation);
        var scaleMatrix = Matrix4x4.Scale(otherGO.transform.lossyScale);
        modelMatrix = translationMatrix * rotationMatrix * scaleMatrix;
        if(isExternalCamera && otherCamera != null){
            modelMatrix = otherCamera.GetVPMatrix() * modelMatrix;
        }
        mvpMatrix = GetVPMatrix() * modelMatrix;
    }

    // TODO what do when there is no more view and projection matrix? 
    // treat everything as the model matrix and return identity for vp i guess
    // TODO what to do about object culling? hmmmm.... drawmeshimmediate?

    private Matrix4x4 GetVPMatrix () {
        if(realCam == null){
            Debug.LogError("Camera not assigned!");
            return Matrix4x4.identity;
        }
        var viewMatrix = realCam.worldToCameraMatrix;
        var projectionMatrix = GL.GetGPUProjectionMatrix(realCam.projectionMatrix, true);
        return projectionMatrix * viewMatrix;
    }
	
}

// #if UNITY_EDITOR

// [CustomEditor(typeof(CustomCamera))]
// public class CustomCameraEditor : Editor {

//     CustomCamera cc;

//     void OnEnable () {
//         cc = target as CustomCamera;
//     }

//     public override void OnInspectorGUI () {
//         DrawDefaultInspector();
//         if(serializedObject.FindProperty("isExternalCamera").boolValue){
//             EditorGUILayout.PropertyField(serializedObject.FindProperty("otherCamera"));
//         }
//     }

// }

// #endif