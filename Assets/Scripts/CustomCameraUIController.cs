using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] TMP_FontAsset labelFont;
    [SerializeField] float labelFontSize;

    CustomGLCamera targetCam;
    Vector3 pivotPoint; 
    PointerType currentPointerType;
    Vector3 lastMousePos;

    public bool IsExternalCamController => isExternalCamController;
    public CustomGLCamera Cam => targetCam;

    public void Initialize (MatrixScreen matrixScreen, CustomCameraUIController otherController) {
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
        InitializeControls();

        void InitializeControls () {


            if(targetCam.IsExternalCamera){
                // more buttons
            }
        }
    }

    public void ResetCamera () {
        targetCam.ResetToDefault();
    }

    void OnDisable () {
        targetCam.drawPivot = false;
        currentPointerType = PointerType.None;
    }

    void Update () {
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
