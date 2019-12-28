using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LightingModels {

    public class IntensityGraphDrawer : MonoBehaviour {

        [Header("Components")]
        [SerializeField] Material blitMatTemplate;
        [SerializeField] LightingModelGraphPropNameResolver nameResolver;
        [SerializeField] Image scrollTargetImage;
        [SerializeField] IntensityGraphWindowOverlay windowOverlay;
        [SerializeField] RectTransform lightGizmoParent;
        [SerializeField] RectTransform viewGizmoParent;
        [SerializeField] List<Graphic> regularColorUIElements;
        [SerializeField] List<Graphic> dropShadowUIElements;

        [Header("Settings")]
        [SerializeField] Vector2 camRectPos;
        [SerializeField] Vector2 camRectSize;

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

        public void Initialize (LightingScreen lightingScreen) {
            CreateCamera();
            CreateBlitMat();
            CollectShaderProperties();
            SetupBackgroundScoll();
            windowOverlay.Initialize("Reset the zoom", ResetToDefault);
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
                if(debugOut.Length > 0){
                    debugOut = debugOut.Remove(debugOut.Length - 2);     // to remove the last ", "
                    Debug.Log($"Found obects of type {nameof(ShaderProperty)}: {debugOut}.");
                }
            }

            void SetupBackgroundScoll () {
                scrollTargetImage.raycastTarget = true;
                scrollTargetImage.color = Color.clear;
                scrollTargetImage.gameObject.AddComponent<BackgroundScroll>().onScroll += (delta) => {graphScale = Mathf.Clamp(graphScale - (delta * graphScale), minGraphScale, maxGraphScale);};
            }
        }

        void ResetToDefault () {
            graphScale = defaultGraphScale;
        }

        public void LoadColors (ColorScheme cs) {
            blitMat.SetColor("_BackgroundColor", cs.LSIGBackground);
            blitMat.SetColor("_ForegroundColor", cs.LSIGGraph);
            foreach(var g in regularColorUIElements){
                g.color = cs.LSIGUIElements;
            }
            foreach(var g in dropShadowUIElements){
                g.color = cs.LSIGUIElementDropShadow;
            }
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
        }

        void LateUpdate () {
            if(!initialized){
                return;
            }
            UpdateGraphMatValues(lightingScreen.GetMaterialPropertyBlock());

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

                var lv = CalculateLightAndViewAngles();
                blitMat.SetVector("_LightDir", new Vector4(Mathf.Sin(lv.lightAngle), Mathf.Cos(lv.lightAngle), 0, 0));
                blitMat.SetVector("_ViewDir", new Vector4(Mathf.Sin(lv.viewAngle), Mathf.Cos(lv.viewAngle), 0, 0));

                lightGizmoParent.localEulerAngles = new Vector3(0, 0, -lv.lightAngle * Mathf.Rad2Deg);
                lightGizmoParent.GetChild(0).localEulerAngles = -lightGizmoParent.localEulerAngles;
                viewGizmoParent.localEulerAngles = new Vector3(0, 0, -lv.viewAngle * Mathf.Rad2Deg);
                viewGizmoParent.GetChild(0).localEulerAngles = -viewGizmoParent.localEulerAngles;

                // works well enough
                (float lightAngle, float viewAngle) CalculateLightAndViewAngles () {
                    var lDir = lightingScreen.GetMainLightDir();
                    var vDir = lightingScreen.GetCamViewDir();
                    var avg = (0.8f * lDir + vDir).normalized;
                    var refAngle = Mathf.Atan2(avg.x, avg.y);
                    var deltaAngle = Mathf.Deg2Rad * Vector3.Angle(lDir, vDir);
                    var lightAngle = refAngle - deltaAngle / 2;
                    var viewAngle = refAngle + deltaAngle / 2;
                    return (lightAngle, viewAngle);
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
