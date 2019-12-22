using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LightingModels {

    public class UIRenderViewController : ClickDragScrollHandler {

        // theres quite a bit of overlap with the matrix screen cam controller here, i know
        // but making them inherit from the same thing would be a LOT of work, so i'll just make this one its own thing

        float scrollSensitivity => CustomCameraUIController.scrollSensitivity;
        float smoothScrollSensitivity => CustomCameraUIController.smoothScrollSensitivity;
        float orbitSensitivity => CustomCameraUIController.orbitSensitivity;

        // TODO window overlay 

        [Header("Settings")]
        [SerializeField] int renderLayer;

        [Header("Cam Settings")]
        [SerializeField] Vector2 camRectPosition;
        [SerializeField] Vector2 camRectDimensions;
        [SerializeField] Vector3 defaultDirection;
        [SerializeField] float defaultFOV;
        [SerializeField] float defaultNearClip;
        [SerializeField] float defaultFarClip;
        [SerializeField] float defaultDist;
        [SerializeField] float minDist;
        [SerializeField] float maxDist;

        bool initialized = false;

        Camera cam;
        Transform lightsParent;
        GameObject renderObject;
        MeshFilter renderObjectMF;
        public MeshRenderer renderObjectMR { get; private set; }

        PointerType currentPointerType;
        Vector3 lastMousePos;

        Vector3 pivotPoint => Vector3.zero;

        public void Initialize () {
            if(initialized){
                Debug.LogError("Duplicate init call, aborting!");
                return;
            }
            CreateCam();
            CreateRenderObject();
            CreateLightsParent();

            this.initialized = true;
            ResetCamera();

            void CreateCam () {
                cam = new GameObject("Render Cam", typeof(Camera)).GetComponent<Camera>();
                cam.cullingMask = 1 << renderLayer;
            }

            void CreateRenderObject () {
                renderObject = new GameObject("Render Object", typeof(MeshFilter), typeof(MeshRenderer));
                renderObjectMF = renderObject.GetComponent<MeshFilter>();
                renderObjectMR = renderObject.GetComponent<MeshRenderer>();
                renderObject.layer = renderLayer;
            }

            void CreateLightsParent () {
                lightsParent = new GameObject("Lights parent").transform;
                lightsParent.gameObject.layer = renderLayer;
                // this is temporary
                var newLight = lightsParent.gameObject.AddComponent<Light>();
                newLight.type = LightType.Directional;
                newLight.color = Color.white;
                newLight.intensity = 1f;
            }
        }

        void Update () {
            if(!initialized){
                return;
            }
            if(currentPointerType != PointerType.None){
                var currentMousePos = Input.mousePosition;
                var mouseDelta = currentMousePos - lastMousePos;
                switch(currentPointerType){
                    case PointerType.Left:
                        OrbitCam(mouseDelta);
                        break;
                    case PointerType.Right:
                        RotateLights(mouseDelta);
                        break;
                    case PointerType.Middle:
                        ZoomCam(mouseDelta.y * smoothScrollSensitivity);
                        break;
                    default:
                        break;
                }
                lastMousePos = currentMousePos;
            }
            KeepCamInDistanceBounds();
            EnsureCamLookingAtPivot();

            void KeepCamInDistanceBounds () {
                var dir = cam.transform.position - pivotPoint;
                float dist = dir.magnitude;
                cam.transform.position = pivotPoint + (dir.normalized * Mathf.Clamp(dist, minDist, maxDist));
            }
            
            void EnsureCamLookingAtPivot () {
                Vector3 origUp = cam.transform.up;
                Vector3 customUp;
                if(Mathf.Abs(origUp.y) > 0.1f){
                    customUp = Vector3.up * Mathf.Sign(origUp.y);
                }else{
                    customUp = Vector3.ProjectOnPlane(origUp, Vector3.up);
                }
                cam.transform.LookAt(pivotPoint, customUp);
            }
        }

        void OrbitCam (Vector3 mouseDelta) {
            mouseDelta *= InputSystem.shiftCtrlMultiplier * orbitSensitivity;
            cam.transform.RotateAround(pivotPoint, Vector3.up, mouseDelta.x);
            cam.transform.RotateAround(pivotPoint, cam.transform.right, -mouseDelta.y);
        }

        void ZoomCam (float zoomAmount) {
            zoomAmount *= InputSystem.shiftCtrlMultiplier;
            float moveDist = zoomAmount * scrollSensitivity * GetPivotDistanceScale();
            float currentDist = (pivotPoint - cam.transform.position).magnitude;
            moveDist = Mathf.Clamp(moveDist, -(maxDist - currentDist), currentDist - minDist);
            cam.transform.position += cam.transform.forward * moveDist;
        }

        // TODO this certainly doesn't work as intended, right?
        void RotateLights (Vector3 mouseDelta) {
            mouseDelta *= InputSystem.shiftCtrlMultiplier * orbitSensitivity;
            lightsParent.Rotate(Vector3.up, -mouseDelta.x, Space.World);
            lightsParent.Rotate(lightsParent.transform.right, mouseDelta.y, Space.World);
        }

        bool NotYetInitAbort () {
            if(!initialized){
                Debug.LogWarning("not yet initialized, aborting!", this.gameObject);
            }
            return !initialized;
        }

        public void ResetCamera () {
            if(NotYetInitAbort()){
                return;
            }
            cam.transform.position = defaultDirection.normalized * defaultDist;
            cam.rect = new Rect(camRectPosition, camRectDimensions);
            cam.orthographic = false;
            cam.fieldOfView = defaultFOV;
            cam.nearClipPlane = defaultNearClip;
            cam.farClipPlane = defaultFarClip;
            cam.ResetAspect();
        }

        public void ResetLightRotation () {
            if(NotYetInitAbort()){
                return;
            }
            lightsParent.rotation = Quaternion.identity;
        }

        public void LoadMesh (Mesh newMesh) {
            if(NotYetInitAbort()){
                return;
            }
            renderObjectMF.sharedMesh = newMesh;
            float size = newMesh.bounds.extents.magnitude;
            renderObject.transform.localScale = Vector3.one / size;
            renderObject.transform.localPosition = pivotPoint + (-newMesh.bounds.center / size);
        }

        public void LoadMaterials (Material diffMat, Material specMat) {
            if(NotYetInitAbort()){
                return;
            }
            var newMats = new List<Material>();
            if(diffMat != null){
                newMats.Add(diffMat);
            }
            if(specMat != null){
                newMats.Add(specMat);
            }
            if(newMats.Count == 0){
                Debug.LogWarning("No materials, are you sure?");
            }
            renderObjectMR.sharedMaterials = newMats.ToArray();
        }

        float GetPivotDistanceScale () {
            float currentDistToPivot = cam.transform.position.magnitude;
            float nearPlaneDist = cam.nearClipPlane;
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
            ZoomCam(eventData.scrollDelta.y);
        }
        
    }

}