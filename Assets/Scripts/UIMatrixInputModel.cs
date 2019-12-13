using System.Collections;
using System.Collections.Generic;
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

    [Header("Settings")]
    [SerializeField] float previewTexScale;
    [SerializeField] Vector3 previewCamPosition;
    [SerializeField] Material meshPreviewMat;

    [Header("TEMP")]
    [SerializeField] bool selfInit;
    [SerializeField] Mesh meshToSelfInitWith;
    [SerializeField] string meshNameToSelfInitWith;

    private RectTransform m_rectTransform;
    public RectTransform rectTransform => m_rectTransform;

    bool initialized = false;
    System.Action<Mesh> onMeshChanged;
    // Material meshPreviewMat;
    RenderTexture previewTex;

    void Initialize (Mesh mesh, string meshName, System.Action<Mesh> onMeshChanged) {
        var previewDimensions = previewImage.GetComponent<RectTransform>().rect;
        previewTex = new RenderTexture(
            width: (int)(previewDimensions.width * previewTexScale), 
            height: (int)(previewDimensions.height * previewTexScale),
            depth: 32,
            format: RenderTextureFormat.ARGB32,
            readWrite: RenderTextureReadWrite.Default
        );
        UpdatePreview(mesh);
        UpdateName(meshName);
        this.onMeshChanged = onMeshChanged;
        this.initialized = true;
    }

    void UpdateName (string newName) {
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
        UpdateHeaderBackgroundColor(ColorScheme.current);
    }

    void LoadColors (ColorScheme cs) {
        label.color = cs.UiMatrixLabel;
        labelDropShadow.color = cs.UiMatrixLabelDropShadow;
        outline.color = cs.UiMatrixOutline;
        background.color = cs.UiMatrixBackground;
        UpdateHeaderBackgroundColor(ColorScheme.current);
    }

    void UpdateHeaderBackgroundColor (ColorScheme cs) {
        headerBackground.color = cs.UiMatrixHeaders.FromStringHash(label.text);
    }

    void OnEnable () {
        if(!initialized && selfInit){
            Initialize(meshToSelfInitWith, meshNameToSelfInitWith, null);
        }
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColors;
    }

    void UpdatePreview (Mesh meshToUse) {
        EnsurePreviewMatLoaded();
        var rtCache = RenderTexture.active;
        RenderTexture.active = previewTex;
        GL.Clear(
            clearDepth: true,
            clearColor: true,
            backgroundColor: Color.clear
        );
        GL.PushMatrix();
        GL.LoadProjectionMatrix(Matrix4x4.identity);
        GL.LoadIdentity();
        var proj = GLMatrixCreator.GetProjectionMatrix(
            fov: 45f, 
            aspectRatio: (float)(previewTex.width) / previewTex.height,
            zNear: 0.1f,
            zFar: 100f
        );
        var view = GLMatrixCreator.GetLookAtMatrix(
            eye: previewCamPosition,
            center: Vector3.zero,
            up: Vector3.up
        );
        var translate = GLMatrixCreator.GetTranslationMatrix(-meshToUse.bounds.center);
        var scale = GLMatrixCreator.GetScaleMatrix(Vector3.one / meshToUse.bounds.extents.magnitude);
        GL.MultMatrix(proj * view * translate * scale);
        meshPreviewMat.SetPass(0);
        CustomGLCamera.DrawMesh(meshToUse, Color.white);
        GL.PopMatrix();
        previewImage.texture = previewTex;
        RenderTexture.active = rtCache;
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
