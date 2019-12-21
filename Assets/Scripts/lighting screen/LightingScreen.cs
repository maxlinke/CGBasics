using System.Collections;
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
    [SerializeField] bool modelPropsAreAlwaysVisible;

    [Header("Lighting Models")]
    [SerializeField] LightingModel solidColorLM;
    [SerializeField] LightingModel[] lightingModels;

    [Header("Defaults")]
    [SerializeField] ModelPreset defaultModel;
    [SerializeField] LightingModel defaultDiffuseModel;
    [SerializeField] LightingModel defaultSpecularModel;

    bool initialized = false;
    bool verticalScrollbarWasActive;
    MeshRenderer targetMR;
    MaterialPropertyBlock mpb;

    LightingModel currentDiffuseModel;
    LightingModel currentSpecularModel;

    Dictionary<LightingModel, Material> diffuseModels;
    Dictionary<LightingModel, Material> specularModels;

    UIPropertyGroup modelPropertyGroup;
    UIPropertyGroup lightsPropertyGroup;
    UIPropertyGroup diffusePropertyGroup;
    UIPropertyGroup specularPropertyGroup;

    private class ShaderVariable {
        public readonly int id;
        public readonly string name;
        public readonly LightingModel.Type lmType;
        public readonly ShaderProperty prop;
        public ShaderVariable (string inputName, LightingModel.Type inputLmType, ShaderProperty actualProp) {
            this.name = inputName;
            this.id = Shader.PropertyToID(inputName);
            this.lmType = inputLmType;
            this.prop = actualProp;
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

    // THIS is why i need system.collections (not generic, that gives me lists etc)
    IEnumerator Start () {
        yield return null;  // because otherwise all the ui recttransforms won't be loaded yet (widths will be zero...)
        if(!initialized){
            Initialize();
        }
    }

    void Initialize () {
        if(initialized){
            Debug.LogError("Already initialized! Aborting...");
            return;
        }
        CreateMaterialsAndSetupMaterialDictionaries();
        SetupShaderVariableDictionaries();
        CreateModelGroup();
        CreateLightGroup();
        CreateDiffuseGroup();
        CreateSpecularGroup();
        LoadModel(defaultModel.mesh, defaultModel.name);
        LoadDiffuseLightingModel(defaultDiffuseModel);
        LoadSpecularLightingModel(defaultSpecularModel);

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
                    AddIfNotDuplicate(prop, lm.type);
                }
            }

            void AddIfNotDuplicate (ShaderProperty prop, LightingModel.Type lmType) {
                var propName = prop.name;
                if(!CheckForDuplicateName(propName)){
                    var shaderVar = new ShaderVariable(propName, lmType, prop);
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
            var newGroup = Instantiate(propertyGroupPrefab);
            newGroup.rectTransform.SetParent(scrollRect.content, false);
            newGroup.rectTransform.ResetLocalScale();
            return newGroup;
        }

        void CreateModelGroup () {
            modelPropertyGroup = CreateNewPropGroup();
            modelPropertyGroup.Initialize(modelGroupName, false);
            if(colorShaderPropsAreModelProps){
                foreach(var key in shaderColors.Keys){
                    var prop = key.prop;
                    var colObj = shaderColors[key];
                    modelPropertyGroup.AddColorProperty(prop, (c) => {colObj.value = c;});
                }
            }
            modelPropertyGroup.AddConfigButton(
                icon: UISprites.UIConfig, 
                onButtonClicked: () => {
                    ModelPicker.Open(
                        onMeshPicked: LoadModel,
                        scale: 1f
                    );
                }, hoverMessage: "Select a model"
            );
            modelPropertyGroup.RebuildContent();
        }

        void CreateLightGroup () {
            lightsPropertyGroup = CreateNewPropGroup();
            lightsPropertyGroup.Initialize(lightsGroupName, false);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            lightsPropertyGroup.AddColorProperty("Ambient Color", Color.grey, (c) => {
                RenderSettings.ambientLight = c;
            });
            lightsPropertyGroup.AddColorProperty("Light Color", Color.white, (c) => {
                // Main light color = c;
            });
            // the other lights...
            //TODO config button
            lightsPropertyGroup.RebuildContent();
        }

        UIPropertyGroup CreateLightingModelGroup (
            Dictionary<LightingModel, Material> lmDictionary, 
            System.Action<LightingModel> loadModelAction,
            Foldout.ButtonSetup nullSetup, 
            string groupName, 
            string configButtonHoverMessage
        ) {
            var newGroup = CreateNewPropGroup();
            newGroup.Initialize(groupName, false);
            List<Foldout.ButtonSetup> buttonSetups = new List<Foldout.ButtonSetup>();
            buttonSetups.Add(nullSetup);
            LightingModel firstLM = null;
            foreach(var lm in lmDictionary.Keys){
                if(firstLM == null){
                    firstLM = lm;
                }
                var lmCopy = lm;
                buttonSetups.Add(new Foldout.ButtonSetup(
                    buttonName: lm.name,
                    buttonHoverMessage: lm.name,
                    buttonClickAction: () => {
                        loadModelAction(lmCopy);
                    }, buttonInteractable: true
                ));
            }
            newGroup.AddConfigButton(
                icon: UISprites.UIConfig,
                onButtonClicked: () => {
                    Foldout.Create(
                        setups: buttonSetups, 
                        onNotSelectAnything: null, 
                        scale: 1f
                    );
                }, hoverMessage: configButtonHoverMessage
            );
            var lmType = firstLM.type;
            if(!colorShaderPropsAreModelProps){
                foreach(var shaderVar in shaderColors.Keys){
                    if(shaderVar.lmType == lmType){
                        var colObj = shaderColors[shaderVar];
                        newGroup.AddColorProperty(shaderVar.prop, (c) => {colObj.value = c;});
                    }
                }
            }
            foreach(var shaderVar in shaderFloats.Keys){
                if(shaderVar.lmType == lmType){
                    var floatObj = shaderFloats[shaderVar];
                    System.Func<float, string> customStringFormat;
                    float scrollMultiplier;
                    if(Mathf.Abs(shaderVar.prop.maxValue) <= 10){       // TODO this is some dodgy ass code...
                        customStringFormat = (f) => { return $"{f:F2}".ShortenNumberString();};
                        scrollMultiplier = 1f;
                    }else{
                        customStringFormat = (f) => { return $"{f:F1}".ShortenNumberString();};
                        scrollMultiplier = 10f;
                    }
                    newGroup.AddFloatProperty(shaderVar.prop, (f) => {floatObj.value = f;}, customStringFormat, scrollMultiplier);
                }
            }
            return newGroup;
        }

        void CreateDiffuseGroup () {
            var noDiffModelSetup = new Foldout.ButtonSetup(
                buttonName: "None", 
                buttonHoverMessage: "None", 
                buttonClickAction: () => {
                    LoadDiffuseLightingModel(solidColorLM);
                    diffusePropertyGroup.SetName(CreateGroupName(diffGroupName, "None"));
                }, buttonInteractable: true
            );
            diffusePropertyGroup = CreateLightingModelGroup(
                lmDictionary: diffuseModels,
                loadModelAction: LoadDiffuseLightingModel,
                nullSetup: noDiffModelSetup,
                groupName: diffGroupName,
                configButtonHoverMessage: "Load diffuse lighting model"
            );
        }

        void CreateSpecularGroup () {
            var noSpecModelSetup = new Foldout.ButtonSetup(
                buttonName: "None",
                buttonHoverMessage: "None",
                buttonClickAction: () => {LoadSpecularLightingModel(null);},
                buttonInteractable: true
            );
            specularPropertyGroup = CreateLightingModelGroup(
                lmDictionary: specularModels,
                loadModelAction: LoadSpecularLightingModel,
                nullSetup: noSpecModelSetup,
                groupName: specGroupName,
                configButtonHoverMessage: "Load specular lighting model"
            );
        }
    }

    void RebuildGroups () {
        modelPropertyGroup.RebuildContent();
        lightsPropertyGroup.RebuildContent();
        diffusePropertyGroup.RebuildContent();
        specularPropertyGroup.RebuildContent();
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
        if(!initialized){
            return;
        }
        SetupMaterialPropertyBlock();
        if(targetMR != null){
            targetMR.SetPropertyBlock(mpb);
        }
        bool verticalScrollbarIsActiveNow = scrollRect.verticalScrollbar.gameObject.activeSelf;
        if(verticalScrollbarWasActive != verticalScrollbarIsActiveNow){
            RebuildGroups();
            RebuildContent();
        }
        verticalScrollbarWasActive = verticalScrollbarIsActiveNow;

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

    void UpdatePropertyFieldActiveStatesAndRebuildContent () {
        List<ShaderProperty> validProperties = new List<ShaderProperty>();
        foreach(var prop in currentDiffuseModel){
            validProperties.Add(prop);
        }
        if(currentSpecularModel != null){
            foreach(var prop in currentSpecularModel){
                validProperties.Add(prop);
            }
        }
        foreach(var propField in modelPropertyGroup){
            propField.SetGOActive(modelPropsAreAlwaysVisible || validProperties.Contains(propField.initProperty));
        }
        // foreach(var propField in lightsGroup){   // do this in the actual lights loading thingy
        //     propField.SetGOActive(true);
        // }
        foreach(var propField in diffusePropertyGroup){
            propField.SetGOActive(validProperties.Contains(propField.initProperty));
        }
        foreach(var propField in specularPropertyGroup){
            propField.SetGOActive(validProperties.Contains(propField.initProperty));
        }
        modelPropertyGroup.RebuildContent();
        diffusePropertyGroup.RebuildContent();
        specularPropertyGroup.RebuildContent();
        RebuildContent();
    }

    void LoadModel (Mesh newModel, string newModelName) {
        // could do the color here...
        modelPropertyGroup.SetName(CreateGroupName(modelGroupName, newModelName));
    }

    void LoadDiffuseLightingModel (LightingModel lm) {
        if(lm == solidColorLM){
            diffusePropertyGroup.SetName(CreateGroupName(diffGroupName, "None"));
            diffusePropertyGroup.HideImage(false);
            diffusePropertyGroup.ShowText("No lighting model selected", false);
        }else{
            diffusePropertyGroup.SetName(CreateGroupName(diffGroupName, lm.name));   
            diffusePropertyGroup.ShowText(lm.description, false);
        }
        currentDiffuseModel = lm;
        UpdatePropertyFieldActiveStatesAndRebuildContent();
    }

    void LoadSpecularLightingModel (LightingModel lm) {
        if(lm == null){
            specularPropertyGroup.SetName(CreateGroupName(specGroupName, "None"));
            specularPropertyGroup.HideImage(false);
            specularPropertyGroup.ShowText("No lighting model selected", false);
        }else{
            specularPropertyGroup.SetName(CreateGroupName(specGroupName, lm.name));
            specularPropertyGroup.ShowText(lm.description, false);
        }
        currentSpecularModel = lm;
        UpdatePropertyFieldActiveStatesAndRebuildContent();
    }

}
