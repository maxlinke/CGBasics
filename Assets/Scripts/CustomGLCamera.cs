﻿using UnityEngine;
using UIMatrices;

public class CustomGLCamera : MonoBehaviour {

    const string clippingKeyword = "SHOW_CLIPPING";
    const string specialClippingMatrixKeyword = "USE_SPECIAL_CLIPPING_MATRIX";
    const string clippingMatrixName = "_SpecialClippingMatrix";
    const string specialModelMatrixKeyword = "USE_SPECIAL_MODEL_MATRIX";
    const string modelMatrixName = "_SpecialModelMatrix";
    const string inverseModelMatrixName = "_SpecialInverseModelMatrix";
    const string camPosName = "_SpecialCamPos";

    const string litBackfaceKeyword = "LIT_BACKFACES";
    const string solidBackfaceKeyword = "SOLID_BACKFACES";

    #if UNITY_WEBGL
        public const bool allowMSAA = false;
    #else
        public const bool allowMSAA = true;
    #endif
    

    [SerializeField] Material objectMatTemplate;
    [SerializeField] Material objectMatTemplateWebGL;

    Material objectMat;
    BackfaceCullingMode currentCullingMode;

    public Material ObjectRenderMaterial { get; private set; }
    public bool IsExternalCamera => isExternalCamera;
    public bool matricesAreUpdated { get; private set; }
    public Matrix4x4 modelMatrix { get; private set; }
    public Matrix4x4 cameraMatrix { get; private set; }
    private Mesh currentMesh;

    public float nearClipPlane { 
        get {
            return attachedUnityCam.nearClipPlane;
        } set {
            attachedUnityCam.nearClipPlane = value;
        }
    }

    public float farClipPlane { 
        get {
            return attachedUnityCam.farClipPlane;
        } set {
            attachedUnityCam.farClipPlane = value;
        }
    }

    public float fieldOfView { 
        get {
            return attachedUnityCam.fieldOfView;
        } set {
            attachedUnityCam.fieldOfView = value;
        }
    }

    public float orthoSize { 
        get {
            return attachedUnityCam.orthographicSize;
        } set {
            attachedUnityCam.orthographicSize = value;
        }
    }

    public float aspect { 
        get {
            return attachedUnityCam.aspect;
        } set {
            attachedUnityCam.aspect = value;
        }
    }

    [System.NonSerialized] public bool drawPivot;
    [System.NonSerialized] public Vector3 pivotPointToDraw;

    [System.NonSerialized] public bool drawSeeThrough;
    [System.NonSerialized] public bool drawObjectAsWireFrame;
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
    float startOrthoSize;
    Vector3 startPosition;
    Quaternion startRotation;

    const float seeThroughAlphaMultiplier = 0.333f;
    const float pointSize = 8f;

    Color matFrontFaces;
    Color matBackFaces;

    Color wireGridColor;
    Color wireObjectColor;
    Color camFrustumColor;
    Color clipBoxColor;
    Color xColor;
    Color yColor;
    Color zColor;
    Color pivotColor;
    Color pivotOutlineColor;
    Color clipOverlayColor;
    Color vectorColor;
    Color vectorOutlineColor;

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

    public void Initialize (MatrixScreen matrixScreen, bool isExternalCamera, CustomGLCamera otherCamera, float inputFOV, float inputNearClip, float inputFarClip, Vector3 inputStartPos, float inputOrthoSize) {
        if(initialized){
            Debug.LogError("Duplicate init call, aborting!", this.gameObject);
            return;
        }
        this.matrixScreen = matrixScreen;
        this.isExternalCamera = isExternalCamera;
        this.otherCamera = otherCamera;

        EnsureUnityCamLoaded();
        SetupPremadeUnityColoredMaterials();        

        #if UNITY_WEBGL
            objectMat = Instantiate(objectMatTemplateWebGL);
            ObjectRenderMaterial = Instantiate(objectMatTemplateWebGL);
        #else
            objectMat = Instantiate(objectMatTemplate);
            ObjectRenderMaterial = Instantiate(objectMatTemplate);
        #endif
        DoForBothMaterials((m) => {m.hideFlags = HideFlags.HideAndDontSave;});
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
        orthoSize = inputOrthoSize;
        transform.position = inputStartPos;
        transform.rotation = Quaternion.LookRotation(-inputStartPos, Vector3.up);

        startNearClipPlane = nearClipPlane;
        startFarClipPlane = farClipPlane;
        startFieldOfView = fieldOfView;
        startPosition = transform.position;
        startRotation = transform.rotation;
        startOrthoSize = inputOrthoSize;

        initialized = true;
    }

    void DoForBothMaterials (System.Action<Material> doAction) {
        doAction(objectMat);
        doAction(ObjectRenderMaterial);
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

    void OnDestroy () {
        DestroyImmediate(objectMat);
        DestroyImmediate(ObjectRenderMaterial);
        DestroyImmediate(lineMaterialSeeThrough);
        DestroyImmediate(lineMaterialSolid);
    }

    public void ResetToDefault () {
        if(!initialized){
            Debug.LogError("Not initialized yet! Aborting...", this.gameObject);
            return;
        }
        nearClipPlane = startNearClipPlane;
        farClipPlane = startFarClipPlane;
        fieldOfView = startFieldOfView;
        orthoSize = startOrthoSize;
        attachedUnityCam.ResetAspect();
        transform.position = startPosition;
        transform.rotation = startRotation;
    }

    void EnsureUnityCamLoaded () {
        if(attachedUnityCam == null){
            attachedUnityCam = GetComponent<Camera>();
            attachedUnityCam.allowMSAA = CustomGLCamera.allowMSAA;
        }
    }

    public void LoadColors (ColorScheme cs) {
        EnsureUnityCamLoaded();
        attachedUnityCam.backgroundColor = cs.ApplicationBackground;
        wireGridColor = cs.RenderWireFloor;
        wireObjectColor = cs.VertRenderWireObject;
        camFrustumColor = cs.VertRenderCameraFrustum;
        clipBoxColor = cs.VertRenderClipSpaceBox;
        xColor = cs.RenderXAxis;
        yColor = cs.RenderYAxis;
        zColor = cs.RenderZAxis;
        pivotColor = cs.VertRenderPivot;
        pivotOutlineColor = cs.VertRenderPivotOutline;
        vectorColor = cs.VertRenderVectorPoint;
        vectorOutlineColor = cs.VertRenderVectorPointOutline;

        matFrontFaces = cs.VertRenderObjectColor;
        matBackFaces = cs.VertRenderObjectBackfaceColor;
        DoForBothMaterials((m) => {m.SetColor("_FrontColor", matFrontFaces);});
        DoForBothMaterials((m) => {m.SetColor("_LightColorFront", cs.VertRenderLight1);});
        DoForBothMaterials((m) => {m.SetColor("_LightColorBack", cs.VertRenderLight2);});
        DoForBothMaterials((m) => {m.SetColor("_LightColorAmbient", cs.VertRenderAmbientLight);});
        clipOverlayColor = cs.VertRenderClippingOverlay;
        DoForBothMaterials((m) => {m.SetColor("_ClippingOverlayColor", clipOverlayColor);});
        lineMaterialSolid.SetColor("_ClippingOverlayColor", clipOverlayColor);
        lineMaterialSeeThrough.SetColor("_ClippingOverlayColor", clipOverlayColor);
        UpdateBackfaceCulling(currentCullingMode);
    }

    public enum BackfaceCullingMode {
        Off,
        On,
        Highlight
    }

    public void UpdateBackfaceCulling (BackfaceCullingMode inputMode) {
        switch(inputMode){
            case BackfaceCullingMode.Off:
                DoForBothMaterials((m) => {m.SetColor("_BackColor", matFrontFaces);});
                DoForBothMaterials((m) => {m.SetKeywordEnabled(solidBackfaceKeyword, true);});
                DoForBothMaterials((m) => {m.SetKeywordEnabled(litBackfaceKeyword, true);});
                DoForBothMaterials((m) => {m.SetInt("_Cull", (int)(UnityEngine.Rendering.CullMode.Off));});
                break;
            case BackfaceCullingMode.On: 
                DoForBothMaterials((m) => {m.SetColor("_BackColor", matFrontFaces);});
                DoForBothMaterials((m) => {m.SetKeywordEnabled(solidBackfaceKeyword, true);});
                DoForBothMaterials((m) => {m.SetKeywordEnabled(litBackfaceKeyword, true);});
                DoForBothMaterials((m) => {m.SetInt("_Cull", (int)(UnityEngine.Rendering.CullMode.Back));});
                break;
            case BackfaceCullingMode.Highlight:
                DoForBothMaterials((m) => {m.SetColor("_BackColor", matBackFaces);});
                DoForBothMaterials((m) => {m.SetKeywordEnabled(solidBackfaceKeyword, false);});
                DoForBothMaterials((m) => {m.SetKeywordEnabled(litBackfaceKeyword, false);});
                DoForBothMaterials((m) => {m.SetInt("_Cull", (int)(UnityEngine.Rendering.CullMode.Off));});
                break;
            default:
                throw new System.ArgumentException($"Unknown {nameof(BackfaceCullingMode)} \"{inputMode}\"!");
        }
        this.currentCullingMode = inputMode;
    } 

    void SetupPremadeUnityColoredMaterials () {
        lineMaterialSolid = GetLineMaterial(false);
        lineMaterialSeeThrough = GetLineMaterial(true);
    }

    public static Material GetLineMaterial (bool seeThrough) {
        // modified from https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnPostRender.html
        var shader = Shader.Find("Custom/InternalColoredWithCulling");
        var output = new Material(shader);
        output.hideFlags = HideFlags.HideAndDontSave;
        output.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        output.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        output.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        output.SetInt("_ZWrite", seeThrough ? 0 : 1);
        output.SetInt("_ZTest", seeThrough ? (int)UnityEngine.Rendering.CompareFunction.Always : (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        return output;
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
            otherCamera.UpdateMatricesAndMesh();
        }
        UpdateMatricesAndMesh();
    }

    void UpdateMatricesAndMesh () {
        if(!isExternalCamera && !matrixScreen.FreeMode){
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
            if(matrixScreen.OrthoMode){
                matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.OrthographicProjectionConfig.orthoSize, orthoSize, false);
                matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.OrthographicProjectionConfig.nearClip, nearClipPlane, false);
                matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.OrthographicProjectionConfig.farClip, farClipPlane, false);
                matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.OrthographicProjectionConfig.aspect, aspect, true);
            }else{
                matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.fov, fieldOfView, false);
                matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.nearClip, nearClipPlane, false);
                matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.farClip, farClipPlane, false);
                matrixScreen.ProjMatrix.VariableContainer.EditVariable(MatrixConfig.PerspectiveProjectionConfig.aspect, aspect, true);
            }
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

        if(matrixScreen.VectorMode){
            RenderTheVector();
        }else if(currentMesh != null){            
            RenderTheObject();
        }

        DrawWithNewMVPMatrix(cameraMatrix, () => {
            if(drawSeeThrough){
                DrawAllTheWireThings(true);
            }
            DrawAllTheWireThings(false);
        });

        if(drawPivot){
            Vector4 v4Pivot = new Vector4(pivotPointToDraw.x, pivotPointToDraw.y, pivotPointToDraw.z, 1f);
            RenderPoint(v4Pivot, pivotColor, pivotOutlineColor, true);
        }

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
                objectMat.SetVector(camPosName, otherCamera.transform.position);
            }else{
                newMVP = cameraMatrix * modelMatrix;
                drawMat.DisableKeyword(clippingKeyword);
                objectMat.SetVector(camPosName, transform.position);
            }
            var mm = isExternalCamera ? otherCamera.modelMatrix : modelMatrix;
            drawMat.SetMatrix(modelMatrixName, mm);
            drawMat.SetMatrix(inverseModelMatrixName, mm.inverse);

            bool wireCache = GL.wireframe;
            GL.wireframe = drawObjectAsWireFrame;
            DrawWithNewMVPMatrix(newMVP, () => {
                drawMat.SetPass(0);
                var meshColor = drawObjectAsWireFrame ? wireObjectColor : Color.white;
                #if UNITY_WEBGL
                    if(drawObjectAsWireFrame){
                        DrawMeshManualWireframe(currentMesh, meshColor);
                    }else{
                        DrawMeshWithNormalsAsColors(currentMesh);
                    }
                #else
                    DrawMesh(currentMesh, meshColor);
                #endif
            });
            GL.wireframe = wireCache;
        }

        void RenderTheVector () {
            Vector4 vmVec;
            Color mCol = vectorColor;
            Color oCol = vectorOutlineColor;
            if(isExternalCamera){
                Matrix4x4 otherMVP = otherCamera.cameraMatrix * otherCamera.modelMatrix;
                vmVec = otherMVP * matrixScreen.VectorModeVector;
                Vector3 vmVecCPos = new Vector3(vmVec.x, vmVec.y, vmVec.z);
                if(vmVec.w > 0){
                    vmVecCPos /= vmVec.w;
                    bool clipped = Mathf.Abs(vmVecCPos.x) > 1 || Mathf.Abs(vmVecCPos.y) > 1 || Mathf.Abs(vmVecCPos.z) > 1;
                    if(clipped && showClipping){
                        mCol = mCol.AlphaOver(clipOverlayColor);
                        oCol = oCol.AlphaOver(clipOverlayColor);
                    }
                    RenderPoint(vmVec, mCol, oCol, true);
                }
            }else{
                vmVec = modelMatrix * matrixScreen.VectorModeVector;
                RenderPoint(vmVec, mCol, oCol, true);
            }
        }

        void RenderPoint (Vector4 inputPoint, Color mainColor, Color outlineColor, bool alsoSeeThrough) {
            Vector4 clipSpaceInput = this.cameraMatrix * inputPoint;
            Vector3 drawPoint = new Vector3(clipSpaceInput.x, clipSpaceInput.y, clipSpaceInput.z);
            if(inputPoint.w != 0){
                drawPoint /= clipSpaceInput.w;
            }
            if(alsoSeeThrough){
                DrawThePoint(true);
            }
            DrawThePoint(false);

            void DrawThePoint (bool seeThrough) {
                Material drawMat = seeThrough ? lineMaterialSeeThrough : lineMaterialSolid;
                bool clipCache = drawMat.IsKeywordEnabled(clippingKeyword);
                drawMat.DisableKeyword(clippingKeyword);
                DrawWithNewMVPMatrix(Matrix4x4.identity, () => {
                    drawMat.SetPass(0);
                    DrawClipspacePoint(drawPoint, GetConditionalSeeThroughColor(mainColor, seeThrough), GetConditionalSeeThroughColor(outlineColor, seeThrough));
                });
                if(clipCache){
                    drawMat.EnableKeyword(clippingKeyword);
                }
            }
        }

        void DrawAllTheWireThings (bool seeThrough) {
            Material drawMat = seeThrough ? lineMaterialSeeThrough : lineMaterialSolid;
            drawMat.DisableKeyword(clippingKeyword);
            drawMat.SetPass(0);
            if(drawGridFloor){
                DrawWireFloor(wireGridColor, seeThrough, drawOrigin);
            }
            if(drawOrigin){
                DrawAxes(xColor, yColor, zColor, seeThrough);
            }
            if(isExternalCamera){
                if(drawCamera && !matrixScreen.CameraMatrixFullyWeighted()){
                    DrawWithNewMVPMatrix(cameraMatrix * otherCamera.cameraMatrix * matrixScreen.GetUnweightedCameraMatrixForRendering().inverse, () => {
                        DrawClipSpace(camFrustumColor, seeThrough);
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
                            DrawWireFloor(wireGridColor, seeThrough, otherCamera.drawOrigin || (this.drawOrigin && !matrixScreen.CameraMatrixNotUnweighted()));
                        }
                        if(otherCamera.drawOrigin){
                            DrawAxes(xColor, yColor, zColor, seeThrough);
                        }
                    });
                }
            }
        }
    }

#region GL_Drawing

    public static void DrawWireFloor (Color inputColor, bool seeThrough, bool holdOutOrigin) {
        GLDraw(GL.LINES, () => {
            GL.Color(GetConditionalSeeThroughColor(inputColor, seeThrough));
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

    public static void DrawAxes (Color xColor, Color yColor, Color zColor, bool seeThrough) {
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

    public static void DrawMesh (Mesh meshToDraw, Color drawColor) {
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

    public static void DrawMeshWithNormalsAsColors (Mesh meshToDraw) {
        GLDraw(GL.TRIANGLES, () => {
            var verts = meshToDraw.vertices;
            var norms = meshToDraw.normals;
            var tris = meshToDraw.triangles;
            for(int i=0; i<tris.Length; i+=3){
                var n1 = (norms[tris[i+0]] + Vector3.one) / 2f;
                var n2 = (norms[tris[i+1]] + Vector3.one) / 2f;
                var n3 = (norms[tris[i+2]] + Vector3.one) / 2f;
                GL.Color(new Color(n1.x, n1.y, n1.z));              // if the colors come before the vertices, it's like bad flat shading...
                GL.Vertex(verts[tris[i+0]]);
                GL.Color(new Color(n2.x, n2.y, n2.z));
                GL.Vertex(verts[tris[i+1]]);
                GL.Color(new Color(n3.x, n3.y, n3.z));
                GL.Vertex(verts[tris[i+2]]);
            }
        });
    }

    public static void DrawMeshManualWireframe (Mesh meshToDraw, Color drawColor) {
        GLDraw(GL.LINES, () => {
            GL.Color(drawColor);
            var verts = meshToDraw.vertices;
            var tris = meshToDraw.triangles;
            for(int i=0; i<tris.Length; i+=3){
                var v1 = verts[tris[i+0]];
                var v2 = verts[tris[i+1]];
                var v3 = verts[tris[i+2]];
                GL.Vertex(v1);
                GL.Vertex(v2);
                GL.Vertex(v2);
                GL.Vertex(v3);
                GL.Vertex(v3);
                GL.Vertex(v1);
            }
        });
    }

    public static void DrawClipspacePoint (Vector3 point, Color mainColor, Color outlineColor) {
        float fOffsetV = 2f * pointSize / Screen.height;
        float fOffsetH = 2f * pointSize / Screen.width;
        Vector3 offsetV = new Vector3(0f, fOffsetV, 0f);
        Vector3 offsetH = new Vector3(fOffsetH, 0f, 0f);
        Vector3[] points = new Vector3[]{
            point + offsetV,
            point + offsetH,
            point - offsetV,
            point - offsetH
        };
        GLDraw(GL.TRIANGLE_STRIP, () => {
            GL.Color(mainColor);
            GL.Vertex(points[3]);
            GL.Vertex(points[2]);
            GL.Vertex(points[0]);
            GL.Vertex(points[1]);
        });
        GLDraw(GL.LINE_STRIP, () => {
            GL.Color(outlineColor);
            GL.Vertex(points[0]);
            GL.Vertex(points[1]);
            GL.Vertex(points[2]);
            GL.Vertex(points[3]);
            GL.Vertex(points[0]);
        });
    }

#endregion

    public static Color GetConditionalSeeThroughColor (Color inputColor, bool seeThrough) {
        return (seeThrough ? GetSeeThroughColor(inputColor) : inputColor);
    }

    public static Color GetSeeThroughColor (Color inputColor) {
        return new Color(inputColor.r, inputColor.g, inputColor.b, inputColor.a * seeThroughAlphaMultiplier);
    }

    public static void DrawWithNewMVPMatrix (Matrix4x4 newMVP, System.Action drawAction) {
        GL.PushMatrix();
        GL.LoadProjectionMatrix(Matrix4x4.identity);
        GL.LoadIdentity();
        GL.MultMatrix(newMVP);
        drawAction.Invoke();
        GL.PopMatrix();
    }

    public static void GLDraw (int drawMode, System.Action drawAction) {
        GL.Begin(drawMode);
        drawAction.Invoke();
        GL.End();
    }

}
