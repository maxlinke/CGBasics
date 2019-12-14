﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MatrixScreenUtils;

public class MatrixScreen : MonoBehaviour {

    private const int MAX_MATRIX_COUNT = 16;

    [Header("Prefabs")]
    [SerializeField] UIMatrix uiMatrixPrefab;
    [SerializeField] UIMatrixGroup matrixGroupPrefab;
    [SerializeField] UIMatrixInputModel modelPreviewPrefab;

    [Header("Components")]
    [SerializeField] MatrixWindowOverlay windowOverlay;
    [SerializeField] CustomCameraUIController matrixCamController;
    [SerializeField] CustomCameraUIController externalCamController;
    [SerializeField] Image backgroundImage;
    [SerializeField] DefaultMesh defaultMesh;
    [SerializeField] RectTransform uiMatrixParent;
    [SerializeField] PanAndZoom panAndZoomController;
    [SerializeField] Image[] borders;
    [SerializeField] CenterBottomPopup centerBottomPopup;

    [Header("Settings")]
    [SerializeField] float weightLerpDeltaPerSecond;
    [SerializeField] float multiplicationSignSize;
    [SerializeField] float matrixGroupMargin;

    public float zoomLevel => panAndZoomController.zoomLevel;
    public UIMatrix ViewPosMatrix => viewPosMatrix;
    public UIMatrix ViewRotMatrix => viewRotMatrix;
    public UIMatrix ProjMatrix => projMatrix;
    public PanAndZoom PanAndZoomController => panAndZoomController;
    public Vector4 VectorModeVector => new Vector4(1, 1, 1, 1);         // TODO this and all that comes with it

    private bool cantAddMoreMatrices => modelGroup.matrixCount + camGroup.matrixCount >= MAX_MATRIX_COUNT;

    bool initialized;
    UIMatrixGroup modelGroup;
    UIMatrixGroup camGroup;
    UIMatrix viewPosMatrix;
    UIMatrix viewRotMatrix;
    UIMatrix projMatrix;
    UIMatrixInputModel modelPreview;
    List<Image> multiplicationSignImages;
    RectTransform previewMultiplicationSignRT;
    Mesh currentMesh;

    float currentLinearWeight;
    float currentWeightTarget;

    public bool FreeMode => centerBottomPopup.FreeModeToggle.isOn;
    public bool OpenGLMode => windowOverlay.glToggle.isOn;
    public bool OrthoMode => windowOverlay.orthoToggle.isOn;
    public bool VectorMode => windowOverlay.vectorToggle.isOn;

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
        if(!initialized){
            return;
        }
        UpdateLinearWeight();
        ApplyIndividualWeights();

        void UpdateLinearWeight () {
            float delta = currentWeightTarget - currentLinearWeight;
            currentLinearWeight += Mathf.Sign(delta) * Mathf.Min(Mathf.Abs(delta), weightLerpDeltaPerSecond * Time.deltaTime);
        }

        void ApplyIndividualWeights () {
            int totalMatrixCount = modelGroup.matrixCount + camGroup.matrixCount;
            for(int i=0; i<totalMatrixCount; i++){
                float matrixWeight = Mathf.Clamp01(currentLinearWeight - i);
                UIMatrix matrix;
                if(i<modelGroup.matrixCount){
                    matrix = modelGroup[i];
                }else{
                    matrix = camGroup[i-modelGroup.matrixCount];
                }
                matrix.CurrentWeight = matrixWeight;
            }
            modelGroup.displayWeight = Mathf.Clamp01(currentLinearWeight / modelGroup.matrixCount);
            camGroup.displayWeight = Mathf.Clamp01((currentLinearWeight - modelGroup.matrixCount) / camGroup.matrixCount);
        }  
    }

    void Initialize () {
        if(initialized){
            Debug.LogWarning($"Duplicate init call for {nameof(MatrixScreen)}, aborting!", this.gameObject);
            return;
        }
        windowOverlay.Initialize(
            matrixScreen: this, 
            glInit: false, 
            onGLToggled: GLModeUpdated, 
            orthoInit: false, 
            onOrthoToggled: OrthoModeUpdated,
            vectorInit: false,
            onVectorToggled: VectorModeUpdated
        );
        matrixCamController.Initialize(this, externalCamController);
        externalCamController.Initialize(this, matrixCamController);
        centerBottomPopup.Initialize(
            matrixScreen: this, 
            freeModeInit: false,
            onFreeModeToggled: FreeModeUpdated,
            onSliderValueChanged: (newVal) => {
                currentWeightTarget = newVal;
            }
        );
        multiplicationSignImages = new List<Image>();
        centerBottomPopup.LoadColors(ColorScheme.current);
        CreateMultiplicationSign(out _);
        modelGroup = CreateMatrixGroup(leftSide: true);
        modelGroup.SetName("Model Matrix");
        camGroup = CreateMatrixGroup(leftSide: false);
        camGroup.SetName("Camera Matrix");
        CreateMultiplicationSign(out previewMultiplicationSignRT);
        modelPreview = Instantiate(modelPreviewPrefab);
        modelPreview.Initialize(this, defaultMesh.mesh, defaultMesh.name, (m) => {currentMesh = m;});
        modelPreview.rectTransform.SetParent(uiMatrixParent, false);
        modelPreview.rectTransform.ResetLocalScale();
        currentMesh = defaultMesh.mesh;
        AlignMatrixGroups();
        modelGroup.onContentRebuilt += UpdatePreviewPosition;
        camGroup.onContentRebuilt += UpdatePreviewPosition;
        ActivateNonFreeMode();
        initialized = true;

        UIMatrixGroup CreateMatrixGroup (bool leftSide) {
            var newGroup = Instantiate(matrixGroupPrefab);
            newGroup.rectTransform.SetParent(uiMatrixParent, false);
            newGroup.rectTransform.localScale = Vector3.one;
            newGroup.Initialize(this);
            return newGroup;
        }
        
    }

    void CreateMultiplicationSign (out RectTransform mulSignRT) {
        mulSignRT = new GameObject("Multiplication Sign", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        mulSignRT.SetParent(uiMatrixParent, false);
        mulSignRT.localScale = Vector3.one;
        mulSignRT.SetAnchor(0.5f * Vector2.one);
        mulSignRT.pivot = 0.5f * Vector2.one;
        mulSignRT.anchoredPosition = Vector2.zero;
        mulSignRT.sizeDelta = Vector2.one * multiplicationSignSize;
        var mulSignImg = mulSignRT.gameObject.GetComponent<Image>();
        mulSignImg.sprite = UISprites.MatrixMultiply;
        multiplicationSignImages.Add(mulSignImg);
    }

    void FreeModeUpdated (bool value) {
        if(value){
            ActivateFreeMode();
        }else{
            ActivateNonFreeMode();
        }
    }

    void GLModeUpdated (bool value) {
        AlignMatrixGroups();
        if(FreeMode){
            centerBottomPopup.FreeModeToggle.isOn = false;
        }else{
            ActivateNonFreeMode();
        }
    }

    void OrthoModeUpdated (bool value) {
        if(FreeMode){
            centerBottomPopup.FreeModeToggle.isOn = false;
        }else{
            ActivateNonFreeMode();
        }
    }

    void VectorModeUpdated (bool value) {

    }

    void AlignMatrixGroups () {
        Vector2 anchor = new Vector2(0.5f, 0.5f);
        Vector2 leftPivot = new Vector2(1f, 0.5f);
        Vector2 rightPivot = new Vector2(0f, 0.5f);
        Vector2 leftPos = new Vector2(-1f * (matrixGroupMargin + multiplicationSignSize / 2f), 0f);
        Vector2 rightPos = new Vector2(1f * (matrixGroupMargin + multiplicationSignSize / 2f), 0f);
        modelGroup.rectTransform.SetAnchor(anchor);
        modelGroup.rectTransform.pivot = (OpenGLMode ? rightPivot : leftPivot);
        modelGroup.rectTransform.anchoredPosition = (OpenGLMode ? rightPos : leftPos);
        camGroup.rectTransform.SetAnchor(anchor);
        camGroup.rectTransform.pivot = (OpenGLMode ? leftPivot : rightPivot);
        camGroup.rectTransform.anchoredPosition= (OpenGLMode ? leftPos : rightPos);
    }

    void UpdatePreviewPosition () {
        Vector2 anchor = new Vector2(0.5f, 0.5f);
        Vector2 leftPivot = new Vector2(1f, 0.5f);
        Vector2 rightPivot = new Vector2(0f, 0.5f);
        float absX = Mathf.Abs(modelGroup.rectTransform.anchoredPosition.x) + modelGroup.rectTransform.rect.width + matrixGroupMargin + (multiplicationSignSize / 2);
        Vector2 pos = (OpenGLMode ? new Vector2(absX, 0) : new Vector2(-absX, 0));
        previewMultiplicationSignRT.SetAnchor(anchor);
        previewMultiplicationSignRT.pivot = new Vector2(0.5f, 0.5f);
        previewMultiplicationSignRT.anchoredPosition = pos;
        absX += matrixGroupMargin + (multiplicationSignSize / 2);
        pos = (OpenGLMode ? new Vector2(absX, 0) : new Vector2(-absX, 0));
        modelPreview.rectTransform.SetAnchor(anchor);
        modelPreview.rectTransform.pivot = (OpenGLMode ? rightPivot : leftPivot);
        modelPreview.rectTransform.anchoredPosition = pos;
    }

    public void ActivateNonFreeMode () {
        modelGroup.ResetToOnlyOneMatrix(false);
        modelGroup[0].LoadConfig(UIMatrices.MatrixConfig.scaleConfig);
        modelGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.fullRotationConfig, UIMatrix.Editability.FULL, 1, false);
        modelGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.translationConfig, UIMatrix.Editability.FULL, 2, true);

        camGroup.ResetToOnlyOneMatrix(false);
        camGroup[0].LoadConfig(UIMatrices.MatrixConfig.inverseTranslationConfig);
        camGroup[0].SetName("Inv. Camera Position");
        camGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.rebaseConfig, UIMatrix.Editability.FULL, 1, false);
        camGroup[1].SetName("Inv. Camera Rotation");
        camGroup[1].Transpose(false);
        if(OrthoMode){
            camGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.orthoProjConfig, UIMatrix.Editability.FULL, 2, true);
        }else{
            camGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.perspProjConfig, UIMatrix.Editability.FULL, 2, true);
        }

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
        matrixCamController.CanCurrentlyControlCamera = true;
        matrixCamController.ResetCamera();
        externalCamController.CanCurrentlyControlCamera = true;
        // externalCamController.ResetCamera();

        UpdateMatrixButtonsAndSlider(modelGroup.matrixCount + camGroup.matrixCount);
    }

    public void ActivateFreeMode () {
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
        matrixCamController.CanCurrentlyControlCamera = false;
        externalCamController.CanCurrentlyControlCamera = true;
    }

    void LoadColors (ColorScheme cs) {
        backgroundImage.color = cs.MatrixScreenBackground;
        foreach(var mulImg in multiplicationSignImages){
            mulImg.color = cs.MatrixScreenMultiplicationSign;
        }
        modelGroup.LoadColors(cs.MatrixScreenModelMatrixHeader, cs);
        camGroup.LoadColors(cs.MatrixScreenCameraMatrixHeader, cs);
        foreach(var b in borders){
            b.color = cs.MatrixScreenBorderColor;
        }
        centerBottomPopup.LoadColors(cs);
        windowOverlay.LoadColors(cs);
    }

    public void AddMatrix (UIMatrix callingMatrix) {
        if(callingMatrix.matrixGroup.TryGetIndexOf(callingMatrix, out var index)){
            // callingMatrix.matrixGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.identityConfig, UIMatrix.Editability.FULL, index + 1);
            callingMatrix.matrixGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.identityConfig, UIMatrix.Editability.FULL, index + (OpenGLMode ? 0 : 1));
            int realIndex = (callingMatrix.matrixGroup == modelGroup ? index : modelGroup.matrixCount + index);
            if(realIndex < currentLinearWeight){
                currentLinearWeight += 1;
            }
            UpdateMatrixButtonsAndSlider(Mathf.RoundToInt(currentLinearWeight));
        }else{
            Debug.LogError("wat");
        }
    }

    public void DeleteMatrix (UIMatrix matrixToDelete) {
        if(matrixToDelete.matrixGroup.TryGetIndexOf(matrixToDelete, out var index)){
            int realIndex = (matrixToDelete.matrixGroup == modelGroup ? index : modelGroup.matrixCount + index);
            if(realIndex < currentLinearWeight){
                currentLinearWeight -= 1;
            }
        }
        matrixToDelete.matrixGroup.DeleteMatrix(matrixToDelete);
        UpdateMatrixButtonsAndSlider(Mathf.RoundToInt(currentLinearWeight));
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
        UpdateMatrixButtonsAndSlider();
    }

    void UpdateMatrixButtonsAndSlider (int newSliderValue = -1) {
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
        var totalMatrixCount = modelGroup.matrixCount + camGroup.matrixCount;
        if(newSliderValue != -1){
            centerBottomPopup.UpdateSlider(totalMatrixCount, true, newSliderValue);
        }else{
            centerBottomPopup.UpdateSlider(totalMatrixCount);
        }
    }

    public Matrix4x4 GetWeightedModelMatrixForRendering () {
        if(OpenGLMode){
            return modelGroup.WeightedMatrixProduct;
        }else{
            return modelGroup.WeightedMatrixProduct.transpose;
        }
    }

    public Matrix4x4 GetWeightedCameraMatrixForRendering () {
        if(OpenGLMode){
            return camGroup.WeightedMatrixProduct;
        }else{
            return camGroup.WeightedMatrixProduct.transpose;
        }
    }

    public Matrix4x4 GetUnweightedCameraMatrixForRendering () {
        if(OpenGLMode){
            return camGroup.UnweightedMatrixProduct;
        }else{
            return camGroup.UnweightedMatrixProduct.transpose;
        }
    }

    public Mesh GetCurrentMesh () {
        return currentMesh;
    }

    public bool CameraMatrixNotUnweighted () {
        return (currentLinearWeight > modelGroup.matrixCount);
    }

    public bool CameraMatrixFullyWeighted () {
        return (currentLinearWeight >= (modelGroup.matrixCount + camGroup.matrixCount));
    }

}
