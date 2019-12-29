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

        [SerializeField] RenderViewWindowOverlay windowOverlay;

        [Header("Settings")]
        [SerializeField] int renderLayer;
        [SerializeField] float lightGizmoDistance;
        [SerializeField] float lightGizmoRayLength;
        [SerializeField, Range(0, 90)] float topBottomLimit;

        [Header("Cam Settings")]
        [SerializeField] bool camAllowAA;
        [SerializeField] Vector2 camRectPosition;
        [SerializeField] Vector2 camRectDimensions;
        [SerializeField] Vector3 defaultEuler;
        [SerializeField] float defaultFOV;
        [SerializeField] float defaultNearClip;
        [SerializeField] float defaultFarClip;
        [SerializeField] float defaultDist;
        [SerializeField] float minDist;
        [SerializeField] float maxDist;

        bool initialized = false;

        LightingScreen lightingScreen;
        Camera cam;
        Transform camYRotParent;
        Transform camXRotParent;
        float camRotX;
        float camRotY;
        Transform lightsParent;
        List<Light> lights;
        GameObject renderObject;
        MeshFilter renderObjectMF;
        MeshRenderer renderObjectMR;

        Material glMatSolid;
        Material glMatTransparent;
        Color wireFloorColor;
        Color camBackgroundColor;

        PointerType currentPointerType;
        Vector3 lastMousePos;

        Vector3 pivotPoint => Vector3.zero;

        public int lightCount => lights.Count;
        public Light this[int index] => lights[index];
        public Vector3 mainLightDir => lights[0].transform.forward;
        public Color mainLightColor => lights[0].color;
        public Vector3 camDir => cam.transform.forward;

        void OnDestroy () {         // these nullchecks are basically only here for the editor...
            if(camYRotParent != null){
                Destroy(camYRotParent.gameObject);
            }
            if(lightsParent != null){
                Destroy(lightsParent.gameObject);
            }
            if(renderObject != null){
                Destroy(renderObject);
            }
        }

        public void LoadColors (ColorScheme cs) {
            // cam.backgroundColor = cs.LightingScreenRenderBackground;
            camBackgroundColor = cs.LightingScreenRenderBackground;
            wireFloorColor = cs.LightingScreenRenderGrid;
            windowOverlay.LoadColors(cs);
        }

        public void Initialize (LightingScreen lightingScreen) {
            if(initialized){
                Debug.LogError("Duplicate init call, aborting!");
                return;
            }
            this.lightingScreen = lightingScreen;
            CreateCam();
            CreateRenderObject();
            CreateLightsParent();
            lights = new List<Light>();
            windowOverlay.Initialize(ResetCamera, initialAmbientToggleState: false, initialGizmoToggleState: true);

            this.initialized = true;
            ResetCamera();

            void CreateCam () {
                camYRotParent = new GameObject("Cam Y Rotation Parent").transform;
                camYRotParent.position = pivotPoint;
                camXRotParent = new GameObject("Cam X Rotation Parent").transform;
                camXRotParent.SetParent(camYRotParent, false);
                camXRotParent.ResetLocalScale();
                cam = new GameObject("Render Cam", typeof(Camera)).GetComponent<Camera>();
                cam.cullingMask = 1 << renderLayer;
                cam.backgroundColor = Color.black;
                cam.allowMSAA = camAllowAA;             // TODO disable in web-version?
                cam.transform.SetParent(camXRotParent, false);
                cam.transform.ResetLocalScale();
                var camScript = cam.gameObject.AddComponent<ControlledCamera>();
                camScript.onPreRender += OnCamPreRender;
                camScript.onPostRender += OnCamPostRender;
            }

            void CreateRenderObject () {
                renderObject = new GameObject("Render Object", typeof(MeshFilter), typeof(MeshRenderer));
                renderObjectMF = renderObject.GetComponent<MeshFilter>();
                renderObjectMR = renderObject.GetComponent<MeshRenderer>();
                renderObject.layer = renderLayer;
            }

            void CreateLightsParent () {
                lightsParent = new GameObject("Lights parent").transform;
                lightsParent.position = pivotPoint;
                lightsParent.gameObject.layer = renderLayer;
            }
        }

        void OnCamPreRender () {
            cam.backgroundColor = windowOverlay.bgIsAmientColorToggle.isOn ? RenderSettings.ambientLight : camBackgroundColor;
        }

        void OnCamPostRender () {
            if(!initialized || (currentPointerType == PointerType.None && !windowOverlay.gizmoToggle.isOn)){
                return;
            }
            if(glMatSolid == null){
                glMatSolid = CustomGLCamera.GetLineMaterial(false);
                glMatSolid.renderQueue = (int)(UnityEngine.Rendering.RenderQueue.Geometry) + 10;
            }
            if(glMatTransparent == null){
                glMatTransparent = CustomGLCamera.GetLineMaterial(true);
                glMatTransparent.renderQueue = (int)(UnityEngine.Rendering.RenderQueue.Geometry) + 11;
            }
            GL.PushMatrix();
            var proj = GLMatrixCreator.GetProjectionMatrix(cam.fieldOfView, cam.aspect, cam.nearClipPlane, cam.farClipPlane);
            var view = GLMatrixCreator.GetViewMatrix(cam.transform.position - pivotPoint, cam.transform.forward, cam.transform.up);
            GL.LoadIdentity();
            GL.LoadProjectionMatrix(proj);
            GL.MultMatrix(view);
            if(currentPointerType != PointerType.None){
                DrawTheThings(true);
            }
            DrawTheThings(false);
            GL.PopMatrix();

            void DrawTheThings (bool seeThrough) {
                if(seeThrough){
                    glMatTransparent.SetPass(0);
                }else{
                    glMatSolid.SetPass(0);
                }
                CustomGLCamera.DrawWireFloor(wireFloorColor, seeThrough, false);
                foreach(var l in lights){
                    Vector3 pos = pivotPoint - (l.transform.forward * lightGizmoDistance);
                    Vector3 rayEnd = pos + (l.transform.forward * lightGizmoRayLength);
                    var lCol = CustomGLCamera.GetConditionalSeeThroughColor(l.color, seeThrough);
                    CustomGLCamera.GLDraw(GL.LINES, () => {
                        GL.Color(lCol);
                        GL.Vertex(pos);
                        GL.Vertex(rayEnd);
                    });
                    CustomGLCamera.DrawWithNewMVPMatrix(Matrix4x4.identity, () => {
                        CustomGLCamera.DrawClipspacePoint(GetClipPoint(pos), lCol, lCol);
                    });

                    Vector3 GetClipPoint (Vector3 inputPoint) {
                        Vector4 p4 = new Vector4(inputPoint.x, inputPoint.y, inputPoint.z, 1);
                        p4 = proj * view * p4;
                        p4 /= p4.w;
                        return new Vector3(p4.x, p4.y, p4.z);
                    }
                }
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
        }

        void LateUpdate () {
            if(!initialized){
                return;
            }
            renderObjectMR.SetPropertyBlock(lightingScreen.GetMaterialPropertyBlock());
        }

        void OrbitCam (Vector3 mouseDelta) {
            mouseDelta *= InputSystem.shiftCtrlMultiplier * orbitSensitivity;
            camRotY = Mathf.Repeat(camRotY + mouseDelta.x, 360f);
            camRotX = Mathf.Clamp(camRotX - mouseDelta.y, -topBottomLimit, topBottomLimit);
            ApplyCamRotation();
        }

        void ApplyCamRotation () {
            camYRotParent.localEulerAngles = new Vector3(0f, camRotY, 0f);
            camXRotParent.localEulerAngles = new Vector3(camRotX, 0f, 0f);
        }

        void ZoomCam (float zoomAmount) {
            zoomAmount *= InputSystem.shiftCtrlMultiplier;
            float moveDist = zoomAmount * scrollSensitivity * GetPivotDistanceScale();
            float currentDist = -cam.transform.localPosition.z;
            cam.transform.localPosition = new Vector3(0f, 0f, -Mathf.Clamp(currentDist - moveDist, minDist, maxDist));
        }

        void RotateLights (Vector3 mouseDelta) {
            mouseDelta *= InputSystem.shiftCtrlMultiplier * orbitSensitivity;
            lightsParent.Rotate(cam.transform.up, -mouseDelta.x, Space.World);
            lightsParent.Rotate(cam.transform.right, mouseDelta.y, Space.World);
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
            camYRotParent.position = pivotPoint;
            camXRotParent.localPosition = Vector3.zero;
            camRotY = defaultEuler.y;
            camRotX = defaultEuler.x;
            ApplyCamRotation();
            cam.transform.localPosition = new Vector3(0f, 0f, -defaultDist);
            cam.transform.localRotation = Quaternion.identity;
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
            renderObject.transform.position = pivotPoint + (-newMesh.bounds.center / size);
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

        public void LoadLightingSetup (LightingSetup setup, bool applyEuler = true) {
            for(int i=lightsParent.childCount-1; i>=0; i--){
                Destroy(lightsParent.GetChild(i).gameObject);
            }
            lights.Clear();
            int lightIndex = 0;
            foreach(var l in setup){
                var newLight = new GameObject($"Light {lightIndex}", typeof(Light)).GetComponent<Light>();
                newLight.type = LightType.Directional;
                newLight.shadows = LightShadows.None;
                newLight.intensity = 1f;
                newLight.color = l.color;
                lights.Add(newLight);
                newLight.transform.SetParent(lightsParent, false);
                var normedLPos = l.position.normalized;
                newLight.transform.localPosition = normedLPos;
                newLight.transform.localRotation = Quaternion.LookRotation(-normedLPos);
                lightIndex++;
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.ambientLight = setup.ambientColor;
            if(applyEuler){
                lightsParent.localEulerAngles = setup.defaultEuler;
            }
        }

        public void UpdateLightColor (int lightIndex, Color newColor) {
            if(lightIndex < 0 || lightIndex >= lights.Count){
                Debug.LogError("Light index out of bounds! Aborting!", this.gameObject);
                return;
            }
            lights[lightIndex].color = newColor;
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

        private class ControlledCamera : MonoBehaviour {

            public event System.Action onPreRender = delegate {};
            public event System.Action onPostRender = delegate {};

            void OnPreRender () {
                onPreRender.Invoke();
            }

            void OnPostRender () {
                onPostRender.Invoke();
            }

        }
        
    }

}