using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MatrixScreenUtils;

public class CustomCameraUIController : ClickDragScrollHandler {

    const string renderCamLabelText = "Render View";
    const string externalCamLabelText = "External View";
    const string renderCamLockedSuffix = "(Locked, use the matrices or deactivate free mode)";

    [Header("Prefabs")]
    [SerializeField] CustomGLCamera targetCamPrefab;

    [Header("References")]
    [SerializeField] CamControllerWindowOverlay windowOverlay;

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

    bool initialized;
    
    Vector3 pivotPoint; 
    PointerType currentPointerType;
    Vector3 lastMousePos;

    public bool CanCurrentlyControlCamera { 
        get {
            return windowOverlay.resetButtonEnabled;
        } set {
            windowOverlay.resetButtonEnabled = value;
            if(!value){
                currentPointerType = PointerType.None;
            }
        }
    }
    
    public CustomGLCamera targetCam { get; private set; }
    public CustomCameraUIController otherController { get; private set; }

    public bool IsExternalCamController => isExternalCamController;
    public CamControllerWindowOverlay overlay => windowOverlay;

    public void Initialize (MatrixScreen matrixScreen, CustomCameraUIController otherController) {
        if(initialized){
            Debug.LogError("Duplicate init call! Aborting...", this.gameObject);
            return;
        }
        this.otherController = otherController;
        targetCam = Instantiate(targetCamPrefab);
        targetCam.Initialize(
            isExternalCamera: isExternalCamController,
            otherCamera: (isExternalCamController ? otherController.targetCam : null),
            matrixScreen: matrixScreen, 
            inputFOV: camDefaultFOV,
            inputNearClip: camDefaultNearClip,
            inputFarClip: camDefaultFarClip,
            inputStartPos: camDefaultPosition
        );
        targetCam.SetupViewportRect(new Rect(camRectPos, camRectSize));
        targetCam.LoadColors(ColorScheme.current);
        windowOverlay.Initialize(this);
        initialized = true;
        LoadColors(ColorScheme.current);    
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
        windowOverlay.LoadColors(cs);
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
