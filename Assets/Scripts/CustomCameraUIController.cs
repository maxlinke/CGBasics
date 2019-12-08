using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CustomCameraUIController : ClickDragScrollHandler {

    [Header("Prefabs")]
    [SerializeField] CustomGLCamera targetCamPrefab;

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
    [SerializeField] TMP_FontAsset labelFont;
    [SerializeField] float labelFontSize;
    [SerializeField] float toggleSize;
    [SerializeField] float toggleOffset;
    [SerializeField] float toggleSeparatorOffset;

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

    public bool CanCurrentlyControlCamera { 
        get {
            return m_canCurrentlyControlCamera;       
        } set {
            m_canCurrentlyControlCamera = value;
            resetButton.interactable = value;
            LoadResetButtonColors(ColorScheme.current);
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
        SetupLabel();
        SetupTogglesAndResetButton();
        initialized = true;
        LoadColors(ColorScheme.current);

        void SetupLabel () {
            label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.rectTransform.SetParent(uiParent, false);
            label.rectTransform.SetAnchor(0, 1);
            label.rectTransform.SetPivot(0, 1);
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
            label.font = labelFont;
            label.fontSize = labelFontSize;
            label.raycastTarget = false;
            if(isExternalCamController){
                label.text = "External View";
            }else{
                label.text = "Render View";
            }
        }

        void SetupTogglesAndResetButton () {
            toggles = new List<Toggle>();
            toggleBackgrounds = new List<Image>();
            toggleIcons = new List<Image>();
            int toggleIndex = 0;
            float y = 0;
            CreateSpecialToggle(UISprites.MCamCtrlDrawWireframe, "Wireframe", "Toggles wireframe drawing", (b) => {targetCam.drawObjectAsWireFrame = b;}, false);
            y -= toggleSeparatorOffset;
            CreateSpecialToggle(UISprites.MCamCtrlDrawFloor, "Grid", "Toggles drawing the grid floor", (b) => {targetCam.drawGridFloor = b;}, true);
            CreateSpecialToggle(UISprites.MCamCtrlDrawOrigin, "Origin", "Toggles drawing the origin", (b) => {targetCam.drawOrigin = b;}, true);
            CreateSpecialToggle(UISprites.MCamCtrlDrawSeeThrough, "XRay", "Toggles see-through drawing for all wireframe gizmos", (b) => {targetCam.drawSeeThrough = b;}, false);
            if(targetCam.IsExternalCamera){
                y -= toggleSeparatorOffset;
                CreateSpecialToggle(UISprites.MCamCtrlDrawCamera, "Cam", "Toggles drawing the other camera", (b) => {targetCam.drawCamera = b;}, true);
                CreateSpecialToggle(UISprites.MCamCtrlDrawClipBox, "ClipBox", "Toggles drawing the clip space area", (b) => {targetCam.drawClipSpace = b;}, true);
                CreateSpecialToggle(UISprites.MCamCtrlShowCulling, "ShowClip", "Toggles culling visualization", (b) => {targetCam.showClipping = b;}, true);
            }
            CreateResetButton();

            void CreateSpecialToggle (Sprite icon, string toggleName, string hoverMessage, System.Action<bool> onStateChange, bool initialState) {
                // setting up position and looks
                var newToggleRT = CreateThingWithIcon(icon, toggleName, hoverMessage, out var newToggleIcon, out var newToggleBackground);
                newToggleRT.localScale = Vector3.one;
                newToggleRT.SetAnchor(1, 1);
                newToggleRT.SetPivot(1, 1);
                newToggleRT.sizeDelta = Vector2.one * toggleSize;
                newToggleRT.anchoredPosition = new Vector2(0, y);
                // setting up the toggle itself
                newToggleRT.gameObject.AddComponent(typeof(Toggle));
                var newToggle = newToggleRT.gameObject.GetComponent<Toggle>();
                newToggle.targetGraphic = newToggleBackground;
                newToggle.isOn = initialState;
                var indexCopy = toggleIndex;
                newToggle.onValueChanged.AddListener((newVal) => {
                    SetToggleColors(indexCopy);
                    onStateChange?.Invoke(newVal);
                });
                onStateChange?.Invoke(initialState);
                // saving to the lists, updating index and y
                toggles.Add(newToggle);
                toggleBackgrounds.Add(newToggleBackground);
                toggleIcons.Add(newToggleIcon);
                toggleIndex++;
                y -= (toggleSize + toggleOffset);
            }

            void CreateResetButton () {
                var newBtnRT = CreateThingWithIcon(UISprites.UIReset, "Reset", "Resets the view", out resetButtonIcon, out resetButtonBackground);
                newBtnRT.localScale = Vector3.one;
                newBtnRT.SetAnchor(0, 1);
                newBtnRT.SetPivot(0, 1);
                newBtnRT.sizeDelta = Vector2.one * toggleSize;
                newBtnRT.anchoredPosition = Vector2.zero;
                label.rectTransform.anchoredPosition += new Vector2(newBtnRT.rect.width + toggleOffset, 0f);    // a bit dirty but who gives a damn
                // setting up the button
                newBtnRT.gameObject.AddComponent<Button>();
                resetButton = newBtnRT.gameObject.GetComponent<Button>();
                resetButton.targetGraphic = resetButtonBackground;
                resetButton.onClick.AddListener(() => {ResetCamera();});
            }

            RectTransform CreateThingWithIcon (Sprite icon, string thingName, string hoverMessage, out Image iconImage, out Image backgroundImage) {
                // main creation
                var newThingRT = new GameObject(thingName, typeof(RectTransform), typeof(Image), typeof(UIHoverEventCaller)).GetComponent<RectTransform>();
                newThingRT.SetParent(uiParent, false);
                // hover init
                var newThingHover = newThingRT.gameObject.GetComponent<UIHoverEventCaller>();
                newThingHover.SetActions(
                    onHoverEnter: (ped) => {BottomLog.DisplayMessage(hoverMessage);},
                    onHoverExit: (ped) => {BottomLog.ClearDisplay();}
                );
                // initializing and assigning the background image
                backgroundImage = newThingRT.gameObject.GetComponent<Image>();
                backgroundImage.sprite = UISprites.UICircle;
                backgroundImage.raycastTarget = true;
                // the icon
                var newThingIconRT = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                newThingIconRT.SetParent(newThingRT, false);
                newThingIconRT.localScale = Vector3.one;
                newThingIconRT.SetToFill();
                // initializing and assigning the icon image
                iconImage = newThingIconRT.gameObject.GetComponent<Image>();
                iconImage.sprite = icon;
                iconImage.raycastTarget = false;
                // output
                return newThingRT;
            }
        }
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
        label.color = cs.MatrixCamControllerLabel;
        toggleBackgroundActive = cs.MatrixCamControllerToggleBackgroundActive;
        toggleBackgroundInactive = cs.MatrixCamControllerToggleBackgroundInactive;
        toggleIconActive = cs.MatrixCamControllerToggleIconActive;
        toggleIconInactive = cs.MatrixCamControllerToggleIconInactive;
        for(int i=0; i<toggles.Count; i++){
            toggles[i].SetFadeTransition(0f, Color.white, cs.MatrixCamControllerToggleHover, cs.MatrixCamControllerToggleClick, Color.magenta);
            SetToggleColors(i);
        }
        LoadResetButtonColors(cs);
    }

    void LoadResetButtonColors (ColorScheme cs) {
        if(resetButton.interactable){
            resetButtonBackground.color = cs.MatrixCamControllerToggleBackgroundActive;
            resetButtonIcon.color = cs.MatrixCamControllerToggleIconActive;
        }else{
            resetButtonBackground.color = cs.MatrixCamControllerToggleBackgroundInactive;
            resetButtonIcon.color = cs.MatrixCamControllerToggleIconInactive;
        }
        resetButton.SetFadeTransition(0f, Color.white, cs.MatrixCamControllerToggleHover, cs.MatrixCamControllerToggleClick, Color.white);
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
