using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LightingModels;

// TODO class "propertyGroup" or something like that
// has a header, a "description" (optional image?) and a list of properties i guess (action to setup) AND an optional "configure"-button (mode, diffuse, specular but NOT light)

public class LightingScreen : MonoBehaviour {

    private enum LM {
        None,           // diff: solid color, spec: null
        DiffLambert,
        DiffOrenNayer,
        DiffMinnaert,
        DiffWrap,
        SpecPhong,
        SpecBlinnPhong,
        SpecCookTorr,
        SpecWardIso,
        SpecWardAniso
    }

    private const string modelGroupName = "Model";
    private const string lightsGroupName = "Lights";
    private const string diffGroupName = "Diffuse Model";
    private const string specGroupName = "Specular Model";

    // class for lighting model? has name (for foldout) and material?
    // two dictionaries<LM, LightingModel> ? diff and spec...

    [Header("Components")]
    [SerializeField] Image[] borders;
    [SerializeField] ScrollRect scrollRect;
    

    [Header("Settings")]
    [SerializeField] float scrollRectElementVerticalMargin;

    [Header("Lighting Models")]
    [SerializeField] LightingModel solidColorLM;
    [SerializeField] LightingModel[] lightingModels;

    bool initialized = false;
    MeshRenderer targetMR;
    MaterialPropertyBlock mpb;

    Color diffColor;
    Color specColor;
    // also sliders/scrollableinputfields for intensity? automatically clamp value (in inputfield) to [0, Infinity]?
    Color ambientLightColor;
    Color mainLightColor;
    Color backLightColor;

    Dictionary<LightingModel, Material> diffuseModels;
    Dictionary<LightingModel, Material> specularModels;

    PropGroup modelGroup;
    PropGroup lightsGroup;
    PropGroup diffGroup;
    PropGroup specGroup;

    void Start () {

    }

    void Initialize () {
        if(initialized){
            Debug.LogError("Already initialized! Aborting...");
            return;
        }
        CreateMaterialsAndSetupDictionaries();

        this.initialized = true;

        void CreateMaterialsAndSetupDictionaries () {
            if(diffuseModels != null || specularModels != null){
                Debug.Log("Either one or both of the dictionaries is null, this should not happen!");
            }
            diffuseModels = new Dictionary<LightingModel, Material>();
            specularModels = new Dictionary<LightingModel, Material>();
            foreach(var lm in lightingModels){
                if(lm.type == LightingModel.Type.Diffuse){
                    CreateMatAndAddToDictionary(CreateDiffuseMaterial, diffuseModels);
                }else if(lm.type == LightingModel.Type.Specular){
                    CreateMatAndAddToDictionary(CreateSpecularMaterial, specularModels);
                }else{
                    Debug.LogError($"Unknown {nameof(LightingModel.Type)} \"{lm.type}\"!");
                }

                void CreateMatAndAddToDictionary (System.Func<Shader, Material> createMat, Dictionary<LightingModel, Material> dictionary) {
                    if(dictionary.ContainsKey(lm)){
                        Debug.LogError($"{nameof(LightingModel)} \"{lm.name}\" is already in dictionary! Aborting...");
                        return;
                    }
                    dictionary.Add(lm, createMat(lm.shader));
                }

                Material CreateDiffuseMaterial (Shader shader) {
                    var newMat = new Material(shader);
                    newMat.hideFlags = HideFlags.HideAndDontSave;
                    newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    newMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                    newMat.SetInt("_ZWrite", 1);
                    newMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
                    return newMat;
                }

                Material CreateSpecularMaterial (Shader shader) {
                    var newMat = new Material(shader);
                    newMat.hideFlags = HideFlags.HideAndDontSave;
                    newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    newMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                    newMat.SetInt("_ZWrite", 0);
                    newMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Equal);
                    return newMat;
                }
            }
        }
    }

    void RebuildContent () {
        float y = 0;
        int cCount = scrollRect.content.childCount;
        for(int i=0; i<cCount; i++){
            var child = (RectTransform)(scrollRect.content.GetChild(i));
            if(!child.gameObject.activeSelf){
                continue;
            }
            child.anchoredPosition = new Vector2(child.anchoredPosition.x, y);
            y -= (child.rect.height + ((i+1 < cCount) ? scrollRectElementVerticalMargin : 0));
        }
        scrollRect.content.SetSizeDeltaY(Mathf.Abs(y));
    }

    void Update () {
        
        SetupMaterialPropertyBlock();
        // targetMR.SetPropertyBlock(mpb);

        void SetupMaterialPropertyBlock () {
            if(mpb == null){
                mpb = new MaterialPropertyBlock();
            }
            mpb.SetColor(ShaderProps.diffuseColor.propID, diffColor);
            mpb.SetColor(ShaderProps.specularColor.propID, specColor);
            // TODO the floats
        }
    }

    // void CreateEmptyGroup (string name, System.Action onConfigButtonClicked, out TextMeshProUGUI 

    void CreateModelGroup () {

    }

    void CreateLightGroup () {

    }

    void CreateDiffuseGroup () {
        // enable config button (() => {...});
        // create slider ("_Roughness", 0, 1, (value) => {this.roughness = value;});
    }

    void CreateSpecularGroup () {

    }

    string CreateGroupName (string prefix, string suffix) {
        return $"{prefix}: {suffix}";
    }

    void LoadSpecModel (LightingModel specModel) {
        specGroup.SetName(CreateGroupName(modelGroupName, specModel.name));
        targetMR.materials[1] = specularModels[specModel];
        // show and hide the corresponing sliders n shit...
        specGroup.RebuildContent();
        RebuildContent();
    }


	
}
