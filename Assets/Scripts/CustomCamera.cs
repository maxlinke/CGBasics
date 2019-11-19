using UnityEngine;

public class CustomCamera : MonoBehaviour {

    [Header("Components")]
    [SerializeField] Camera realCam;
    [SerializeField] CustomExternalTransform customExternalTransform;

    [Header("Settings")]
    [SerializeField] bool isExternalCamera;
    [SerializeField] CustomCamera otherCamera;

    public new Camera camera => realCam;    // new because "camera" is an old inherited monobehaviour thing
    public new Transform transform {        // because these cameras should not move. culling n stuff.
        get {  
            if(customExternalTransform.dummyObject == null){
                return this.gameObject.transform;
            }else{
                return customExternalTransform.dummyObject.transform;    
            }
        }
    }

    void Awake () {
        customExternalTransform.Init(this.gameObject);
        customExternalTransform.dummyObject.transform.localScale = new Vector3(1, 1, -1);   //otherwise everything is flipped AGAIN
    }

    void Reset () {
        if(realCam == null){
            realCam = GetComponent<Camera>();
        }
        if(customExternalTransform == null){
            customExternalTransform = GetComponent<CustomExternalTransform>();
        }
    }

    public void GetMatrices (GameObject otherGO, out Matrix4x4 modelMatrix, out Matrix4x4 mvpMatrix) {
        modelMatrix = CreateModelMatrix(otherGO.transform);
        if(isExternalCamera && otherCamera != null){
            modelMatrix = otherCamera.GetVPMatrix() * modelMatrix;
        }
        mvpMatrix = GetVPMatrix() * modelMatrix;
    }

    // TODO what do when there is no more view and projection matrix? 
    // treat everything as the model matrix and return identity for vp i guess
    // TODO what to do about object culling? hmmmm.... drawmeshimmediate?

    private Matrix4x4 CreateModelMatrix (Transform otherTransform) {
        var translationMatrix = Matrix4x4.Translate(otherTransform.position);
        var rotationMatrix = Matrix4x4.Rotate(otherTransform.rotation);
        var scaleMatrix = Matrix4x4.Scale(otherTransform.lossyScale);
        return translationMatrix * rotationMatrix * scaleMatrix;
    }

    private Matrix4x4 GetVPMatrix () {
        if(realCam == null){
            Debug.LogError("Camera not assigned!");
            return Matrix4x4.identity;
        }
        var viewMatrix = GetCustomViewMatrix();
        var projectionMatrix = GetProjectionMatrix();
        return projectionMatrix * viewMatrix;
    }

    public Matrix4x4 GetRealCameraViewMatrix () {
        return realCam.worldToCameraMatrix;
    }

    public Matrix4x4 GetCustomViewMatrix () {
        return CreateModelMatrix(transform).inverse;
    }

    public Matrix4x4 GetProjectionMatrix (bool overrideToTexture = false, bool overrideValue = false) {
        bool toTex;
        if(!overrideToTexture){
            toTex = (realCam.targetTexture != null || realCam.renderingPath == RenderingPath.DeferredShading || realCam.renderingPath == RenderingPath.DeferredLighting);  //this has to be the weirdest thing...
        }else{
            toTex = overrideValue;
        }
        return GL.GetGPUProjectionMatrix(realCam.projectionMatrix, toTex);
    }
	
}