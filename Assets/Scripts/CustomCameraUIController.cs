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
    [SerializeField] Vector3 camDefaultEuler;

    [Header("UI Generation")]
    [SerializeField] RectTransform uiParent;
    [SerializeField] TMP_FontAsset labelFont;
    [SerializeField] float labelFontSize;
    [SerializeField] float toggleSize;
    [SerializeField] float toggleOffset;
    [SerializeField] float toggleSeparatorOffset;

    bool initialized;
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

    public bool CanCurrentlyControlCamera { private get; set; }
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
            inputStartPos: camDefaultPosition,
            inputStartEuler: camDefaultEuler
        );
        targetCam.SetupViewportRect(new Rect(camRectPos, camRectSize));
        targetCam.LoadColors(ColorScheme.current);
        this.otherController = otherController;
        SetupLabel();
        SetupToggles();
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

        void SetupToggles () {
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

            void CreateSpecialToggle (Sprite icon, string toggleName, string hoverMessage, System.Action<bool> onStateChange, bool initialState) {
                // the toggle itself
                var newToggleRT = new GameObject(toggleName, typeof(RectTransform), typeof(Image), typeof(Toggle), typeof(UIHoverEventCaller)).GetComponent<RectTransform>();
                newToggleRT.SetParent(uiParent, false);
                newToggleRT.localScale = Vector3.one;
                newToggleRT.SetAnchor(1, 1);
                newToggleRT.SetPivot(1, 1);
                newToggleRT.sizeDelta = Vector2.one * toggleSize;
                newToggleRT.anchoredPosition = new Vector2(0, y);
                var newToggleBG = newToggleRT.gameObject.GetComponent<Image>();
                newToggleBG.sprite = UISprites.UICircle;
                newToggleBG.raycastTarget = true;
                var newToggle = newToggleRT.gameObject.GetComponent<Toggle>();
                newToggle.targetGraphic = newToggleBG;
                newToggle.isOn = initialState;
                var indexCopy = toggleIndex;
                newToggle.onValueChanged.AddListener((newVal) => {
                    SetToggleColors(indexCopy);
                    onStateChange?.Invoke(newVal);
                });
                onStateChange?.Invoke(initialState);
                var hover = newToggleRT.gameObject.GetComponent<UIHoverEventCaller>();
                hover.SetActions(
                    onHoverEnter: (ped) => {BottomLog.DisplayMessage(hoverMessage);},
                    onHoverExit: (ped) => {BottomLog.ClearDisplay();}
                );
                // the icon
                var newToggleIconRT = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                newToggleIconRT.SetParent(newToggleRT, false);
                newToggleIconRT.localScale = Vector3.one;
                newToggleIconRT.SetToFill();                
                var newToggleIcon = newToggleIconRT.gameObject.GetComponent<Image>();
                newToggleIcon.sprite = icon;
                newToggleIcon.raycastTarget = false;
                // setting all the things
                toggles.Add(newToggle);
                toggleBackgrounds.Add(newToggleBG);
                toggleIcons.Add(newToggleIcon);
                toggleIndex++;
                y -= (toggleSize + toggleOffset);
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
        targetCam.transform.RotateAround(pivotPoint, Vector3.up, mouseDelta.x * (inverted ? -1 : 1));
        targetCam.transform.RotateAround(pivotPoint, targetCam.transform.right, -mouseDelta.y);
    }

    void Move (Vector3 mouseDelta) {
        Vector3 moveDelta = moveSensitivity * GetPivotDistanceScale() * -1 * (targetCam.transform.right * mouseDelta.x + targetCam.transform.up * mouseDelta.y);
        pivotPoint += moveDelta;
        targetCam.transform.position += moveDelta;
    }

    void Zoom (float zoomAmount) {
        float currentDistToPivot = (targetCam.transform.position - pivotPoint).magnitude;
        float nearPlaneDist = targetCam.nearClipPlane;
        float tempDist = zoomAmount * scrollSensitivity * GetPivotDistanceScale();
        Vector3 moveDelta = targetCam.transform.forward * Mathf.Clamp(tempDist, Mathf.NegativeInfinity, currentDistToPivot - nearPlaneDist);
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
