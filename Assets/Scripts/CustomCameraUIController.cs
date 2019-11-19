using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomCameraUIController : MonoBehaviour, IScrollHandler, IPointerDownHandler, IPointerUpHandler {

    [SerializeField] CustomCamera targetCustomCam;

    [SerializeField] float scrollSensitivity;

    Vector3 pivotPoint; 
    PointerType currentPointerType;
    Vector3 lastMousePos;

    void Start () {
        
    }

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
                    Zoom(mouseDelta.y * 0.2f);
                    break;
                default:
                    break;
            }
            lastMousePos = currentMousePos;
        }
    }

    void Orbit (Vector3 mouseDelta) {

    }

    void Move (Vector3 mouseDelta) {

    }

    void Zoom (float zoomAmount) {
        float currentDistToPivot = (targetCustomCam.transform.position - pivotPoint).magnitude;
        float nearPlaneDist = targetCustomCam.camera.nearClipPlane;
        float tempDist = zoomAmount * scrollSensitivity * Mathf.Max(Mathf.Abs(currentDistToPivot - nearPlaneDist), 0.01f);
        Vector3 moveDelta = targetCustomCam.transform.forward * Mathf.Clamp(tempDist, Mathf.NegativeInfinity, currentDistToPivot - nearPlaneDist);
        targetCustomCam.transform.position += moveDelta;
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
