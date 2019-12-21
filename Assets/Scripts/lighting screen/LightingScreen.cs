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
    [SerializeField] UIPropertyGroup propertyGroupPrefab;

    [Header("Components")]
    [SerializeField] Image[] borders;
    [SerializeField] ScrollRect scrollRect;

    [Header("Settings")]
    [SerializeField] float scrollRectElementVerticalMargin;
    [SerializeField] bool colorShaderPropsAreModelProps;

    [Header("Lighting Models")]
    [SerializeField] LightingModel solidColorLM;
    [SerializeField] LightingModel[] lightingModels;

    bool initialized = false;
    MeshRenderer targetMR;
    MaterialPropertyBlock mpb;

    Dictionary<LightingModel, Material> diffuseModels;
    Dictionary<LightingModel, Material> specularModels;
    Dictionary<ShaderProperty, UIPropertyField> propertyFields;     // TODO one such dictionary for each group? how do i do this?    

    UIPropertyGroup modelGroup;
    UIPropertyGroup lightsGroup;
    UIPropertyGroup diffGroup;
    UIPropertyGroup specGroup;

    private class ShaderVariable {
        public readonly int id;
        public readonly string name;
        public ShaderVariable (string inputName) {
            this.name = inputName;
            this.id = Shader.PropertyToID(inputName);
        }
    }

    private class FloatObject {
        public float value;
        public FloatObject (float inputValue) {
            this.value = inputValue;
        }
    }

    private class ColorObject {
        public Color value;
        public ColorObject (Color inputColor) {
            this.value = inputColor;
        }
    }

    Dictionary<ShaderVariable, FloatObject> shaderFloats;
    Dictionary<ShaderVariable, ColorObject> shaderColors;

    void Start () {

    }

    void Initialize () {
        if(initialized){
            Debug.LogError("Already initialized! Aborting...");
            return;
        }
        CreateMaterialsAndSetupMaterialDictionaries();
        SetupShaderVariableDictionaries();

        this.initialized = true;

        void CreateMaterialsAndSetupMaterialDictionaries () {
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

        void SetupShaderVariableDictionaries () {
            if(shaderFloats != null || shaderColors != null){
                Debug.LogError("wat");
                return;
            }
            shaderFloats = new Dictionary<ShaderVariable, FloatObject>();
            shaderColors = new Dictionary<ShaderVariable, ColorObject>();
            foreach(var lm in lightingModels){
                foreach(var prop in lm){
                    AddIfNotDuplicate(prop);
                }
            }

            void AddIfNotDuplicate (ShaderProperty prop) {
                var propName = prop.name;
                if(!CheckForDuplicateName(propName)){
                    var shaderVar = new ShaderVariable(propName);
                    switch(prop.type){
                        case ShaderProperty.Type.Float:
                            shaderFloats.Add(shaderVar, new FloatObject(prop.defaultValue));
                            break;
                        case ShaderProperty.Type.Color:
                            shaderColors.Add(shaderVar, new ColorObject(prop.defaultColor));
                            break;
                        default:
                            Debug.Log($"Unknown type \"{prop.type}\"!");
                            break;
                    }
                }
            }

            bool CheckForDuplicateName (string inputName) {
                foreach(var key in shaderFloats.Keys){
                    if(key.name == inputName){
                        return true;
                    }
                }
                foreach(var key in shaderColors.Keys){
                    if(key.name == inputName){
                        return true;
                    }
                }
                return false;
            }
        }

        UIPropertyGroup CreateNewPropGroup () {

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
        if(targetMR != null){
            targetMR.SetPropertyBlock(mpb);
        }

        void SetupMaterialPropertyBlock () {
            if(mpb == null){
                mpb = new MaterialPropertyBlock();
            }
            foreach(var key in shaderColors.Keys){
                mpb.SetColor(key.id, shaderColors[key].value);
            }
            foreach(var key in shaderFloats.Keys){
                mpb.SetFloat(key.id, shaderFloats[key].value);
            }
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
