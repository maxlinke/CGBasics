using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMatrixInputModel : MonoBehaviour {

    [Header("Components")]
    [SerializeField] Image background;
    [SerializeField] Image headerBackground;
    [SerializeField] Image outline;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] TextMeshProUGUI labelDropShadow;
    [SerializeField] RawImage previewImage;
    [SerializeField] Button previewButton;

    [Header("Settings")]
    [SerializeField] bool wireframe;
    [SerializeField] bool perspectivePreview;
    [SerializeField] bool dynamicallyScalePreview;
    [SerializeField] Vector3 previewCamPosition;
    [SerializeField] Material meshPreviewMat;

    private RectTransform m_rectTransform;
    public RectTransform rectTransform => m_rectTransform;

    bool initialized = false;
    RenderTexture previewTex;
    Mesh previewMesh;
    MatrixScreen matrixScreen;
    float lastScale = 1;

    public void Initialize (MatrixScreen matrixScreen, Mesh mesh, string meshName, System.Action<Mesh> onMeshChanged) {
        this.matrixScreen = matrixScreen;
        this.m_rectTransform = GetComponent<RectTransform>();
        UpdateNameAndMesh(meshName, mesh);
        previewButton.onClick.AddListener(() => {
            ModelPicker.Open(
                (newMesh, newName) => {
                    if(newMesh != null){
                        UpdateNameAndMesh(newName, newMesh, true); 
                        onMeshChanged?.Invoke(newMesh);
                    }
                }, 
                (matrixScreen != null ? matrixScreen.zoomLevel : 1f)
            );
        });
        this.initialized = true;
    }

    public void UpdateNameAndMesh (string newName, Mesh newMesh, bool autoUpdatePreview = true) {
        if(newName == null){
            Debug.LogWarning("Name can't be null, aborting!", this.gameObject);
            return;
        }
        newName = newName.Trim();
        if(newName.Length < 1){
            Debug.LogWarning("Name can't be empty! Aborting...", this.gameObject);
            return;
        }
        label.text = newName;
        labelDropShadow.text = newName;
        previewMesh = newMesh;
        if(autoUpdatePreview){
            UpdatePreview();
        }
    }

    void Update () {
        if(initialized && (matrixScreen != null)){
            if(matrixScreen.zoomLevel != lastScale && dynamicallyScalePreview){
                UpdatePreview();
            }
        }
    }

    void LoadColors (ColorScheme cs) {
        label.color = cs.UiMatrixLabel;
        labelDropShadow.color = cs.UiMatrixLabelDropShadow;
        outline.color = cs.UiMatrixOutline;
        background.color = cs.UiMatrixBackground;
        headerBackground.color = cs.UiMatrixModelPreviewHeader;
        previewButton.SetFadeTransition(0f, cs.UiMatrixModelPreview, cs.UiMatrixModelPreviewHover, cs.UiMatrixModelPreviewClick, Color.magenta);
    }

    void OnEnable () {
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColors;
    }

    public void UpdatePreview () {
        EnsurePreviewMatLoaded();
        UpdatePreviewTex();
        var rtCache = RenderTexture.active;
        RenderTexture.active = previewTex;
        var wireCache = GL.wireframe;
        GL.wireframe = wireframe;
        GL.Clear(
            clearDepth: true,
            clearColor: true,
            backgroundColor: Color.clear
        );
        GL.PushMatrix();
        GL.LoadProjectionMatrix(GetProjectionMatrix());
        GL.LoadIdentity();
        GL.MultMatrix(GetModelViewMatrix());
        meshPreviewMat.SetPass(0);
        if(previewMesh != null){
            CustomGLCamera.DrawMesh(previewMesh, Color.white);
        }
        GL.PopMatrix();
        GL.wireframe = wireCache;
        previewImage.texture = previewTex;
        RenderTexture.active = rtCache;

        void UpdatePreviewTex () {
            if(dynamicallyScalePreview){
                if(previewTex != null){
                    previewTex.Release();
                }
                float newScale = (matrixScreen != null ? matrixScreen.zoomLevel : 1f);
                previewTex = CreateTex(newScale, FilterMode.Point);
            }else{
                if(previewTex == null){
                    float newScale = (matrixScreen != null ? matrixScreen.maxmumZoomLevel : 1f);
                    previewTex = CreateTex(newScale, FilterMode.Bilinear);
                }
            }

            RenderTexture CreateTex (float inputTexScale, FilterMode inputFilterMode) {
                var previewDimensions = previewImage.GetComponent<RectTransform>().rect;
                var output = new RenderTexture(
                    width: (int)(previewDimensions.width * inputTexScale), 
                    height: (int)(previewDimensions.height * inputTexScale),
                    depth: 32,
                    format: RenderTextureFormat.ARGB32,
                    readWrite: RenderTextureReadWrite.Default
                );
                output.filterMode = inputFilterMode;
                lastScale = inputTexScale;
                return output;
            }
        }

        Matrix4x4 GetProjectionMatrix () {
            if(perspectivePreview){
                return GLMatrixCreator.GetProjectionMatrix(
                    fov: 45f, 
                    aspectRatio: (float)(previewTex.width) / previewTex.height,
                    zNear: 0.1f,
                    zFar: 100f
                );
            }else{
                return GLMatrixCreator.GetOrthoProjectionMatrix(
                    orthoSize: 2f,
                    aspect: (float)(previewTex.width) / previewTex.height,
                    zNear: 0.1f,
                    zFar: 100f
                );
            }
        }

        Matrix4x4 GetModelViewMatrix () {
            var view = GLMatrixCreator.GetLookAtMatrix(
                eye: previewCamPosition,
                center: Vector3.zero,
                up: Vector3.up
            );
            var translate = GLMatrixCreator.GetTranslationMatrix(-previewMesh.bounds.center);
            var scale = GLMatrixCreator.GetScaleMatrix(Vector3.one / previewMesh.bounds.extents.magnitude);
            return (view * scale * translate);     // scale and translate are NORMALLY the other way round, but not here!
        }
    }

    void EnsurePreviewMatLoaded () {
        if(meshPreviewMat != null){
            return;
        }
        var shader = Shader.Find("Hidden/Internal-Colored");
        meshPreviewMat = new Material(shader);
        meshPreviewMat.hideFlags = HideFlags.HideAndDontSave;
        meshPreviewMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        meshPreviewMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        meshPreviewMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        meshPreviewMat.SetInt("_ZWrite", 1);
        meshPreviewMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
    }
	
}
