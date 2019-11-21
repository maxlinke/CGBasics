using UnityEngine;
using UnityEngine.EventSystems;

public class CustomCameraUIController : MonoBehaviour, IScrollHandler, IPointerDownHandler, IPointerUpHandler {

    [Header("References")]
    [SerializeField] CustomGLCamera targetCam;

    [Header("Settings")]
    [SerializeField] float scrollSensitivity;
    [SerializeField] float smoothScrollSensitivity;
    [SerializeField] float orbitSensitivity;
    [SerializeField] float moveSensitivity;
    [SerializeField] bool inverted;

    Vector3 pivotPoint; 
    PointerType currentPointerType;
    Vector3 lastMousePos;

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
        float nearPlaneDist = targetCam.attachedUnityCam.nearClipPlane;
        float tempDist = zoomAmount * scrollSensitivity * GetPivotDistanceScale();
        Vector3 moveDelta = targetCam.transform.forward * Mathf.Clamp(tempDist, Mathf.NegativeInfinity, currentDistToPivot - nearPlaneDist);
        targetCam.transform.position += moveDelta;
    }

    float GetPivotDistanceScale () {
        float currentDistToPivot = (targetCam.transform.position - pivotPoint).magnitude;
        float nearPlaneDist = targetCam.attachedUnityCam.nearClipPlane;
        return Mathf.Max(Mathf.Abs(currentDistToPivot - nearPlaneDist), 0.01f);
    }

    public void OnPointerDown (PointerEventData eventData) {
        if(currentPointerType == PointerType.None){
            currentPointerType = PointerIDToType(eventData.pointerId);
            lastMousePos = Input.mousePosition;
        }
    }

    public void OnPointerUp (PointerEventData eventData) {
        if(PointerIDToType(eventData.pointerId) == currentPointerType){
            currentPointerType = PointerType.None;
        }
    }

    public void OnScroll (PointerEventData eventData) {
        Zoom(eventData.scrollDelta.y);
    }

    PointerType PointerIDToType (int id) {
        switch(id){
            case -1: return PointerType.Left;
            case -2: return PointerType.Right;
            case -3: return PointerType.Middle;
            default: return PointerType.None;
        }
    }

    enum PointerType {
        None,
        Left,
        Right,
        Middle
    }
	
}
