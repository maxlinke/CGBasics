using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LightingModels;

public class LightingScreen : MonoBehaviour {

    private const string modelGroupName = "Model";
    private const string lightsGroupName = "Lights";
    private const string diffGroupName = "Diffuse Model";
    private const string specGroupName = "Specular Model";

    [Header("Prefabs")]
    [SerializeField] PropGroup propertyGroupPrefab;

    [Header("Components")]
    [SerializeField] Image[] borders;
    [SerializeField] ScrollRect scrollRect;

    [Header("Settings")]
    [SerializeField] float scrollRectElementVerticalMargin;
    [SerializeField] bool modelPropsAlwaysVisible;      // if the colors are to be part of the model group, then this should be on

    [Header("Lighting Models")]
    [SerializeField] LightingModel solidColorLM;
    [SerializeField] LightingModel[] lightingModels;

    [Header("Shader Properties")]
    [SerializeField] ShaderProperty[] modelProps;       // i can easily change whether i want the colors to be part of the model or the lighting models here
    [SerializeField] ShaderProperty[] diffuseProps;
    [SerializeField] ShaderProperty[] specularProps;

    bool initialized = false;
    MeshRenderer targetMR;
    MaterialPropertyBlock mpb;

    Dictionary<LightingModel, Material> diffuseModels;
    Dictionary<LightingModel, Material> specularModels;
    Dictionary<ShaderProperty, UIPropertyField> propertyFields;     // TODO one such dictionary for each group? how do i do this?

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

        PropGroup CreateNewPropGroup () {

            return null;
        }

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
            // mpb.SetColor(ShaderProps.diffuseColor.propID, diffColor);
            // mpb.SetColor(ShaderProps.specularColor.propID, specColor);
            // TODO the floats
        }
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

    // TODO this
    void UpdateSliderPropVisibiltyAndRebuildContent () {
        // foreach
    }
	
}
