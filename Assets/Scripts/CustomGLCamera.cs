using UnityEngine;
using UIMatrices;

public class CustomGLCamera : MonoBehaviour {

    const string clippingKeyword = "SHOW_CLIPPING";
    const string specialClippingMatrixKeyword = "USE_SPECIAL_CLIPPING_MATRIX";
    const string clippingMatrixName = "_SpecialClippingMatrix";
    const string specialModelMatrixKeyword = "USE_SPECIAL_MODEL_MATRIX";
    const string modelMatrixName = "_SpecialModelMatrix";

    [SerializeField] Material objectMat;

    public bool IsExternalCamera => isExternalCamera;
    public bool matricesAreUpdated { get; private set; }
    public Matrix4x4 modelMatrix { get; private set; }
    public Matrix4x4 cameraMatrix { get; private set; }
    private Mesh currentMesh;

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

    [System.NonSerialized] public bool drawPivot;
    [System.NonSerialized] public Vector3 pivotPointToDraw;

    [System.NonSerialized] public bool drawSeeThrough;
    [System.NonSerialized] public bool drawObjectAsWireFrame;
    // [System.NonSerialized] public bool canDrawCamera;
    [System.NonSerialized] public bool drawCamera;
    [System.NonSerialized] public bool drawClipSpace;
    [System.NonSerialized] public bool showClipping;
    [System.NonSerialized] public bool drawGridFloor;
    [System.NonSerialized] public bool drawOrigin;

    bool isExternalCamera;
    CustomGLCamera otherCamera;

    bool initialized;
    MatrixScreen matrixScreen;
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

    public void Initialize (MatrixScreen matrixScreen, bool isExternalCamera, CustomGLCamera otherCamera, float inputFOV, float inputNearClip, float inputFarClip, Vector3 inputStartPos) {
        if(initialized){
            Debug.LogError("Duplicate init call, aborting!", this.gameObject);
            return;
        }
        this.matrixScreen = matrixScreen;
        this.isExternalCamera = isExternalCamera;
        this.otherCamera = otherCamera;

        EnsureUnityCamLoaded();
        SetupPremadeUnityColoredMaterials();        

        objectMat = Instantiate(objectMat);
        objectMat.hideFlags = HideFlags.HideAndDontSave;
        objectMat.EnableKeyword(specialModelMatrixKeyword);
        lineMaterialSolid.EnableKeyword(specialModelMatrixKeyword);
        lineMaterialSeeThrough.EnableKeyword(specialModelMatrixKeyword);

        if(isExternalCamera){
            objectMat.EnableKeyword(specialClippingMatrixKeyword);
            lineMaterialSolid.EnableKeyword(specialClippingMatrixKeyword);
            lineMaterialSeeThrough.EnableKeyword(specialClippingMatrixKeyword);
        }

        nearClipPlane = inputNearClip;
        farClipPlane = inputFarClip;
        fieldOfView = inputFOV;
        transform.position = inputStartPos;
        transform.rotation = Quaternion.LookRotation(-inputStartPos, Vector3.up);

        startNearClipPlane = nearClipPlane;
        startFarClipPlane = farClipPlane;
        startFieldOfView = fieldOfView;
        startPosition = transform.position;
        startRotation = transform.rotation;

        initialized = true;
    }

    public void SetupViewportRect (Rect viewportRect) {
        attachedUnityCam.rect = viewportRect;
    }

    void OnEnable () {
        if(initialized){
            LoadColors(ColorScheme.current);
        }
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ResetToDefault();
        ColorScheme.onChange -= LoadColors;
    }

    public void ResetToDefault () {
        if(!initialized){
            Debug.LogError("Not initialized yet! Aborting...", this.gameObject);
            return;
        }
        nearClipPlane = startNearClipPlane;
        farClipPlane = startFarClipPlane;
        fieldOfView = startFieldOfView;
        attachedUnityCam.ResetAspect();
        transform.position = startPosition;
        transform.rotation = startRotation;
    }

    void EnsureUnityCamLoaded () {
        if(attachedUnityCam == null){
            attachedUnityCam = GetComponent<Camera>();
        }
    }

    public void LoadColors (ColorScheme cs) {
        EnsureUnityCamLoaded();
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
        lineMaterialSolid.SetColor("_ClippingOverlayColor", cs.VertRenderClippingOverlay);
        lineMaterialSeeThrough.SetColor("_ClippingOverlayColor", cs.VertRenderClippingOverlay);
    }

    void SetupPremadeUnityColoredMaterials () {
        // modified from https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnPostRender.html
        var shader = Shader.Find("Custom/InternalColoredWithCulling");
        lineMaterialSolid = new Material(shader);
        lineMaterialSolid.hideFlags = HideFlags.HideAndDontSave;
        lineMaterialSolid.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        lineMaterialSolid.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        lineMaterialSolid.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterialSolid.SetInt("_ZWrite", 1);
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
        if(!initialized){
            return;
        }
        attachedUnityCam.cullingMask = 0;
        if(isExternalCamera && !otherCamera.matricesAreUpdated){
            otherCamera.UpdateMatricesAndMesh();  //TODO change to a better name maybe?
        }
        UpdateMatricesAndMesh();
    }

    void UpdateMatricesAndMesh () {
        if(!isExternalCamera && !matrixScreen.freeModeActivated){
            matrixScreen.ViewPosMatrix.VariableContainer.EditVariable(MatrixConfig.InverseTranslationConfig.xPos, transform.position.x, false);
            matrixScreen.ViewPosMatrix.VariableContainer.EditVariable(MatrixConfig.InverseTranslationConfig.yPos, transform.position.y, false);
            matrixScreen.ViewPosMatrix.VariableContainer.EditVariable(MatrixConfig.InverseTranslationConfig.zPos, transform.position.z, true);
            var right = transform.right;
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newXx, right.x, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newXy, right.y, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newXz, right.z, false);
            var up = transform.up;
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newYx, up.x, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newYy, up.y, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newYz, up.z, false);
            var fwd = transform.forward;
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newZx, fwd.x, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newZy, fwd.y, false);
            matrixScreen.ViewRotMatrix.VariableContainer.EditVariable(MatrixConfig.RebaseConfig.newZz, fwd.z, true);
            matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.fov, attachedUnityCam.fieldOfView, false);
            matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.nearClip, attachedUnityCam.nearClipPlane, false);
            matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.farClip, attachedUnityCam.farClipPlane, false);
            matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.aspect, attachedUnityCam.aspect, true);
        }

        if(isExternalCamera){
            var currentViewMatrix = GLMatrixCreator.GetViewMatrix(
                pos: transform.position,
                forward: transform.forward,
                up: transform.up
            );
            var currentProjectionMatrix = GLMatrixCreator.GetProjectionMatrix(
                fov: fieldOfView,
                aspectRatio: aspect,
                zNear: nearClipPlane,
                zFar: farClipPlane
            );
            cameraMatrix = currentProjectionMatrix * currentViewMatrix;
            modelMatrix = Matrix4x4.zero;       // if THIS (external) camera needs a model matrix, it should get it from the other camera.
        }else{
            cameraMatrix = matrixScreen.GetWeightedCameraMatrixForRendering();
            modelMatrix = matrixScreen.GetWeightedModelMatrixForRendering();
        }
        currentMesh = matrixScreen.GetCurrentMesh();
        matricesAreUpdated = true;
    }

    void OnPostRender () {
        if(!initialized){
            return;
        }

        GL.PushMatrix();
        GL.LoadProjectionMatrix(Matrix4x4.identity);
        GL.LoadIdentity();

        if(currentMesh != null){            
            RenderTheObject();
        }

        DrawWithNewMVPMatrix(cameraMatrix, () => {
            if(drawSeeThrough){
                DrawAllTheWireThings(true);
            }
            DrawAllTheWireThings(false);

            if(drawPivot){
                lineMaterialSolid.DisableKeyword(clippingKeyword);
                lineMaterialSeeThrough.DisableKeyword(clippingKeyword);
                DrawPivot(true);
                DrawPivot(false);
            }
        });

        GL.PopMatrix();

        void RenderTheObject () {
            Material drawMat = drawObjectAsWireFrame ? lineMaterialSolid : objectMat;
            Matrix4x4 newMVP;
            if(isExternalCamera){
                var otherMVP = otherCamera.cameraMatrix * otherCamera.modelMatrix;
                newMVP = cameraMatrix * otherMVP;
                if(showClipping){
                    drawMat.EnableKeyword(clippingKeyword);
                    drawMat.SetMatrix(clippingMatrixName, otherMVP);
                }else{
                    drawMat.DisableKeyword(clippingKeyword);
                }
            }else{
                newMVP = cameraMatrix * modelMatrix;
                drawMat.DisableKeyword(clippingKeyword);
            }
            drawMat.SetMatrix(modelMatrixName, isExternalCamera ? otherCamera.modelMatrix : modelMatrix);

            bool wireCache = GL.wireframe;
            GL.wireframe = drawObjectAsWireFrame;
            DrawWithNewMVPMatrix(newMVP, () => {
                drawMat.SetPass(0);
                DrawMesh(currentMesh, drawObjectAsWireFrame ? wireObjectColor : Color.white);
            });
            GL.wireframe = wireCache;
        }

        void DrawAllTheWireThings (bool seeThrough) {
            Material drawMat = seeThrough ? lineMaterialSeeThrough : lineMaterialSolid;
            drawMat.DisableKeyword(clippingKeyword);
            drawMat.SetPass(0);
            if(drawGridFloor){
                DrawWireFloor(seeThrough, drawOrigin);
            }
            if(drawOrigin){
                DrawAxes(seeThrough);
            }
            if(isExternalCamera){
                if(drawCamera && !matrixScreen.CameraMatrixFullyWeighted()){
                    DrawWithNewMVPMatrix(cameraMatrix * otherCamera.cameraMatrix * matrixScreen.GetUnweightedCameraMatrixForRendering().inverse, () => {
                        var camDrawColor = matrixScreen.CameraMatrixNotUnweighted() ? camFrustumColor : ((0.5f * camFrustumColor) + (0.5f * attachedUnityCam.backgroundColor));
                        DrawClipSpace(camDrawColor, seeThrough);
                    });
                }
                if(drawClipSpace){
                    DrawClipSpace(clipBoxColor, seeThrough);
                }
                if(otherCamera.drawGridFloor || otherCamera.drawOrigin){
                    if(showClipping){
                        drawMat.EnableKeyword(clippingKeyword);
                        drawMat.SetMatrix(clippingMatrixName, otherCamera.cameraMatrix);
                        drawMat.SetPass(0);
                    }
                    DrawWithNewMVPMatrix(cameraMatrix * otherCamera.cameraMatrix, () => {
                        if(otherCamera.drawGridFloor){
                            DrawWireFloor(seeThrough, otherCamera.drawOrigin);
                        }
                        if(otherCamera.drawOrigin){
                            DrawAxes(seeThrough);
                        }
                    });
                }
            }
        }
    }

#region GL_Drawing

    void DrawWireFloor (bool seeThrough, bool holdOutOrigin) {
        GLDraw(GL.LINES, () => {
            GL.Color(GetConditionalSeeThroughColor(wireGridColor, seeThrough));
            for(int x=-10; x<=10; x++){
                if(x == 0 && holdOutOrigin){
                    GL.Vertex3(x, 0, -10);
                    GL.Vertex3(x, 0, 0);
                    GL.Vertex3(x, 0, 1);
                    GL.Vertex3(x, 0, 10);
                }else{
                    GL.Vertex3(x, 0, -10);
                    GL.Vertex3(x, 0, 10);
                }
            }
            for(int z=-10; z<=10; z++){
                if(z == 0 && holdOutOrigin){
                    GL.Vertex3(-10, 0, z);
                    GL.Vertex3(0, 0, z);
                    GL.Vertex3(1, 0, z);
                    GL.Vertex3(10, 0, z);
                }else{
                    GL.Vertex3(-10, 0, z);
                    GL.Vertex3(10, 0, z);
                }
            }
        });
    }

    void DrawAxes (bool seeThrough) {
        GLDraw(GL.LINES, () => {
            GL.Color(GetConditionalSeeThroughColor(xColor, seeThrough));
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Color(GetConditionalSeeThroughColor(yColor, seeThrough));
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 1, 0);
            GL.Color(GetConditionalSeeThroughColor(zColor, seeThrough));
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 1);
        });
    }

    void DrawClipSpace (Color drawColor, bool seeThrough) {
        GLDraw(GL.LINES, () => {
            GL.Color(GetConditionalSeeThroughColor(drawColor, seeThrough));
            for(int i=0; i<4; i++){
                ClipVertLine(i, (i+1)%4);
                ClipVertLine(4+i, 4+(i+1)%4);
                ClipVertLine(i, i+4);
            }
        });    
        
        void ClipVertLine (int index1, int index2) {
            GL.Vertex(clipSpaceVertices[index1]);
            GL.Vertex(clipSpaceVertices[index2]);
        }
    }

    void DrawMesh (Mesh meshToDraw, Color drawColor) {
        GLDraw(GL.TRIANGLES, () => {
            GL.Color(drawColor);
            var verts = meshToDraw.vertices;
            var tris = meshToDraw.triangles;
            for(int i=0; i<tris.Length; i+=3){
                GL.Vertex(verts[tris[i+0]]);
                GL.Vertex(verts[tris[i+1]]);
                GL.Vertex(verts[tris[i+2]]);
            }
        });
    }
	
    void DrawPivot (bool seeThrough) {
        // should always have the same pixel-size
        float dist = (pivotPointToDraw - attachedUnityCam.transform.position).magnitude;
        float preMul = pivotSize * dist / Screen.height;
        preMul *= fieldOfView / 60;         // TODO not perfect but better than nothing. 
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
        GLDraw(GL.TRIANGLE_STRIP, () => {
            GL.Color(pivotColor * new Color(1,1,1, seeThrough ? seeThroughAlphaMultiplier : 1));
            GL.Vertex(points[3]);
            GL.Vertex(points[2]);
            GL.Vertex(points[0]);
            GL.Vertex(points[1]);
        });
        GLDraw(GL.LINE_STRIP, () => {
            GL.Color(pivotOutlineColor * new Color(1,1,1, seeThrough ? seeThroughAlphaMultiplier : 1));
            GL.Vertex(points[0]);
            GL.Vertex(points[1]);
            GL.Vertex(points[2]);
            GL.Vertex(points[3]);
            GL.Vertex(points[0]);
        });
    }

#endregion

    Color GetConditionalSeeThroughColor (Color inputColor, bool seeThrough) {
        return (seeThrough ? GetSeeThroughColor(inputColor) : inputColor);
    }

    Color GetSeeThroughColor (Color inputColor) {
        return new Color(inputColor.r, inputColor.g, inputColor.b, inputColor.a * seeThroughAlphaMultiplier);
    }

    void DrawWithNewMVPMatrix (Matrix4x4 newMVP, System.Action drawAction) {
        GL.PushMatrix();
        GL.LoadProjectionMatrix(Matrix4x4.identity);
        GL.LoadIdentity();
        GL.MultMatrix(newMVP);
        drawAction.Invoke();
        GL.PopMatrix();
    }

    void GLDraw (int drawMode, System.Action drawAction) {
        GL.Begin(drawMode);
        drawAction.Invoke();
        GL.End();
    }

}
