using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MatrixScreenPanAndZoom : ClickDragScrollHandler {

    [Header("Components")]
    [SerializeField] RectTransform zoomRT;
    [SerializeField] RectTransform panRT;

    [Header("Settings")]
    [SerializeField] float zoomSensitivity;
    [SerializeField] float smoothZoomMultiplier;
    [SerializeField] float minZoom;
    [SerializeField] float maxZoom;

    PointerType currentPointerType;
    Vector3 lastMousePos;

    public float zoomLevel => zoomRT.UniformLocalScale();

    void Update () {
        if(currentPointerType != PointerType.None){
            var currentMousePos = Input.mousePosition;
            var mouseDelta = currentMousePos - lastMousePos;
            switch(currentPointerType){
                case PointerType.Right:
                    Pan(mouseDelta);
                    break;
                case PointerType.Middle:
                    Zoom(mouseDelta.y * smoothZoomMultiplier);
                    break;
                default:
                    break;
            }
            lastMousePos = currentMousePos;
        }
    }

    protected override void PointerDown (PointerEventData ped) {
        if(currentPointerType == PointerType.None){
            currentPointerType = PointerIDToType(ped.pointerId);
            lastMousePos = Input.mousePosition;
        }
    }

    protected override void PointerUp (PointerEventData ped) {
        if(PointerIDToType(ped.pointerId) == currentPointerType){
            currentPointerType = PointerType.None;
        }
    }

    protected override void Scroll (PointerEventData ped) {
        Zoom(ped.scrollDelta.y);
    }

    void Pan (Vector2 mouseDelta) {
        Vector2 panDelta = mouseDelta / zoomLevel;
        panRT.anchoredPosition += panDelta;
    }

    void Zoom (float zoomDelta) {
        zoomRT.localScale = Vector3.one * Mathf.Clamp(zoomLevel + (zoomSensitivity * zoomDelta * zoomLevel), minZoom, maxZoom);
    }
	
}
