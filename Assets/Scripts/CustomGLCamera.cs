using UnityEngine;

public class CustomGLCamera : MonoBehaviour {

    [Header("Drawing Things")]
    [SerializeField] Material objectMat;

    [Header("Settings")]
    [SerializeField] bool isExternalCamera;
    [SerializeField] CustomGLCamera otherCamera;

    public Camera attachedUnityCam { get; private set; }
    Material lineMaterial;

    public Matrix4x4 currentViewMatrix { get; private set; }
    public Matrix4x4 currentProjectionMatrix { get; private set; }

    void Awake () {
        attachedUnityCam = GetComponent<Camera>();

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

    void OnPreRender () {
        attachedUnityCam.cullingMask = 0;
        SetupCurrentViewAndProjectionMatrix();
    }

    void OnPostRender () {
        GL.PushMatrix();

        GL.LoadProjectionMatrix(currentProjectionMatrix);
        GL.LoadIdentity();
        GL.MultMatrix(currentViewMatrix);

        DrawWireFloor();
        DrawAxes();
        if(isExternalCamera){
            DrawOtherCamera();
        }

        GL.PopMatrix();
    }

    void SetupCurrentViewAndProjectionMatrix () {
        currentViewMatrix = GLMatrixCreator.GetViewMatrix(
            pos: transform.position,
            forward: transform.forward,
            up: transform.up
        );
        currentProjectionMatrix = GLMatrixCreator.GetProjectionMatrix(
            fov: attachedUnityCam.fieldOfView,
            aspectRatio: attachedUnityCam.aspect,
            zNear: attachedUnityCam.nearClipPlane,
            zFar: attachedUnityCam.farClipPlane
        );
    }

    void DrawWireFloor () {
        lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(new Color(0.2f, 0.2f, 0.2f));
        for(int x=-10; x<=10; x++){
            GL.Vertex3(x, 0, -10);
            GL.Vertex3(x, 0, 10);
        }
        for(int z=-10; z<=10; z++){
            GL.Vertex3(-10, 0, z);
            GL.Vertex3(10, 0, z);
        }
        GL.End();
    }

    void DrawAxes () {
        lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(1, 0, 0);
        GL.Color(Color.green);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(0, 1, 0);
        GL.Color(Color.blue);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(0, 0, 1);
        GL.End();
    }

    void DrawOtherCamera () {
        Vector3[] clipSpaceVertices = new Vector3[]{
            new Vector3(-1, -1, 0),
            new Vector3( 1, -1, 0),
            new Vector3( 1,  1, 0),
            new Vector3(-1,  1, 0),
            new Vector3(-1, -1, 1),
            new Vector3( 1, -1, 1),
            new Vector3( 1,  1, 1),
            new Vector3(-1,  1, 1)
        };
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        
        GL.LoadProjectionMatrix(currentProjectionMatrix);
        GL.LoadIdentity();
        GL.MultMatrix(currentViewMatrix * (otherCamera.currentProjectionMatrix * otherCamera.currentViewMatrix).inverse);

        GL.Begin(GL.LINES);
        GL.Color(Color.white);

        for(int i=0; i<4; i++){
            ClipVertLine(i, (i+1)%4);
            ClipVertLine(4+i, 4+(i+1)%4);
            ClipVertLine(i, i+4);
        }
        
        GL.End();
        GL.PopMatrix();

        void ClipVertLine (int index1, int index2) {
            GL.Vertex(clipSpaceVertices[index1]);
            GL.Vertex(clipSpaceVertices[index2]);
        }
    }

    void DrawWithGL (System.Action betweenPushPop) {
        
        betweenPushPop();
        GL.PopMatrix();
    }
	
}
