using UnityEngine;
using UnityEngine.EventSystems;

public class CustomCameraUIController : MonoBehaviour, IScrollHandler, IPointerDownHandler, IPointerUpHandler {

    [Header("References")]
    [SerializeField] CustomCamera targetCustomCam;

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
            Debug.Log(currentPointerType);
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

        void EnsureCameraLookingAtPivot () {
            Vector3 origUp = targetCustomCam.transform.up;
            Vector3 customUp;
            if(Mathf.Abs(origUp.y) > 0.1f){
                customUp = Vector3.up * Mathf.Sign(origUp.y);
            }else{
                customUp = Vector3.ProjectOnPlane(origUp, Vector3.up);
            }
            targetCustomCam.transform.LookAt(pivotPoint, customUp);
        }
    }

    void Orbit (Vector3 mouseDelta) {
        targetCustomCam.transform.RotateAround(pivotPoint, Vector3.up, mouseDelta.x * (inverted ? -1 : 1));
        targetCustomCam.transform.RotateAround(pivotPoint, targetCustomCam.transform.right, -mouseDelta.y);
    }

    void Move (Vector3 mouseDelta) {
        Vector3 moveDelta = moveSensitivity * GetPivotDistanceScale() * -1 * (targetCustomCam.transform.right * mouseDelta.x + targetCustomCam.transform.up * mouseDelta.y);
        pivotPoint += moveDelta;
        targetCustomCam.transform.position += moveDelta;
    }

    void Zoom (float zoomAmount) {
        float currentDistToPivot = (targetCustomCam.transform.position - pivotPoint).magnitude;
        float nearPlaneDist = targetCustomCam.camera.nearClipPlane;
        float tempDist = zoomAmount * scrollSensitivity * GetPivotDistanceScale();
        Vector3 moveDelta = targetCustomCam.transform.forward * Mathf.Clamp(tempDist, Mathf.NegativeInfinity, currentDistToPivot - nearPlaneDist);
        targetCustomCam.transform.position += moveDelta;
    }

    float GetPivotDistanceScale () {
        float currentDistToPivot = (targetCustomCam.transform.position - pivotPoint).magnitude;
        float nearPlaneDist = targetCustomCam.camera.nearClipPlane;
        return Mathf.Max(Mathf.Abs(currentDistToPivot - nearPlaneDist), 0.01f);
    }

    public void OnPointerDown (PointerEventData eventData) {
        if(currentPointerType == PointerType.None){
            currentPointerType = PointerIDToType(eventData.pointerId);
            lastMousePos = Input.mousePosition;
        }
    }

    public void OnPointerUp (PointerEventData eventData) {
        currentPointerType = PointerType.None;
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
