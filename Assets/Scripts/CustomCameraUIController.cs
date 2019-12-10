using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CustomCameraUIController : ClickDragScrollHandler {

    const string renderCamLabelText = "Render View";
    const string externalCamLabelText = "External View";
    const string renderCamLockedSuffix = "(Locked, use the matrices or deactivate free mode)";

    [Header("Prefabs")]
    [SerializeField] CustomGLCamera targetCamPrefab;

    [Header("References")]
    [SerializeField] WindowDresser windowDresser;

    [Header("Settings")]
    [SerializeField] bool isExternalCamController;
    [SerializeField] float scrollSensitivity;
    [SerializeField] float smoothScrollSensitivity;
    [SerializeField] float orbitSensitivity;
    [SerializeField] float moveSensitivity;
    [SerializeField] bool inverted;

    [Header("Default Camera Settings")]
    [SerializeField] Vector2 camRectPos;
    [SerializeField] Vector2 camRectSize;
    [SerializeField] float camDefaultFOV;
    [SerializeField] float camDefaultNearClip;
    [SerializeField] float camDefaultFarClip;
    [SerializeField] Vector3 camDefaultPosition;

    [Header("UI Generation")]
    [SerializeField] RectTransform uiParent;

    bool initialized;
    bool m_canCurrentlyControlCamera;
    Color toggleIconActive;
    Color toggleIconInactive;
    Color toggleBackgroundActive;
    Color toggleBackgroundInactive;

    CustomGLCamera targetCam;
    CustomCameraUIController otherController;
    Vector3 pivotPoint; 
    PointerType currentPointerType;
    Vector3 lastMousePos;

    TextMeshProUGUI label;
    List<Toggle> toggles;
    List<Image> toggleBackgrounds;
    List<Image> toggleIcons;
    Button resetButton;
    Image resetButtonBackground;
    Image resetButtonIcon;
    Toggle wireToggle;

    public bool CanCurrentlyControlCamera { 
        get {
            return m_canCurrentlyControlCamera;       
        } set {
            m_canCurrentlyControlCamera = value;
            resetButton.interactable = value;
            LoadResetButtonColors(ColorScheme.current);
            if(!isExternalCamController){
                if(value){
                    label.text = renderCamLabelText;
                }else{
                    label.text = $"{renderCamLabelText} {renderCamLockedSuffix}";
                }
            }
        }
    }
    public bool IsExternalCamController => isExternalCamController;
    public CustomGLCamera Cam => targetCam;

    public void Initialize (MatrixScreen matrixScreen, CustomCameraUIController otherController) {
        if(initialized){
            Debug.LogError("Duplicate init call! Aborting...", this.gameObject);
            return;
        }
        targetCam = Instantiate(targetCamPrefab);
        targetCam.Initialize(
            isExternalCamera: isExternalCamController,
            otherCamera: (isExternalCamController ? otherController.Cam : null),
            matrixScreen: matrixScreen, 
            inputFOV: camDefaultFOV,
            inputNearClip: camDefaultNearClip,
            inputFarClip: camDefaultFarClip,
            inputStartPos: camDefaultPosition
        );
        targetCam.SetupViewportRect(new Rect(camRectPos, camRectSize));
        targetCam.LoadColors(ColorScheme.current);
        this.otherController = otherController;

        toggles = new List<Toggle>();
        toggleBackgrounds = new List<Image>();
        toggleIcons = new List<Image>();
        CreateRightSideToggles();
        CreateResetButtonAndLabel();

        initialized = true;
        LoadColors(ColorScheme.current);

        void CreateRightSideToggles () {
            windowDresser.Begin(uiParent, new Vector2(1, 1), new Vector2(0, -1), Vector2.zero);
            int toggleIndex = 0;
            wireToggle = CreateSpecialToggle(UISprites.MCamCtrlDrawWireframe, "Wireframe", "Toggles wireframe drawing", (b) => {
                targetCam.drawObjectAsWireFrame = b;
                otherController.WireToggled(b);
            }, false, offsetAfter: true);
            CreateSpecialToggle(UISprites.MCamCtrlDrawFloor, "Grid", "Toggles drawing the grid floor", (b) => {targetCam.drawGridFloor = b;}, !isExternalCamController);
            CreateSpecialToggle(UISprites.MCamCtrlDrawOrigin, "Origin", "Toggles drawing the origin", (b) => {targetCam.drawOrigin = b;}, isExternalCamController);
            CreateSpecialToggle(UISprites.MCamCtrlDrawSeeThrough, "XRay", "Toggles see-through drawing for all wireframe gizmos", (b) => {targetCam.drawSeeThrough = b;}, false, offsetAfter: isExternalCamController);
            if(targetCam.IsExternalCamera){     
                CreateSpecialToggle(UISprites.MCamCtrlDrawCamera, "Cam", "Toggles drawing the other camera", (b) => {targetCam.drawCamera = b;}, true);
                CreateSpecialToggle(UISprites.MCamCtrlDrawClipBox, "ClipBox", "Toggles drawing the clip space area", (b) => {targetCam.drawClipSpace = b;}, true);
                CreateSpecialToggle(UISprites.MCamCtrlShowCulling, "ShowClip", "Toggles culling visualization", (b) => {targetCam.showClipping = b;}, true);
            }
            windowDresser.End();

            Toggle CreateSpecialToggle (Sprite icon, string toggleName, string hoverMessage, System.Action<bool> onStateChange, bool initialState, bool offsetAfter = false) {
                // setting up position and looks
                var newToggleRT = windowDresser.CreateCircleWithIcon(icon, toggleName, hoverMessage, out var newToggleIcon, out var newToggleBackground, offsetAfter);
                // setting up the actual toggle
                newToggleRT.gameObject.AddComponent(typeof(Toggle));
                var newToggle = newToggleRT.GetComponent<Toggle>();
                newToggle.targetGraphic = newToggleBackground;
                newToggle.isOn = initialState;
                var indexCopy = toggleIndex;
                newToggle.onValueChanged.AddListener((newVal) => {
                    SetToggleColors(indexCopy);
                    onStateChange?.Invoke(newVal);
                });
                onStateChange?.Invoke(initialState);
                // saving to the lists, updating index
                toggles.Add(newToggle);
                toggleBackgrounds.Add(newToggleBackground);
                toggleIcons.Add(newToggleIcon);
                toggleIndex++;
                // output
                return newToggle;
            }
        }

        void CreateResetButtonAndLabel () {
            windowDresser.Begin(uiParent, new Vector2(0, 1), new Vector2(1, 0), Vector2.zero);
            // the reset button
            var resetRT = windowDresser.CreateCircleWithIcon(UISprites.UIReset, "Reset", "Resets the view", out resetButtonIcon, out resetButtonBackground);
            resetRT.gameObject.AddComponent<Button>();
            resetButton = resetRT.GetComponent<Button>();
            resetButton.targetGraphic = resetButtonBackground;
            resetButton.onClick.AddListener(() => {ResetCamera();});
            // the label
            label = windowDresser.CreateLabel();
            label.text = isExternalCamController ? externalCamLabelText : renderCamLabelText;
            windowDresser.End();
        }
    }

    void WireToggled (bool newVal) {
        if(wireToggle == null || wireToggle.isOn == newVal){
            return;
        }
        wireToggle.isOn = newVal;
    }

    void SetToggleColors (int toggleIndex) {
        if(toggles[toggleIndex].isOn){
            toggleIcons[toggleIndex].color = toggleIconActive;
            toggleBackgrounds[toggleIndex].color = toggleBackgroundActive;
        }else{
            toggleIcons[toggleIndex].color = toggleIconInactive;
            toggleBackgrounds[toggleIndex].color = toggleBackgroundInactive;
        }
    }

    public void ResetCamera () {
        pivotPoint = Vector3.zero;
        targetCam.ResetToDefault();
    }

    void OnEnable () {
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColors;
        targetCam.drawPivot = false;
        currentPointerType = PointerType.None;
    }

    void LoadColors (ColorScheme cs) {
        if(!initialized){
            return;
        }
        label.color = cs.MatrixWindowLabel;
        toggleBackgroundActive = cs.MatrixWindowButtonBackgroundActive;
        toggleBackgroundInactive = cs.MatrixWindowButtonBackgroundInactive;
        toggleIconActive = cs.MatrixWindowButtonIconActive;
        toggleIconInactive = cs.MatrixWindowButtonIconInactive;
        for(int i=0; i<toggles.Count; i++){
            toggles[i].SetFadeTransition(0f, Color.white, cs.MatrixWindowButtonHover, cs.MatrixWindowButtonClick, Color.magenta);
            SetToggleColors(i);
        }
        LoadResetButtonColors(cs);
    }

    void LoadResetButtonColors (ColorScheme cs) {
        if(resetButton.interactable){
            resetButtonBackground.color = cs.MatrixWindowButtonBackgroundActive;
            resetButtonIcon.color = cs.MatrixWindowButtonIconActive;
        }else{
            resetButtonBackground.color = cs.MatrixWindowButtonBackgroundInactive;
            resetButtonIcon.color = cs.MatrixWindowButtonIconInactive;
        }
        resetButton.SetFadeTransition(0f, Color.white, cs.MatrixWindowButtonHover, cs.MatrixWindowButtonClick, Color.white);
    }

    void Update () {
        if(!CanCurrentlyControlCamera){
            return;
        }
        if(currentPointerType != PointerType.None){
            var currentMousePos = Input.mousePosition;
            var mouseDelta = currentMousePos - lastMousePos;
            switch(currentPointerType){
                case PointerType.Left:
                    Orbit(mouseDelta);
                    break;
                case PointerType.Right:
                    Move(mouseDelta);
                    break;
                case PointerType.Middle:
                    Zoom(mouseDelta.y * smoothScrollSensitivity);
                    break;
                default:
                    break;
            }
            lastMousePos = currentMousePos;
        }
        EnsureCameraLookingAtPivot();

        targetCam.drawPivot = currentPointerType != PointerType.None;
        targetCam.pivotPointToDraw = pivotPoint;

        void EnsureCameraLookingAtPivot () {
            Vector3 origUp = targetCam.transform.up;
            Vector3 customUp;
            if(Mathf.Abs(origUp.y) > 0.1f){
                customUp = Vector3.up * Mathf.Sign(origUp.y);
            }else{
                customUp = Vector3.ProjectOnPlane(origUp, Vector3.up);
            }
            targetCam.transform.LookAt(pivotPoint, customUp);
        }
    }

    void Orbit (Vector3 mouseDelta) {
        mouseDelta *= InputSystem.shiftCtrlMultiplier;
        targetCam.transform.RotateAround(pivotPoint, Vector3.up, mouseDelta.x * (inverted ? -1 : 1));
        targetCam.transform.RotateAround(pivotPoint, targetCam.transform.right, -mouseDelta.y);
    }

    void Move (Vector3 mouseDelta) {
        mouseDelta *= InputSystem.shiftCtrlMultiplier;
        Vector3 moveDelta = moveSensitivity * GetPivotDistanceScale() * -1 * (targetCam.transform.right * mouseDelta.x + targetCam.transform.up * mouseDelta.y);
        pivotPoint += moveDelta;
        targetCam.transform.position += moveDelta;
    }

    void Zoom (float zoomAmount) {
        zoomAmount *= InputSystem.shiftCtrlMultiplier;
        float currentDistToPivot = (targetCam.transform.position - pivotPoint).magnitude;
        float nearPlaneDist = targetCam.nearClipPlane;
        float farPlaneDist = targetCam.farClipPlane;
        float tempDist = zoomAmount * scrollSensitivity * GetPivotDistanceScale();
        float moveDist;
        if(tempDist > 0f){
            moveDist = Mathf.Clamp(tempDist, Mathf.NegativeInfinity, currentDistToPivot - nearPlaneDist);
        }else if(tempDist < 0f){
            moveDist = Mathf.Clamp(tempDist, currentDistToPivot - (2f * farPlaneDist), Mathf.Infinity);
        }else{
            moveDist = 0f;
        }
        Vector3 moveDelta = targetCam.transform.forward * moveDist;
        targetCam.transform.position += moveDelta;
    }

    float GetPivotDistanceScale () {
        float currentDistToPivot = (targetCam.transform.position - pivotPoint).magnitude;
        float nearPlaneDist = targetCam.nearClipPlane;
        return Mathf.Max(Mathf.Abs(currentDistToPivot - nearPlaneDist), 0.01f);
    }

    protected override void PointerDown (PointerEventData eventData) {
        if(currentPointerType == PointerType.None){
            currentPointerType = PointerIDToType(eventData.pointerId);
            lastMousePos = Input.mousePosition;
        }
    }

    protected override void PointerUp (PointerEventData eventData) {
        if(PointerIDToType(eventData.pointerId) == currentPointerType){
            currentPointerType = PointerType.None;
        }
    }

    protected override void Scroll (PointerEventData eventData) {
        Zoom(eventData.scrollDelta.y);
    }
	
}
