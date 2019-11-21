using UnityEngine;

public class CustomGLCamera : MonoBehaviour {

    [Header("Drawing Things")]
    [SerializeField] Material objectMat;

    [Header("Settings")]
    [SerializeField] bool isExternalCamera;
    [SerializeField] CustomGLCamera otherCamera;
    [SerializeField] bool clipSpaceVis;
    [SerializeField] float pivotSize;

    public Camera attachedUnityCam { get; private set; }
    Material lineMaterialSolid;
    Material lineMaterialSeeThrough;

    public Matrix4x4 currentViewMatrix { get; private set; }
    public Matrix4x4 currentProjectionMatrix { get; private set; }

    [System.NonSerialized] public VertexMain vertexScreen;
    [System.NonSerialized] public bool drawPivot;
    [System.NonSerialized] public Vector3 pivotPointToDraw;

    void Awake () {
        attachedUnityCam = GetComponent<Camera>();

        // modified from https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnPostRender.html
        var shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterialSolid = new Material(shader);
        lineMaterialSolid.hideFlags = HideFlags.HideAndDontSave;
        lineMaterialSolid.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        lineMaterialSolid.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        lineMaterialSolid.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterialSolid.SetInt("_ZWrite", 0);
        lineMaterialSolid.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);

        lineMaterialSeeThrough = new Material(shader);
        lineMaterialSeeThrough.hideFlags = HideFlags.HideAndDontSave;
        lineMaterialSeeThrough.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterialSeeThrough.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterialSeeThrough.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterialSeeThrough.SetInt("_ZWrite", 0);
        lineMaterialSeeThrough.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);

        objectMat = Instantiate(objectMat);
        objectMat.hideFlags = HideFlags.HideAndDontSave;
        objectMat.EnableKeyword("USE_SPECIAL_MODEL_MATRIX");

        if(isExternalCamera){
            objectMat.EnableKeyword("SHOW_CLIPPING");
            objectMat.EnableKeyword("USE_SPECIAL_CLIPPING_MATRIX");
        }
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

        vertexScreen.GetCurrentMeshAndModelMatrix(out Mesh meshToDraw, out Matrix4x4 modelMatrix);
        if(meshToDraw != null){
            DrawObject(meshToDraw, modelMatrix);
        }
        DrawWireFloor();
        DrawAxes();
        if(isExternalCamera){
            DrawOtherCamera();
        }
        if(drawPivot){
            DrawPivot(true);
            DrawPivot(false);
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
        lineMaterialSolid.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(new Color(0.3f, 0.3f, 0.3f));
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
        lineMaterialSolid.SetPass(0);
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
            new Vector3(-1, -1, -1),
            new Vector3( 1, -1, -1),
            new Vector3( 1,  1, -1),
            new Vector3(-1,  1, -1),
            new Vector3(-1, -1, 1),
            new Vector3( 1, -1, 1),
            new Vector3( 1,  1, 1),
            new Vector3(-1,  1, 1)
        };
        lineMaterialSolid.SetPass(0);
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

    void DrawObject (Mesh meshToDraw, Matrix4x4 modelMatrix) {
        GL.PushMatrix();
        
        GL.LoadProjectionMatrix(currentProjectionMatrix);
        GL.LoadIdentity();
        GL.MultMatrix(currentViewMatrix * modelMatrix);

        if(isExternalCamera){
            objectMat.SetMatrix("_SpecialClippingMatrix", otherCamera.currentProjectionMatrix * otherCamera.currentViewMatrix * modelMatrix);
        }
        objectMat.SetMatrix("_SpecialModelMatrix", modelMatrix);
        objectMat.SetPass(0);
        GL.Begin(GL.TRIANGLES);
        GL.Color(Color.white);
        var verts = meshToDraw.vertices;
        var tris = meshToDraw.triangles;
        for(int i=0; i<tris.Length; i+=3){
            GL.Vertex(verts[tris[i+0]]);
            GL.Vertex(verts[tris[i+1]]);
            GL.Vertex(verts[tris[i+2]]);
        }
        GL.End();

        GL.PopMatrix();
    }
	
    void DrawPivot (bool seeThrough) {
        //should always have the same pixel-size
        float dist = (pivotPointToDraw - attachedUnityCam.transform.position).magnitude;
        Vector3 offsetH = pivotSize * attachedUnityCam.transform.right * dist / Screen.height;
        Vector3 offsetV = pivotSize * attachedUnityCam.transform.up * dist / Screen.height;
        Vector3[] points = new Vector3[]{
            pivotPointToDraw + offsetV,
            pivotPointToDraw + offsetH,
            pivotPointToDraw - offsetV,
            pivotPointToDraw - offsetH
        };
        Color pivotColor = new Color(1, 0.667f, 0);
        Color outlineColor = new Color(0, 0, 0);
        if(seeThrough){
            lineMaterialSeeThrough.SetPass(0);
            pivotColor.a = 0.333f;
            outlineColor.a = 0.333f;
        }else{
            lineMaterialSolid.SetPass(0);
        }
        GL.Begin(GL.TRIANGLES);
        GL.Color(pivotColor);
        GL.Vertex(points[0]);
        GL.Vertex(points[1]);
        GL.Vertex(points[2]);
        GL.Vertex(points[2]);
        GL.Vertex(points[3]);
        GL.Vertex(points[0]);
        GL.End();
        GL.Begin(GL.LINE_STRIP);
        GL.Color(outlineColor);
        GL.Vertex(points[0]);
        GL.Vertex(points[1]);
        GL.Vertex(points[2]);
        GL.Vertex(points[3]);
        GL.Vertex(points[0]);
        GL.End();
    }

}
