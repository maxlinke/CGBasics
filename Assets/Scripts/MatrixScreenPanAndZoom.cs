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
    [SerializeField] float maxVerticalOverpan;
    [SerializeField] float maxHorizontalOverpan;

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
        KeepEverythingKindaOnscreen();

        void KeepEverythingKindaOnscreen () {
            float minX = Mathf.Infinity;
            float minY = Mathf.Infinity;
            float maxX = Mathf.NegativeInfinity;
            float maxY = Mathf.NegativeInfinity;
            for(int i=0; i<panRT.childCount; i++){
                var c = (RectTransform)(panRT.GetChild(i));
                var cr = c.rect;
                minX = Mathf.Min(minX, cr.position.x);
                minY = Mathf.Min(minY, cr.position.y);
                maxX = Mathf.Max(maxX, cr.position.x + cr.width);
                maxY = Mathf.Max(maxY, cr.position.y + cr.height);
            }
            minX -= maxHorizontalOverpan;
            maxX += maxHorizontalOverpan;
            minY -= maxVerticalOverpan;
            maxY += maxVerticalOverpan;
            panRT.anchoredPosition = new Vector2(
                -Mathf.Clamp(-panRT.anchoredPosition.x, minX, maxX),        // i know. it's weird...
                Mathf.Clamp(panRT.anchoredPosition.y, minY, maxY)
            );                
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
        mouseDelta *= InputSystem.shiftCtrlMultiplier;
        Vector2 panDelta = mouseDelta / zoomLevel;
        panRT.anchoredPosition += panDelta;
    }

    void Zoom (float zoomDelta) {
        zoomDelta *= InputSystem.shiftCtrlMultiplier;
        zoomRT.localScale = Vector3.one * Mathf.Clamp(zoomLevel + (zoomSensitivity * zoomDelta * zoomLevel), minZoom, maxZoom);
    }
	
}
