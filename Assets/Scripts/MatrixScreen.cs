using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatrixScreen : MonoBehaviour {

    private const int MAX_MATRIX_COUNT = 8;    // TODO raise to 16 (and maybe also increase the max variable count to 16?)

    [Header("Prefabs")]
    [SerializeField] UIMatrix uiMatrixPrefab;
    [SerializeField] UIMatrixGroup matrixGroupPrefab;

    [Header("Components")]
    [SerializeField] CustomCameraUIController matrixCamController;
    [SerializeField] CustomCameraUIController externalCamController;
    [SerializeField] Image backgroundImage;
    [SerializeField] Mesh defaultMesh;
    [SerializeField] RectTransform uiMatrixParent;
    [SerializeField] MatrixScreenPanAndZoom panAndZoomController;
    [SerializeField] Image[] borders;

    [Header("Settings")]
    [SerializeField] float multiplicationSignSize;
    [SerializeField] float matrixGroupMargin;

    public float matrixZoom => panAndZoomController.zoomLevel;
    public UIMatrix ViewPosMatrix => viewPosMatrix;
    public UIMatrix ViewRotMatrix => viewRotMatrix;
    public UIMatrix ProjMatrix => projMatrix;

    private bool cantAddMoreMatrices => modelGroup.matrixCount + camGroup.matrixCount >= MAX_MATRIX_COUNT;

    public bool freeModeActivated { get; private set; }

    bool initialized;
    UIMatrixGroup modelGroup;
    UIMatrixGroup camGroup;
    UIMatrix viewPosMatrix;
    UIMatrix viewRotMatrix;
    UIMatrix projMatrix;
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

    void Update () {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M)){
            if(freeModeActivated){
                ActivateNonFreeMode();
            }else{
                ActivateFreeMode();
            }
        }
    }

    void Initialize () {
        if(initialized){
            Debug.LogWarning($"Duplicate init call for {nameof(MatrixScreen)}, aborting!", this.gameObject);
            return;
        }
        matrixCamController.Initialize(this);
        externalCamController.Initialize(this);
        CreateMultiplicationSign();
        modelGroup = CreateMatrixGroup(leftSide: true);
        modelGroup.SetName("Model");
        camGroup = CreateMatrixGroup(leftSide: false);
        camGroup.SetName("Camera");
        ActivateNonFreeMode();
        UpdateMatrixButtons();
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
            if(multiplicationSignImage != null){
                Debug.LogError("wat", this.gameObject);
                return;
            }
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

    void ActivateNonFreeMode () {
        modelGroup.ResetToOnlyOneMatrix(false);
        modelGroup[0].LoadConfig(UIMatrices.MatrixConfig.scaleConfig);
        modelGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.fullRotationConfig, UIMatrix.Editability.FULL, 1, false);
        modelGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.translationConfig, UIMatrix.Editability.FULL, 2, true);

        camGroup.ResetToOnlyOneMatrix(false);
        // camGroup[0].LoadConfig(UIMatrices.MatrixConfig.rebaseConfig);
        // camGroup[0].Transpose();
        // camGroup[0].SetName("Inv. Camera Rotation");
        // camGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.inverseTranslationConfig, UIMatrix.Editability.FULL, 1, false);
        // camGroup[1].SetName("Inv. Camera Position");
        camGroup[0].LoadConfig(UIMatrices.MatrixConfig.inverseTranslationConfig);
        camGroup[0].SetName("Inv. Camera Position");
        camGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.rebaseConfig, UIMatrix.Editability.FULL, 1, false);
        camGroup[1].SetName("Inv. Camera Rotation");
        camGroup[1].Transpose();
        camGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.perspProjConfig, UIMatrix.Editability.FULL, 2, true);

        viewRotMatrix = camGroup[1];
        viewPosMatrix = camGroup[0];
        projMatrix = camGroup[2];

        foreach(var m in modelGroup){
            m.editability = UIMatrix.Editability.VARIABLE_VALUES_ONLY;
            m.VariableContainer.Expand();
        }
        foreach(var m in camGroup){
            m.editability = UIMatrix.Editability.NONE;
            m.VariableContainer.Retract();
        }
        matrixCamController.enabled = true;
        matrixCamController.ResetCamera();
        externalCamController.enabled = true;
        // externalCamController.ResetCamera();

        freeModeActivated = false;
    }

    void ActivateFreeMode () {
        viewRotMatrix = null;
        viewPosMatrix = null;
        projMatrix = null;

        foreach(var m in modelGroup){
            m.editability = UIMatrix.Editability.FULL;
            m.VariableContainer.Expand();
        }
        foreach(var m in camGroup){
            m.editability = UIMatrix.Editability.FULL;
            m.VariableContainer.Expand();
        }
        matrixCamController.enabled = false;
        externalCamController.enabled = true;

        freeModeActivated = true;
    }

    void LoadColors (ColorScheme cs) {
        backgroundImage.color = cs.MatrixScreenBackground;
        multiplicationSignImage.color = cs.MatrixScreenMultiplicationSign;
        modelGroup.LoadColors(cs.MatrixScreenModelMatrixHeader, cs);
        camGroup.LoadColors(cs.MatrixScreenCameraMatrixHeader, cs);
        foreach(var b in borders){
            b.color = cs.MatrixScreenBorderColor;
        }
    }

    public void AddMatrix (UIMatrix callingMatrix) {
        if(callingMatrix.matrixGroup.TryGetIndexOf(callingMatrix, out var index)){
            callingMatrix.matrixGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.identityConfig, UIMatrix.Editability.FULL, index + 1);
            UpdateMatrixButtons();
        }else{
            Debug.LogError("wat");
        }
    }

    public void DeleteMatrix (UIMatrix matrixToDelete) {
        matrixToDelete.matrixGroup.DeleteMatrix(matrixToDelete);
        UpdateMatrixButtons();
    }

    public void MoveMatrixLeft (UIMatrix matrixToMove) {
        MoveMatrixWithIndexOffset(matrixToMove, -1);
    }

    public void MoveMatrixRight (UIMatrix matrixToMove) {
        MoveMatrixWithIndexOffset(matrixToMove, 1);
    }

    void MoveMatrixWithIndexOffset (UIMatrix matrixToMove, int indexOffset) {
        if(matrixToMove.matrixGroup.TryMoveMatrix(matrixToMove, indexOffset)){
            // all's cool my dude
        }else{
            if(indexOffset > 0 && matrixToMove.matrixGroup == modelGroup){
                modelGroup.ReleaseMatrix(matrixToMove);
                camGroup.InsertMatrix(matrixToMove, 0);
            }else if(indexOffset < 0 && matrixToMove.matrixGroup == camGroup){
                camGroup.ReleaseMatrix(matrixToMove);
                modelGroup.InsertMatrix(matrixToMove, modelGroup.matrixCount);
            }else{
                // nothing to be done. maybe disable the appropriate move button
            }
        }
        UpdateMatrixButtons();
    }

    void UpdateMatrixButtons () {
        for(int i=0; i<modelGroup.matrixCount; i++){
            var m = modelGroup[i];
            m.moveLeftBlocked = (i == 0);
            m.moveRightBlocked = (i == (modelGroup.matrixCount - 1)) && cantAddMoreMatrices && (modelGroup.matrixCount == 1);
            m.addButtonBlocked = cantAddMoreMatrices;
            m.deleteButtonBlocked = modelGroup.matrixCount == 1;    // TODO is this REALLY important?
        }
        for(int i=0; i<camGroup.matrixCount; i++){
            var m = camGroup[i];
            m.moveLeftBlocked = (i == 0) && cantAddMoreMatrices && (camGroup.matrixCount == 1);
            m.moveRightBlocked = (i == (camGroup.matrixCount - 1));
            m.addButtonBlocked = cantAddMoreMatrices;
            m.deleteButtonBlocked = camGroup.matrixCount == 1;      // TODO is this REALLY important?
        }
    }

    public Matrix4x4 GetWeightedModelMatrixForRendering () {
        return modelGroup.WeightedMatrixProduct.transpose;
    }

    public Matrix4x4 GetWeightedCameraMatrixForRendering () {
        return camGroup.WeightedMatrixProduct.transpose;
    }

    public Matrix4x4 GetUnweightedCameraMatrixForRendering () {
        return camGroup.UnweightedMatrixProduct.transpose;
    }

    public Mesh GetCurrentMesh () {
        return defaultMesh;     // TODO mesh selection
    }
	
}
