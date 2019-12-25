﻿using UnityEngine;
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

            void KeepEverythingKindaOnscreen () {       // TODO depth
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
            Zoom(ped.scrollDelta.y, true);
        }

        void Pan (Vector2 mouseDelta) {
            mouseDelta *= InputSystem.shiftCtrlMultiplier;
            Vector2 panDelta = mouseDelta / zoomLevel;
            panRT.anchoredPosition += panDelta;
        }

        void Zoom (float zoomDelta, bool onCursor) {
            if(onCursor){
                zoomDelta = Mathf.Sign(zoomDelta);  // TODO this is just a band-aid because a higher scrolldelta 
            }
            zoomDelta *= InputSystem.shiftCtrlMultiplier;   // and this doesn't work properly either...
            if(onCursor){
                Pan(-LocalMousePos(Input.mousePosition) * zoomDelta / (zoomDelta < 0 ? 4 : 6)); // that ternary fucks with me...
            }
            zoomRT.localScale = Vector3.one * Mathf.Clamp(zoomLevel + (zoomSensitivity * zoomDelta * zoomLevel), minZoom, maxZoom);
        }

        public void ResetView () {
            zoomRT.localScale = Vector3.one;
            panRT.anchoredPosition = Vector2.zero;
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

}