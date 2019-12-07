using UnityEngine;
using UIMatrices;

public class CustomGLCamera : MonoBehaviour {

    [Header("Drawing Things")]
    [SerializeField] Material objectMat;

    [Header("Settings")]
    [SerializeField] bool isExternalCamera;
    [SerializeField] CustomGLCamera otherCamera;

    public bool matricesAreUpdated { get; private set; }
    public Matrix4x4 currentModelMatrix { get; private set; }
    public Matrix4x4 currentViewMatrix { get; private set; }
    public Matrix4x4 currentProjectionMatrix { get; private set; }

    public float nearClipPlane { 
        get { return attachedUnityCam.nearClipPlane; }
        set { attachedUnityCam.nearClipPlane = value; }
    }
    public float farClipPlane {
        get { return attachedUnityCam.farClipPlane; }
        set { attachedUnityCam.farClipPlane = value; }
    }
    public float fieldOfView {
        get { return attachedUnityCam.fieldOfView; }
        set { attachedUnityCam.fieldOfView = value; }
    }
    public float aspect {
        get { return attachedUnityCam.aspect; }
        set { attachedUnityCam.aspect = value; }
    }

    [System.NonSerialized] public MatrixScreen matrixScreen;
    [System.NonSerialized] public bool drawPivot;
    [System.NonSerialized] public Vector3 pivotPointToDraw;

    [System.NonSerialized] public bool drawSeeThrough;
    [System.NonSerialized] public bool drawObjectAsWireFrame;
    [System.NonSerialized] public bool canDrawCamera;
    [System.NonSerialized] public bool drawCamera;
    [System.NonSerialized] public bool drawClipSpace;

    Camera attachedUnityCam;
    Material lineMaterialSolid;
    Material lineMaterialSeeThrough;

    float startNearClipPlane;
    float startFarClipPlane;
    float startFieldOfView;
    // float startAspect;       // use ResetAspect instead
    Vector3 startPosition;
    Quaternion startRotation;

    const float seeThroughAlphaMultiplier = 0.333f;
    const float pivotSize = 9f;

    Color wireGridColor;
    Color wireObjectColor;
    Color camFrustumColor;
    Color clipBoxColor;
    Color xColor;
    Color yColor;
    Color zColor;
    Color pivotColor;
    Color pivotOutlineColor;

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

    void Awake () {
        attachedUnityCam = GetComponent<Camera>();
        SetupPremadeUnityColoredMaterials();        

        objectMat = Instantiate(objectMat);
        objectMat.hideFlags = HideFlags.HideAndDontSave;
        objectMat.EnableKeyword("USE_SPECIAL_MODEL_MATRIX");

        if(isExternalCamera){
            objectMat.EnableKeyword("SHOW_CLIPPING");
            objectMat.EnableKeyword("USE_SPECIAL_CLIPPING_MATRIX");
        }

        startNearClipPlane = nearClipPlane;
        startFarClipPlane = farClipPlane;
        startFieldOfView = fieldOfView;
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void OnEnable () {
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ResetToDefault();
        ColorScheme.onChange -= LoadColors;
    }

    public void ResetToDefault () {
        nearClipPlane = startNearClipPlane;
        farClipPlane = startFarClipPlane;
        fieldOfView = startFieldOfView;
        attachedUnityCam.ResetAspect();
        transform.position = startPosition;
        transform.rotation = startRotation;
    }

    void LoadColors (ColorScheme cs) {
        attachedUnityCam.backgroundColor = cs.VertRenderBackground;
        wireGridColor = cs.VertRenderWireGridFloor;
        wireObjectColor = cs.VertRenderWireObject;
        camFrustumColor = cs.VertRenderCameraFrustum;
        clipBoxColor = cs.VertRenderClipSpaceBox;
        xColor = cs.VertRenderOriginXAxis;
        yColor = cs.VertRenderOriginYAxis;
        zColor = cs.VertRenderOriginZAxis;
        pivotColor = cs.VertRenderPivot;
        pivotOutlineColor = cs.VertRenderPivotOutline;

        objectMat.SetColor("_FrontColor", cs.VertRenderObjectColor);
        objectMat.SetColor("_BackColor", cs.VertRenderObjectBackfaceColor);
        objectMat.SetColor("_LightColorFront", cs.VertRenderLight1);
        objectMat.SetColor("_LightColorBack", cs.VertRenderLight2);
        objectMat.SetColor("_LightColorAmbient", cs.VertRenderAmbientLight);
        objectMat.SetColor("_ClippingOverlayColor", cs.VertRenderClippingOverlay);
    }

    void SetupPremadeUnityColoredMaterials () {
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
    }

    void Update () {
        matricesAreUpdated = false;
    }

    void OnPreRender () {
        attachedUnityCam.cullingMask = 0;
        if(isExternalCamera && !otherCamera.matricesAreUpdated){
            otherCamera.UpdateMatrices();  //TODO change to a better name maybe?
        }
        UpdateMatrices();
    }

    void UpdateMatrices () {
        if(!isExternalCamera){
            // this is JUST for testing!
            matrixScreen.ViewPosMatrix.VariableContainer.EditVariable(MatrixConfig.InverseTranslationConfig.xPos, transform.position.x, false);
            matrixScreen.ViewPosMatrix.VariableContainer.EditVariable(MatrixConfig.InverseTranslationConfig.yPos, transform.position.y, false);
            matrixScreen.ViewPosMatrix.VariableContainer.EditVariable(MatrixConfig.InverseTranslationConfig.zPos, transform.position.z, true);
            // still just testing
            var left = -transform.right;
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newXx, left.x, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newXy, left.y, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newXz, left.z, false);
            var up = transform.up;
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newYx, up.x, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newYy, up.y, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newYz, up.z, false);
            var fwd = transform.forward;
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newZx, fwd.x, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newZy, fwd.y, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newZz, fwd.z, true);
            // still just testing
            matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.fov, attachedUnityCam.fieldOfView, false);
            matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.nearClip, attachedUnityCam.nearClipPlane, false);
            matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.farClip, attachedUnityCam.farClipPlane, false);
            matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.aspect, attachedUnityCam.aspect, true);
        }

        // if(isExternalCamera){
            currentViewMatrix = GLMatrixCreator.GetViewMatrix(
                pos: transform.position,
                forward: transform.forward,
                up: transform.up
            );
            currentProjectionMatrix = GLMatrixCreator.GetProjectionMatrix(
                fov: fieldOfView,
                aspectRatio: aspect,
                zNear: nearClipPlane,
                zFar: farClipPlane
            );
        // }else{
        //     currentViewMatrix
        // }
    }

    void OnPostRender () {
        bool wireCache = GL.wireframe;
        GL.wireframe = drawObjectAsWireFrame;

        GL.PushMatrix();

        GL.LoadProjectionMatrix(currentProjectionMatrix);
        GL.LoadIdentity();
        GL.MultMatrix(currentViewMatrix);

        // TODO change to get weighted matrices or something like that (only for the render cam)
        if(matrixScreen != null){
            matrixScreen.GetWeightedRenderingMatrices(out Matrix4x4 modelMatrix, out _);  // TODO use cam matrix
            var meshToDraw = matrixScreen.GetCurrentMesh();
            if(meshToDraw != null){
                DrawObject(meshToDraw, modelMatrix);
            }
        }

        if(drawSeeThrough){
            lineMaterialSeeThrough.SetPass(0);
            DrawAllTheWireThings(true);
        }
        lineMaterialSolid.SetPass(0);
        DrawAllTheWireThings(false);

        if(drawPivot){
            DrawPivot(true);
            DrawPivot(false);
        }

        GL.PopMatrix();

        GL.wireframe = wireCache;

        void DrawAllTheWireThings (bool seeThrough) {
            DrawWireFloor(seeThrough);
            DrawAxes(seeThrough);
            if(isExternalCamera){
                DrawOtherCamera(seeThrough);
                DrawClipSpace(clipBoxColor, seeThrough);
            }
        }
    }

#region GL_Drawing

    void DrawWireFloor (bool seeThrough) {
        GL.Begin(GL.LINES);
        GL.Color(GetConditionalSeeThroughColor(wireGridColor, seeThrough));
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

    void DrawAxes (bool seeThrough) {
        GL.Begin(GL.LINES);
        GL.Color(GetConditionalSeeThroughColor(xColor, seeThrough));
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(1, 0, 0);
        GL.Color(GetConditionalSeeThroughColor(yColor, seeThrough));
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(0, 1, 0);
        GL.Color(GetConditionalSeeThroughColor(zColor, seeThrough));
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(0, 0, 1);
        GL.End();
    }

    void DrawOtherCamera (bool seeThrough) {
        GL.PushMatrix();
        
        GL.LoadProjectionMatrix(currentProjectionMatrix);
        GL.LoadIdentity();
        GL.MultMatrix(currentViewMatrix * (otherCamera.currentProjectionMatrix * otherCamera.currentViewMatrix).inverse);
        DrawClipSpace(camFrustumColor, seeThrough);

        GL.PopMatrix();
    }

    void DrawClipSpace (Color drawColor, bool seeThrough) {
        GL.Begin(GL.LINES);
        GL.Color(GetConditionalSeeThroughColor(drawColor, seeThrough));
        for(int i=0; i<4; i++){
            ClipVertLine(i, (i+1)%4);
            ClipVertLine(4+i, 4+(i+1)%4);
            ClipVertLine(i, i+4);
        }
        GL.End();

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
        if(drawObjectAsWireFrame){
            lineMaterialSolid.SetPass(0);
        }else{
            objectMat.SetMatrix("_SpecialModelMatrix", modelMatrix);
            objectMat.SetPass(0);
        }
        GL.Begin(GL.TRIANGLES);
        if(drawObjectAsWireFrame){
            GL.Color(wireObjectColor);
        }else{
            GL.Color(Color.white);
        }
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
        // should always have the same pixel-size
        float dist = (pivotPointToDraw - attachedUnityCam.transform.position).magnitude;
        float preMul = pivotSize * dist / Screen.height;
        preMul *= fieldOfView / 60;         // not perfect but better than nothing. 
        Vector3 offsetH = preMul * transform.right;
        Vector3 offsetV = preMul * transform.up;
        Vector3[] points = new Vector3[]{
            pivotPointToDraw + offsetV,
            pivotPointToDraw + offsetH,
            pivotPointToDraw - offsetV,
            pivotPointToDraw - offsetH
        };
        if(seeThrough){
            lineMaterialSeeThrough.SetPass(0);
        }else{
            lineMaterialSolid.SetPass(0);
        }
        GL.Begin(GL.TRIANGLE_STRIP);
        GL.Color(pivotColor * new Color(1,1,1, seeThrough ? seeThroughAlphaMultiplier : 1));
        GL.Vertex(points[3]);
        GL.Vertex(points[2]);
        GL.Vertex(points[0]);
        GL.Vertex(points[1]);
        GL.End();
        GL.Begin(GL.LINE_STRIP);
        GL.Color(pivotOutlineColor * new Color(1,1,1, seeThrough ? seeThroughAlphaMultiplier : 1));
        GL.Vertex(points[0]);
        GL.Vertex(points[1]);
        GL.Vertex(points[2]);
        GL.Vertex(points[3]);
        GL.Vertex(points[0]);
        GL.End();
    }

#endregion

    Color GetConditionalSeeThroughColor (Color inputColor, bool seeThrough) {
        return (seeThrough ? GetSeeThroughColor(inputColor) : inputColor);
    }

    Color GetSeeThroughColor (Color inputColor) {
        return new Color(inputColor.r, inputColor.g, inputColor.b, inputColor.a * seeThroughAlphaMultiplier);
    }

}
