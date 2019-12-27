using System.Collections.Generic;
using UnityEngine;

public class IntensityGraphDrawer : MonoBehaviour {

    [Header("Components")]
    [SerializeField] Material blitMatTemplate;
    [SerializeField] LightingModelGraphPropNameResolver nameResolver;

    [Header("Settings")]
    [SerializeField] Vector2 camRectPos;
    [SerializeField] Vector2 camRectSize;
    [SerializeField] float graphScale;
    [SerializeField] float lineWidth;   // TODO this might have to be scaled with the screen size...

    [Header("TODO REMOVE")]             // TODO remove
    [SerializeField] float lightAngle;
    [SerializeField] float viewAngle;

    bool initialized = false;
    List<ShaderProperty> shaderProperties;
    Camera targetCam;
    Material blitMat;
    LightingScreen lightingScreen;

    public void Initialize (LightingScreen lightingScreen) {
        CreateCamera();
        CreateBlitMat();
        CollectShaderProperties();
        this.lightingScreen = lightingScreen;
        this.initialized = true;

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
            blitMat.SetFloat("_LineWidth", lineWidth);
            blitMat.SetVector("_LightDir", new Vector4(Mathf.Sin(lightAngle), Mathf.Cos(lightAngle), 0, 0));
            blitMat.SetVector("_ViewDir", new Vector4(Mathf.Sin(viewAngle), Mathf.Cos(viewAngle), 0, 0));
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
	
}
