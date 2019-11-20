using UnityEngine;

public class CustomCamera : MonoBehaviour {

    [Header("Components")]
    [SerializeField] Camera realCam;
    [SerializeField] CustomExternalTransform customExternalTransform;

    [Header("Settings")]
    [SerializeField] bool isExternalCamera;
    [SerializeField] CustomCamera otherCamera;
    [SerializeField] bool useGLMatrices;

    private Material lineMaterial;

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

    void OnPostRender () {
        if(lineMaterial == null){
            // from https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnPostRender.html
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            // Turn off backface culling, depth writes, depth test.
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        }
        GL.PushMatrix();
        GL.LoadIdentity();
        if(useGLMatrices){
            GL.MultMatrix(GLMatrixCreator.GetViewMatrix(transform.position, transform.position + transform.forward, transform.up));
            GL.LoadProjectionMatrix(GLMatrixCreator.GetProjectionMatrix(realCam.fieldOfView, realCam.aspect, realCam.nearClipPlane, realCam.farClipPlane));            
        }else{
            GL.MultMatrix(GetCustomViewMatrix());
            GL.LoadProjectionMatrix(GetProjectionMatrix(true, false));
        }
        lineMaterial.SetPass(0);
        GL.Color(new Color(1,1,1));
        GL.Begin(GL.LINES);
        for(int x=-10; x<=10; x++){
            GL.Vertex(new Vector3(x, 0, -10));
            GL.Vertex(new Vector3(x, 0, 10));
        }
        for(int z=-10; z<=10; z++){
            GL.Vertex(new Vector3(-10, 0, z));
            GL.Vertex(new Vector3(10, 0, z));
        }
        GL.End();
        GL.PopMatrix();
    }

    public void GetMatrices (GameObject otherGO, out Matrix4x4 modelMatrix, out Matrix4x4 mvpMatrix) {
        modelMatrix = CreateModelMatrix(otherGO.transform);
        if(isExternalCamera && otherCamera != null){
            modelMatrix = otherCamera.GetVPMatrix() * modelMatrix;
        }
        mvpMatrix = GetVPMatrix() * modelMatrix;
    }

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