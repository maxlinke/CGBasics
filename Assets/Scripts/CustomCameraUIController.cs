using UnityEngine;
using UnityEngine.EventSystems;
using MatrixScreenUtils;

public class CustomCameraUIController : ClickDragScrollHandler {

    const string renderCamLabelText = "Render View";
    const string externalCamLabelText = "External View";
    const string renderCamLockedSuffix = "(Locked, use the matrices or deactivate free mode)";

    const float scrollSensitivity = 0.2f;
    const float smoothScrollSensitivity = 0.2f;
    const float orbitSensitivity = 0.8f;
    const float orthoMoveSensitivity = 0.01f;
    const float perspMoveSensitivity = 0.0025f;

    [Header("Prefabs")]
    [SerializeField] CustomGLCamera targetCamPrefab;

    [Header("References")]
    [SerializeField] CamControllerWindowOverlay windowOverlay;

    [Header("Settings")]
    [SerializeField] bool isExternalCamController;

    [Header("Default Camera Settings")]
    [SerializeField] Vector2 camRectPos;
    [SerializeField] Vector2 camRectSize;
    [SerializeField] float camDefaultFOV;
    [SerializeField] float camDefaultOrthoSize;
    [SerializeField] float camDefaultNearClip;
    [SerializeField] float camDefaultFarClip;
    [SerializeField] float farClipMaximum;
    [SerializeField] Vector3 camDefaultPosition;

    bool initialized;
    
    Vector3 pivotPoint; 
    PointerType currentPointerType;
    Vector3 lastMousePos;
    MatrixScreen matrixScreen;

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
        this.matrixScreen = matrixScreen;
        this.otherController = otherController;
        targetCam = Instantiate(targetCamPrefab);
        targetCam.Initialize(
            isExternalCamera: isExternalCamController,
            otherCamera: (isExternalCamController ? otherController.targetCam : null),
            matrixScreen: matrixScreen, 
            inputFOV: camDefaultFOV,
            inputNearClip: camDefaultNearClip,
            inputFarClip: camDefaultFarClip,
            inputStartPos: camDefaultPosition,
            inputOrthoSize: camDefaultOrthoSize
        );
        targetCam.SetupViewportRect(new Rect(camRectPos, camRectSize));
        targetCam.LoadColors(ColorScheme.current);
        string nameSuffix = isExternalCamController ? "(ext)" : "(int)";
        targetCam.name += $" {nameSuffix}";
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
        EnsurePivotIsCenteredInClipSpace();

        targetCam.drawPivot = currentPointerType != PointerType.None;
        targetCam.pivotPointToDraw = pivotPoint;

        void EnsurePivotIsCenteredInClipSpace () {
            if(!isExternalCamController && matrixScreen.OrthoMode){
                var offsetDir = (pivotPoint - targetCam.transform.position).normalized;
                var near = targetCam.transform.position + (targetCam.nearClipPlane * offsetDir);
                var far = targetCam.transform.position + (targetCam.farClipPlane * offsetDir);
                var avg = (near + far) / 2f;
                var delta = pivotPoint - avg;
                targetCam.transform.position += delta;
            }
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
        mouseDelta *= InputSystem.shiftCtrlMultiplier * orbitSensitivity;
        targetCam.transform.RotateAround(pivotPoint, Vector3.up, mouseDelta.x);
        targetCam.transform.RotateAround(pivotPoint, targetCam.transform.right, -mouseDelta.y);
    }

    void Move (Vector3 mouseDelta) {
        mouseDelta *= InputSystem.shiftCtrlMultiplier;
        Vector3 moveDelta = -1 * (targetCam.transform.right * mouseDelta.x + targetCam.transform.up * mouseDelta.y);
        if(matrixScreen.OrthoMode && !isExternalCamController){
            moveDelta *= orthoMoveSensitivity * (targetCam.orthoSize / camDefaultOrthoSize);
        }else{
            moveDelta *= perspMoveSensitivity * GetPivotDistanceScale();
        }
        pivotPoint += moveDelta;
        targetCam.transform.position += moveDelta;
    }

    void Zoom (float zoomAmount) {
        zoomAmount *= InputSystem.shiftCtrlMultiplier;
        if(matrixScreen.OrthoMode && !isExternalCamController){
            float currentOrthoSize = targetCam.orthoSize;
            float tempDelta = -zoomAmount * currentOrthoSize * 0.1f;
            targetCam.orthoSize = Mathf.Clamp(currentOrthoSize + tempDelta, 0.5f, 20f);
            float orthoFact = targetCam.orthoSize / camDefaultOrthoSize;
            targetCam.farClipPlane = orthoFact * camDefaultFarClip;
        }else{
            float currentDistToPivot = (targetCam.transform.position - pivotPoint).magnitude;
            float nearPlaneDist = targetCam.nearClipPlane;
            float tempDist = zoomAmount * scrollSensitivity * GetPivotDistanceScale();
            float moveDist;
            float maxNearDist = camDefaultNearClip;
            float maxFarDist = 2f * camDefaultFarClip;
            if(tempDist > 0f){
                moveDist = Mathf.Clamp(tempDist, Mathf.NegativeInfinity, currentDistToPivot - nearPlaneDist);
            }else if(tempDist < 0f){
                moveDist = Mathf.Clamp(tempDist, currentDistToPivot - maxFarDist, Mathf.Infinity);
            }else{
                moveDist = 0f;
            }
            Vector3 moveDelta = targetCam.transform.forward * moveDist;
            targetCam.transform.position += moveDelta;
            float startDist = camDefaultPosition.magnitude;
            float m = (farClipMaximum - camDefaultFarClip) / (maxFarDist - startDist);
            float n = camDefaultFarClip - (startDist * m);
            float farClipMinimum = m * maxNearDist  + n;
            farClipMinimum = Mathf.Clamp(farClipMinimum, nearPlaneDist + 0.1f, Mathf.Infinity);     // just to make sure i don't input bogus values
            targetCam.farClipPlane = Mathf.Lerp(farClipMinimum, farClipMaximum, ((targetCam.transform.position - pivotPoint).magnitude - maxNearDist) / (maxFarDist - maxNearDist));
        }
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
