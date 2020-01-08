using UnityEngine;
using UnityEngine.EventSystems;

namespace MatrixScreenUtils {

    public class PanAndZoom : ClickDragScrollHandler {

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
        public float maxZoomLevel => maxZoom;

        void Update () {
            if(currentPointerType != PointerType.None){
                var currentMousePos = Input.mousePosition;
                var mouseDelta = currentMousePos - lastMousePos;
                switch(currentPointerType){
                    case PointerType.Left:
                        Pan(mouseDelta);
                        break;
                    case PointerType.Right:
                        Pan(mouseDelta);
                        break;
                    case PointerType.Middle:
                        Zoom(mouseDelta.y * smoothZoomMultiplier, false);
                        break;
                    default:
                        break;
                }
                lastMousePos = currentMousePos;
            }
            KeepEverythingKindaOnscreen();

            void KeepEverythingKindaOnscreen () {
                GetContentDimensions(out var min, out var max);
                min.x -= maxHorizontalOverpan;
                max.x += maxHorizontalOverpan;
                min.y -= maxVerticalOverpan;
                max.y += maxVerticalOverpan;
                panRT.anchoredPosition = new Vector2(
                    -Mathf.Clamp(-panRT.anchoredPosition.x, min.x, max.x),        // i know. it's weird...
                    Mathf.Clamp(panRT.anchoredPosition.y, min.y, max.y)
                );                
            }
        }

        // doesn't really work but it works well enough...
        void GetContentDimensions (out Vector2 min, out Vector2 max) {
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
            min = new Vector2(minX, minY);
            max = new Vector2(maxX, maxY);
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
            Zoom(ped.scrollDelta.y, true);
        }

        void Pan (Vector2 mouseDelta) {
            mouseDelta *= InputSystem.shiftCtrlMultiplier;
            Vector2 panDelta = mouseDelta / zoomLevel;
            panRT.anchoredPosition += panDelta;
        }

        void Zoom (float zoomDelta, bool onCursor) {
            zoomDelta *= InputSystem.shiftCtrlMultiplier;
            var origZoom = zoomLevel;
            var newZoom = Mathf.Clamp(zoomLevel + (zoomSensitivity * zoomDelta * zoomLevel), minZoom, maxZoom);
            zoomRT.localScale = Vector3.one * newZoom;
            if(onCursor){
                var realDelta = newZoom - origZoom;
                var scaledOrigAPos = panRT.anchoredPosition * origZoom;
                var scaledNewAPos = panRT.anchoredPosition * newZoom;
                var implicitAPosDelta = scaledNewAPos - scaledOrigAPos;
                panRT.anchoredPosition -= implicitAPosDelta / newZoom;      // as if it was always scaled around the pivot (removes the "offset" that comes from scaling)
                var cursorPos = LocalMousePos(Input.mousePosition);
                var offset = panRT.anchoredPosition * newZoom - cursorPos;
                var deltaOffset = offset * ((newZoom / origZoom) - 1);
                panRT.anchoredPosition += deltaOffset / newZoom;
            }            

            Vector2 LocalMousePos (Vector3 inputMPos) {
                var mPos = inputMPos;
                var x = mPos.x;
                var y = mPos.y;
                y -= Screen.height / 2;      // because the rect of the area is the upper half
                var output = new Vector2(x - (Screen.width / 2), y - (Screen.height / 4));  // because it's the entire width and half the height
                return output;
            }
        }

        public void ResetView () {
            if(panRT.childCount == 0){
                zoomRT.localScale = Vector3.one;
                panRT.anchoredPosition = Vector2.zero;
            }else{
                GetContentDimensions(out var min, out var max);
                zoomRT.localScale = Vector3.one * Mathf.Clamp(Screen.width / (1.167f * Mathf.Abs(min.x - max.x)), minZoom, maxZoom);    // HACK magic numbers yey...
                panRT.anchoredPosition = new Vector2((min.x + max.x) / 2f, 0);
            }
        }
    
    }

}