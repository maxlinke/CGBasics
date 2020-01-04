using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LightingModels {

    public class IntensityGraphDrawer : MonoBehaviour {

        [Header("Prefabs")]
        [SerializeField] IntensityGraphGizmo gizmoPrefab;

        [Header("Components")]
        [SerializeField] Material blitMatTemplate;
        [SerializeField] LightingModelGraphPropNameResolver nameResolver;
        [SerializeField] Image scrollTargetImage;
        [SerializeField] IntensityGraphWindowOverlay windowOverlay;
        [SerializeField] Image planarModeImage;
        [SerializeField] Image planarModeDropShadow;
        [SerializeField] Image sphericalModeImage;
        [SerializeField] Image sphericalModeDropShadow;

        [Header("Gizmo Settings")]
        [SerializeField] GameObject gizmoContainer;
        [SerializeField] RectTransform rotatingGizmoParent;
        [SerializeField] Sprite lightGizmoSprite;
        [SerializeField] float lightGizmoMargins;
        [SerializeField] Sprite viewGizmoSprite;
        [SerializeField] float viewGizmoMargins;

        [Header("\"Camera\" Settings")]
        [SerializeField] Vector2 camRectPos;
        [SerializeField] Vector2 camRectSize;

        [Header("Graph Settings")]
        [SerializeField] float defaultGraphScale;
        [SerializeField] float minGraphScale;
        [SerializeField] float maxGraphScale;
        [SerializeField] float majorLineWidth;
        [SerializeField] float majorLineOpacity;
        [SerializeField] float minorLineWidth;
        [SerializeField] float minorLineOpacity;

        bool initialized = false;
        float graphScale;
        List<ShaderProperty> shaderProperties;
        Camera targetCam;
        Material blitMat;
        LightingScreen lightingScreen;

        IntensityGraphGizmo viewGizmo;
        IntensityGraphGizmo lightGizmo;

        bool planarMode => windowOverlay.planarModeToggle.isOn;

        public void Initialize (LightingScreen lightingScreen) {
            CreateCamera();
            CreateBlitMat();
            CollectShaderProperties();
            SetupBackgroundScoll();
            windowOverlay.Initialize("Reset the zoom", ResetToDefault);
            viewGizmo = CreateGizmo(viewGizmoSprite, viewGizmoMargins);
            lightGizmo = CreateGizmo(lightGizmoSprite, lightGizmoMargins);
            this.lightingScreen = lightingScreen;
            this.initialized = true;
            ResetToDefault();

            void CreateCamera () {
                targetCam = new GameObject("Intensity Graph Cam", typeof(Camera)).GetComponent<Camera>();
                targetCam.cullingMask = 0;
                targetCam.rect = new Rect(camRectPos, camRectSize);
                targetCam.gameObject.AddComponent<IntensityGraphCam>().onRenderImage += BlitGraphEffect;
            }

            void CreateBlitMat () {
                blitMat = Instantiate(blitMatTemplate);
                blitMat.hideFlags = HideFlags.HideAndDontSave;
            }

            void CollectShaderProperties () {
                shaderProperties = new List<ShaderProperty>();
                string debugOut = string.Empty;
                foreach(var sp in Resources.FindObjectsOfTypeAll<ShaderProperty>()){        // TODO does this work 100% of the time?
                    shaderProperties.Add(sp);
                    debugOut += $"{sp.name}, ";
                }
                if(shaderProperties.Count < 1){
                    Debug.LogError("Didn't find any shader properties!!!");
                }
                // if(debugOut.Length > 0){
                //     debugOut = debugOut.Remove(debugOut.Length - 2);     // to remove the last ", "
                //     Debug.Log($"Found obects of type {nameof(ShaderProperty)}: {debugOut}.");
                // }
            }

            void SetupBackgroundScoll () {
                scrollTargetImage.raycastTarget = true;
                scrollTargetImage.color = Color.clear;
                scrollTargetImage.gameObject.AddComponent<BackgroundScroll>().onScroll += (delta) => {graphScale = Mathf.Clamp(graphScale - (delta * graphScale), minGraphScale, maxGraphScale);};
            }

            IntensityGraphGizmo CreateGizmo (Sprite sprite, float margins) {
                var newGizmo = Instantiate(gizmoPrefab, rotatingGizmoParent);
                // newGizmo.rectTransform.SetParent(gizmoParent, false);
                newGizmo.rectTransform.ResetLocalScale();
                newGizmo.rectTransform.SetToFillWithMargins(margins);
                newGizmo.SetSprite(sprite);
                return newGizmo;
            }
        }

        void ResetToDefault () {
            graphScale = defaultGraphScale;
        }

        public void LoadColors (ColorScheme cs) {
            blitMat.SetColor("_BackgroundColor", cs.LSIGBackground);
            blitMat.SetColor("_ForegroundColor", cs.LSIGGraph);
            var fg = cs.LSIGUIElements;
            var bg = cs.LSIGUIElementDropShadow;
            planarModeImage.color = fg;
            planarModeDropShadow.color = bg;
            sphericalModeImage.color = fg;
            sphericalModeDropShadow.color = bg;
            lightGizmo.LoadColors(cs);
            viewGizmo.LoadColors(cs);
            windowOverlay.LoadColors(cs);
        }

        public void UpdateLightingModels (LightingModel diffLM, LightingModel specLM) {
            string diffName = diffLM != null ? diffLM.name : "null";
            string specName = specLM != null ? specLM.name : "null";
            // Debug.Log($"Diff: {diffName}, Spec: {specName}");
            foreach(var link in nameResolver){
                if(link.lm == diffLM || link.lm == specLM){
                    blitMat.SetFloat(link.propName, 1f);
                }else{
                    blitMat.SetFloat(link.propName, 0f);
                }
            }
        }

        void OnDestroy () {
            if(targetCam != null && targetCam.gameObject != null){
                Destroy(targetCam.gameObject);
            }
            if(blitMat != null){
                DestroyImmediate(blitMat);
            }
        }

        void LateUpdate () {
            if(!initialized){
                return;
            }
            var lv = CalculateLightAndViewAngles();
            UpdateGraphMatValues(lightingScreen.GetMaterialPropertyBlock());
            UpdateGizmos();

            void UpdateGraphMatValues (MaterialPropertyBlock mpb) {
                blitMat.SetFloat("_GraphScale", graphScale);
                if(windowOverlay.concentricLineToggle.isOn){
                    blitMat.SetFloat("_MajorLineWidth", graphScale * majorLineWidth / Mathf.Min(Screen.width, Screen.height));
                    blitMat.SetFloat("_MajorLineOpacity", Mathf.Clamp01(majorLineOpacity / graphScale));
                    blitMat.SetFloat("_MinorLineWidth", graphScale * minorLineWidth / Mathf.Min(Screen.width, Screen.height));
                    blitMat.SetFloat("_MinorLineOpacity", Mathf.Clamp01(minorLineOpacity / graphScale));
                }else{
                    blitMat.SetFloat("_MajorLineOpacity", 0);
                    blitMat.SetFloat("_MinorLineOpacity", 0);
                }
                foreach(var sp in shaderProperties){
                    if(sp.type == ShaderProperty.Type.Float){
                        blitMat.SetFloat(sp.name, mpb.GetFloat(sp.name));
                    }else if(sp.type == ShaderProperty.Type.Color){
                        blitMat.SetColor(sp.name, mpb.GetColor(sp.name));
                    }else{
                        Debug.LogError($"WAT?!?!?! {sp.type}");
                    }
                }
                blitMat.SetColor("_AmbientCol", RenderSettings.ambientLight);
                blitMat.SetColor("_LightCol", lightingScreen.GetMainLightColor());
                
                blitMat.SetVector("_LightDir", new Vector4(Mathf.Sin(lv.lightAngle), Mathf.Cos(lv.lightAngle), 0, 0));
                blitMat.SetVector("_ViewDir", new Vector4(Mathf.Sin(lv.viewAngle), Mathf.Cos(lv.viewAngle), 0, 0));                

                blitMat.SetFloat("_PlanarMode", planarMode ? 1 : 0);
            }

            void UpdateGizmos () {
                lightGizmo.SetRotation(-lv.lightAngle * Mathf.Rad2Deg);
                viewGizmo.SetRotation(-lv.viewAngle * Mathf.Rad2Deg);
                bool updateImages = false;
                updateImages |= (planarMode && (!planarModeImage.GOActiveSelf() || sphericalModeImage.GOActiveSelf()));
                updateImages |= (!planarMode && (planarModeImage.GOActiveSelf() || !sphericalModeImage.GOActiveSelf()));
                if(updateImages){
                    UpdateModeImages();     
                }
                if(planarMode && viewGizmo.GOActiveSelf()){
                    viewGizmo.gameObject.SetActive(false);
                }else if(!planarMode && !viewGizmo.GOActiveSelf()){
                    viewGizmo.gameObject.SetActive(true);
                }
                if(windowOverlay.gizmoToggle.isOn && !gizmoContainer.activeSelf){
                    gizmoContainer.SetActive(true);
                }else if(!windowOverlay.gizmoToggle.isOn && gizmoContainer.activeSelf){
                    gizmoContainer.SetActive(false);
                }

                void UpdateModeImages () {
                    planarModeImage.SetGOActive(planarMode);
                    planarModeDropShadow.SetGOActive(planarMode);
                    sphericalModeImage.SetGOActive(!planarMode);
                    sphericalModeDropShadow.SetGOActive(!planarMode);
                }
            }

            // works well enough
            (float lightAngle, float viewAngle) CalculateLightAndViewAngles () {
                var lDir = lightingScreen.GetMainLightDir();
                var vDir = lightingScreen.GetCamViewDir();
                float lightAngle,  viewAngle;
                if(planarMode){
                    var input = ConstructRotationMatrix(lDir) * lDir ;
                    lightAngle = Mathf.Atan2(input.x, input.z);
                    viewAngle = 0f;
                }else{
                    var avg = (0.1f * lDir + vDir).normalized;
                    var newBase = ConstructRotationMatrix(avg);
                    var refVec = newBase * avg;
                    var refAngle = Mathf.Atan2(refVec.x, refVec.z);
                    var deltaAngle = Mathf.Deg2Rad * Vector3.Angle(lDir, vDir);
                    lightAngle = refAngle - deltaAngle / 2;
                    viewAngle = refAngle + deltaAngle / 2;
                }
                return (lightAngle, viewAngle);

                Matrix4x4 ConstructRotationMatrix (Vector3 neutralVector) {
                    var nz = Vector3.forward;
                    var tx = neutralVector;
                    var ny = Vector3.Cross(tx, nz);
                    var nx = Vector3.Cross(-ny, nz);
                    return new Matrix4x4(nx, ny, nz, new Vector4(0,0,0,1)).transpose;
                }
            }
        }

        void BlitGraphEffect (RenderTexture src, RenderTexture dst) {
            Graphics.Blit(src, dst, blitMat);
        }

        class IntensityGraphCam : MonoBehaviour {

            public event System.Action<RenderTexture, RenderTexture> onRenderImage = delegate {};

            void OnRenderImage (RenderTexture src, RenderTexture dst) {
                onRenderImage.Invoke(src, dst);
            }

        }

        class BackgroundScroll : ClickDragScrollHandler {

            public event System.Action<float> onScroll = delegate {};

            Vector3 lastMousePos;
            PointerType currentPointerType;

            void Update () {
                if(currentPointerType != PointerType.Middle){
                    return;
                }
                var currentMousePos = Input.mousePosition;
                float mouseDelta = currentMousePos.y - lastMousePos.y;
                onScroll.Invoke(mouseDelta * 0.02f * InputSystem.shiftCtrlMultiplier);
                lastMousePos = currentMousePos;
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

            protected override void Scroll(PointerEventData ped) {
                onScroll.Invoke(ped.scrollDelta.y * 0.1f * InputSystem.shiftCtrlMultiplier);
            }
        }

    }

}
