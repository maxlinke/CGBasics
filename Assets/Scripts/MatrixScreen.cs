using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatrixScreen : MonoBehaviour {

    private const int MAX_MATRIX_COUNT = 8;    // TODO raise to 16 (and maybe also increase the max variable count to 16?)

    [Header("Prefabs")]
    [SerializeField] UIMatrix uiMatrixPrefab;
    [SerializeField] UIMatrixGroup matrixGroupPrefab;

    [Header("Components")]
    [SerializeField] Camera matrixCam;
    [SerializeField] Camera externalCam;
    [SerializeField] Image backgroundImage;
    [SerializeField] MeshFilter referenceObject;
    [SerializeField] RectTransform uiMatrixParent;                  // TODO matrix groups (object, camera)
    [SerializeField] MatrixScreenPanAndZoom panAndZoomController;

    [Header("Settings")]
    [SerializeField] float multiplicationSignSize;
    [SerializeField] float matrixGroupMargin;

    public float matrixZoom => panAndZoomController.zoomLevel;

    bool initialized;
    UIMatrixGroup modelGroup;
    UIMatrixGroup camGroup;
    Image multiplicationSignImage;

    void OnEnable () {
        if(!initialized){
            Initialize();
        }
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColors;
    }

    void Initialize () {
        if(initialized){
            Debug.LogWarning($"Duplicate init call for {nameof(MatrixScreen)}, aborting!", this.gameObject);
            return;
        }
        matrixCam.GetComponent<CustomGLCamera>().matrixScreen = this;
        externalCam.GetComponent<CustomGLCamera>().matrixScreen = this;

        CreateMultiplicationSign();
        modelGroup = CreateMatrixGroup(leftSide: true);
        modelGroup.SetName("Model");
        modelGroup[0].LoadConfig(UIMatrices.MatrixConfig.scaleConfig);
        modelGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.fullRotationConfig, UIMatrix.Editability.FULL, 1, false);
        modelGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.translationConfig, UIMatrix.Editability.FULL, 2, true);

        camGroup = CreateMatrixGroup(leftSide: false);
        camGroup.SetName("Camera");
        camGroup[0].LoadConfig(UIMatrices.MatrixConfig.rebaseConfig);
        camGroup[0].Transpose();
        camGroup[0].SetName("Inv. Camera Rotation");
        camGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.inverseTranslationConfig, UIMatrix.Editability.FULL, 1, false);
        camGroup[1].SetName("Inv. Camera Position");
        camGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.perspProjConfig, UIMatrix.Editability.FULL, 2, true);

        foreach(var m in modelGroup){
            m.VariableContainer.Retract();
        }
        foreach(var m in camGroup){
            m.VariableContainer.Retract();
        }
        
        // TODO non-editability for all as soon as i get the "free mode /locked mode" thingy working

        initialized = true;

        UIMatrixGroup CreateMatrixGroup (bool leftSide) {
            var newGroup = Instantiate(matrixGroupPrefab);
            newGroup.rectTransform.SetParent(uiMatrixParent, false);
            newGroup.rectTransform.SetAnchor(0.5f * Vector2.one);
            newGroup.rectTransform.pivot = new Vector2(leftSide ? 1f : 0f, 0.5f);
            newGroup.rectTransform.anchoredPosition = new Vector2((leftSide ? -1f : 1f) * (matrixGroupMargin + multiplicationSignSize / 2f), 0f);
            newGroup.rectTransform.localScale = Vector3.one;
            newGroup.Initialize(this);
            return newGroup;
        }

        void CreateMultiplicationSign () {
            var mulSignRT = new GameObject("Multiplication Sign", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            mulSignRT.SetParent(uiMatrixParent, false);
            mulSignRT.localScale = Vector3.one;
            mulSignRT.SetAnchor(0.5f * Vector2.one);
            mulSignRT.pivot = 0.5f * Vector2.one;
            mulSignRT.anchoredPosition = Vector2.zero;
            mulSignRT.sizeDelta = Vector2.one * multiplicationSignSize;
            multiplicationSignImage = mulSignRT.gameObject.GetComponent<Image>();
            multiplicationSignImage.sprite = UISprites.MatrixMultiply;
        }
    }

    void LoadColors (ColorScheme cs) {
        backgroundImage.color = cs.MatrixScreenBackground;
        multiplicationSignImage.color = cs.MatrixScreenMultiplicationSign;
        modelGroup.LoadColors(cs.MatrixScreenModelMatrixHeader, cs);
        camGroup.LoadColors(cs.MatrixScreenCameraMatrixHeader, cs);
    }

    public void AddMatrix (UIMatrix callingMatrix) {
        if(callingMatrix.matrixGroup.TryGetIndexOf(callingMatrix, out var index)){
            callingMatrix.matrixGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.identityConfig, UIMatrix.Editability.FULL, index + 1);
            UpdateMatrixAddButtons();
        }else{
            Debug.LogError("wat");
        }
    }

    public void DeleteMatrix (UIMatrix matrixToDelete) {
        matrixToDelete.matrixGroup.DeleteMatrix(matrixToDelete);
        UpdateMatrixAddButtons();
    }

    public void MoveMatrixLeft (UIMatrix matrixToMove) {

    }

    public void MoveMatrixRight (UIMatrix matrixToMove) {

    }

    void UpdateMatrixAddButtons () {
        bool cantAddMore = modelGroup.matrixCount + camGroup.matrixCount >= MAX_MATRIX_COUNT;
        foreach(var m in modelGroup){
            m.addButtonBlocked = cantAddMore;
        }
        foreach(var m in camGroup){
            m.addButtonBlocked = cantAddMore;
        }
    }

    // TODO remember to transpose
    public void GetCurrentMeshAndWeightedMatrices (out Mesh outputMesh, out Matrix4x4 outputModelMatrix, out Matrix4x4 outputCameraMatrix) {
        outputMesh = referenceObject.sharedMesh;
        var refT = referenceObject.transform;
        outputModelMatrix = GLMatrixCreator.GetModelMatrix(refT.position, refT.eulerAngles, refT.lossyScale);
        outputCameraMatrix = Matrix4x4.identity;    // TODO this
    }
	
}
