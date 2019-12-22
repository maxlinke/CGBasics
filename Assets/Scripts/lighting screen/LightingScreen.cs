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
    [SerializeField] Image background;
    [SerializeField] Image[] borders;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] UIRenderViewController renderViewController;

    [Header("Settings")]
    [SerializeField] float scrollRectElementVerticalMargin;
    [SerializeField] bool colorPropertiesAreModelProperties;
    [SerializeField] bool modelPropsAreAlwaysVisible;
    [SerializeField] float additionalSpaceAtTheTop;
    [SerializeField] float additionalSpaceAtTheBottom;

    [Header("Lighting Models")]
    [SerializeField] LightingModel nullDiffuseLM;
    [SerializeField] LightingModel nullSpecularLM;
    [SerializeField] LightingModel[] lightingModels;

    [Header("Lighting Setups")]
    [SerializeField] LightingSetup[] lightingSetups;
    [SerializeField] bool applyLightingSetupRotation;

    [Header("Defaults")]
    [SerializeField] ModelPreset defaultModel;
    [SerializeField] LightingModel defaultDiffuseModel;
    [SerializeField] LightingModel defaultSpecularModel;
    [SerializeField] LightingSetup defaultLightingSetup;

    bool initialized = false;
    float lastScrollContentWidth = -1f;
    bool mpbUpToDate = false;
    MaterialPropertyBlock mpb;

    LightingModel currentDiffuseModel;
    LightingModel currentSpecularModel;

    Material nullDiffuseMat;
    Material nullSpecularMat;
    Dictionary<LightingModel, Material> diffuseMatMap;
    Dictionary<LightingModel, Material> specularMatMap;

    UIPropertyGroup modelPropertyGroup;
    UIPropertyGroup lightsPropertyGroup;
    UIPropertyGroup diffusePropertyGroup;
    UIPropertyGroup specularPropertyGroup;
    List<UIPropertyGroup> allPropertyGroups;

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

    void Start () {
        Initialize();
    }

    void Initialize () {
        if(initialized){
            Debug.LogError("Already initialized! Aborting...");
            return;
        }
        CreateMaterialsAndSetupMaterialDictionaries();
        SetupShaderVariableDictionaries();
        allPropertyGroups = new List<UIPropertyGroup>();
        CreateModelGroup();
        CreateLightGroup();
        CreateDiffuseGroup();
        CreateSpecularGroup();
        renderViewController.Initialize(this);
        LoadModel(defaultModel.mesh, defaultModel.name);
        LoadDiffuseLightingModel(defaultDiffuseModel);
        LoadSpecularLightingModel(defaultSpecularModel);
        LoadLightingSetup(defaultLightingSetup, true, false);
        this.initialized = true;
        LoadColors(ColorScheme.current);

        void CreateMaterialsAndSetupMaterialDictionaries () {
            if(diffuseMatMap != null || specularMatMap != null){
                Debug.Log("Either one or both of the dictionaries isn't null, this should not happen!");
            }
            nullDiffuseMat = CreateDiffuseMaterial(nullDiffuseLM.shader);
            nullSpecularMat = CreateSpecularMaterial(nullSpecularLM.shader);
            diffuseMatMap = new Dictionary<LightingModel, Material>();
            specularMatMap = new Dictionary<LightingModel, Material>();
            
            foreach(var lm in lightingModels){
                if(lm.type == LightingModel.Type.Diffuse){
                    CreateMatAndAddToDictionary(CreateDiffuseMaterial, diffuseMatMap);
                }else if(lm.type == LightingModel.Type.Specular){
                    CreateMatAndAddToDictionary(CreateSpecularMaterial, specularMatMap);
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
            allPropertyGroups.Add(newGroup);
            return newGroup;
        }

        void CreateModelGroup () {
            modelPropertyGroup = CreateNewPropGroup();
            modelPropertyGroup.Initialize(modelGroupName, false);
            if(colorPropertiesAreModelProperties){
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
            var foldoutSetups = new List<Foldout.ButtonSetup>();
            foreach(var setup in lightingSetups){
                var setupCopy = setup;
                foldoutSetups.Add(new Foldout.ButtonSetup(setup.name, setup.name, () => {LoadLightingSetup(setupCopy);}, true));
            }
            lightsPropertyGroup.AddConfigButton(UISprites.UIConfig, () => {Foldout.Create(foldoutSetups, null);}, "Load a lighting setup");
            lightsPropertyGroup.RebuildContent();
        }

        UIPropertyGroup CreateLightingModelGroup (Dictionary<LightingModel, Material> lmDictionary, System.Action<LightingModel> loadModelAction, LightingModel nullLM, string groupName, string configButtonHoverMessage) {
            var newGroup = CreateNewPropGroup();
            newGroup.Initialize(groupName, false);
            List<Foldout.ButtonSetup> buttonSetups = new List<Foldout.ButtonSetup>();
            var nullSetup = new Foldout.ButtonSetup("None", "None", () => {loadModelAction(nullLM);}, true);
            buttonSetups.Add(nullSetup);
            foreach(var lm in lmDictionary.Keys){
                var lmCopy = lm;
                buttonSetups.Add(new Foldout.ButtonSetup(lm.name, lm.name, () => {loadModelAction(lmCopy);}, true));
            }
            newGroup.AddConfigButton( UISprites.UIConfig, () => {Foldout.Create(buttonSetups, null, 1f);}, hoverMessage: configButtonHoverMessage);
            return newGroup;
        }

        void AddPropertyFieldsToGroup (LightingModel.Type lmType, UIPropertyGroup propGroup) {
            if(!colorPropertiesAreModelProperties){
                foreach(var shaderVar in shaderColors.Keys){
                    if(shaderVar.lmType == lmType){
                        var colObj = shaderColors[shaderVar];
                        propGroup.AddColorProperty(shaderVar.prop, (c) => {colObj.value = c;});
                    }
                }
            }
            foreach(var shaderVar in shaderFloats.Keys){
                if(shaderVar.lmType == lmType){
                    var floatObj = shaderFloats[shaderVar];
                    System.Func<float, string> customStringFormat;
                    float absDelta = Mathf.Abs(shaderVar.prop.minValue - shaderVar.prop.maxValue);
                    if(absDelta <= 1f){
                        customStringFormat = (f) => { return $"{f:F3}".ShortenNumberString();};
                    }else if(absDelta <= 10f){
                        customStringFormat = (f) => { return $"{f:F2}".ShortenNumberString();};
                    }else if(absDelta <= 100f){
                        customStringFormat = (f) => { return $"{f:F1}".ShortenNumberString();};
                    }else{
                        customStringFormat = (f) => { return $"{f:F0}";};
                    }
                    float scrollMultiplier = absDelta;
                    propGroup.AddFloatProperty(shaderVar.prop, (f) => {floatObj.value = f;}, customStringFormat, scrollMultiplier);
                }
            }
        }

        void CreateDiffuseGroup () {
            diffusePropertyGroup = CreateLightingModelGroup(
                lmDictionary: diffuseMatMap,
                loadModelAction: LoadDiffuseLightingModel,
                nullLM: nullDiffuseLM,
                groupName: diffGroupName,
                configButtonHoverMessage: "Load diffuse lighting model"
            );
            AddPropertyFieldsToGroup(LightingModel.Type.Diffuse, diffusePropertyGroup);
        }

        void CreateSpecularGroup () {
            specularPropertyGroup = CreateLightingModelGroup(
                lmDictionary: specularMatMap,
                loadModelAction: LoadSpecularLightingModel,
                nullLM: nullSpecularLM,
                groupName: specGroupName,
                configButtonHoverMessage: "Load specular lighting model"
            );
            AddPropertyFieldsToGroup(LightingModel.Type.Specular, specularPropertyGroup);
        }
    }

    void OnEnable () {
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColors;
    }

    void LoadColors (ColorScheme cs) {
        if(!initialized){
            return;
        }
        background.color = cs.LightingScreenBackground;
        scrollRect.verticalScrollbar.GetComponent<Image>().color = cs.LightingScreenScrollbarBackground;
        scrollRect.verticalScrollbar.targetGraphic.color = Color.white;
        scrollRect.verticalScrollbar.SetFadeTransition(0f, cs.LightingScreenScrollbar, cs.LightingScreenScrollbarHover, cs.LightingScreenScrollbarClick, Color.magenta);
        foreach(var b in borders){
            b.color = cs.LightingScreenBorders;
        }
        foreach(var pg in allPropertyGroups){
            pg.LoadColors(cs);
        }
        renderViewController.LoadColors(cs);
    }

    void RebuildGroups () {
        foreach(var pg in allPropertyGroups){
            pg.RebuildContent();
        }
    }

    void RebuildContent () {
        float y = -Mathf.Max(0, additionalSpaceAtTheTop);
        int cCount = scrollRect.content.childCount;
        for(int i=0; i<cCount; i++){
            var child = (RectTransform)(scrollRect.content.GetChild(i));
            if(!child.gameObject.activeSelf){
                continue;
            }
            child.anchoredPosition = new Vector2(child.anchoredPosition.x, y);
            y -= (child.rect.height + ((i+1 < cCount) ? scrollRectElementVerticalMargin : 0));
        }
        y -= additionalSpaceAtTheBottom;
        scrollRect.content.SetSizeDeltaY(Mathf.Abs(y));
    }

    void Update () {
        if(!initialized){
            return;
        }
        mpbUpToDate = false;
        CheckForWidthChangeAndRebuildIfNeccessary();

        void CheckForWidthChangeAndRebuildIfNeccessary () {
            float currentScrollContentWidth = scrollRect.content.rect.width;
            if(currentScrollContentWidth != lastScrollContentWidth){
                RebuildGroups();
                RebuildContent();
            }
            lastScrollContentWidth = currentScrollContentWidth;
        }
    }

    public MaterialPropertyBlock GetMaterialPropertyBlock () {
        if(mpb == null){
            mpb = new MaterialPropertyBlock();
        }
        if(initialized && !mpbUpToDate){
            foreach(var key in shaderColors.Keys){
                mpb.SetColor(key.id, shaderColors[key].value);
            }
            foreach(var key in shaderFloats.Keys){
                mpb.SetFloat(key.id, shaderFloats[key].value);
            }
            mpbUpToDate = true;
        }
        return mpb;
    }

    string CreateGroupName (string prefix, string suffix) {
        return $"{prefix}: {suffix}";
    }

    void UpdatePropertyFieldActiveStatesAndRebuildEverything () {
        List<ShaderProperty> validProperties = new List<ShaderProperty>();
        if(currentDiffuseModel != null){
            foreach(var prop in currentDiffuseModel){
                validProperties.Add(prop);
            }
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
        RebuildGroups();
        RebuildContent();
    }

    void UpdateMaterialsOnRenderController () {
        Material diffMat, specMat;
        if(currentDiffuseModel == null || !diffuseMatMap.TryGetValue(currentDiffuseModel, out diffMat)){
            diffMat = nullDiffuseMat;
        }
        if(currentSpecularModel == null || !specularMatMap.TryGetValue(currentSpecularModel, out specMat)){
            specMat = null;
        }
        renderViewController.LoadMaterials(diffMat, specMat);
    }

    void LoadModel (Mesh newModel, string newModelName) {
        if(newModel == null){
            return;
        }
        // could do the color here...
        modelPropertyGroup.SetName(CreateGroupName(modelGroupName, newModelName));
        renderViewController.LoadMesh(newModel);
    }

    void LoadLightingModel (LightingModel lm, LightingModel defaultLM, UIPropertyGroup propertyGroup, string groupName, ref LightingModel lmField) {
        if(lm == null){
            Debug.LogError("NULL!");
            return;
        }
        if(lm == defaultLM){
            propertyGroup.SetName(CreateGroupName(groupName, "None"));
            propertyGroup.ShowText("No lighting model selected", false);
        }else{
            propertyGroup.SetName(CreateGroupName(groupName, lm.name));
            propertyGroup.ShowText(lm.description, false);
        }
        if(lm.equation != null){                            // TODO remove this check when i have equations for all...
            propertyGroup.ShowImage(lm.equation, false);    // move this into the non-default
        }else{
            propertyGroup.HideImage(false);                 // move this into the default
        }
        
        lmField = lm;
        UpdatePropertyFieldActiveStatesAndRebuildEverything();
        UpdateMaterialsOnRenderController();
    }

    void LoadDiffuseLightingModel (LightingModel lm) {
        LoadLightingModel(lm, nullDiffuseLM, diffusePropertyGroup, diffGroupName, ref currentDiffuseModel);
    }

    void LoadSpecularLightingModel (LightingModel lm) {
        LoadLightingModel(lm, nullSpecularLM, specularPropertyGroup, specGroupName, ref currentSpecularModel);
    }

    void LoadLightingSetup (LightingSetup setup, bool forceApplyRotation = false, bool callDestroy = true) {
        renderViewController.LoadLightingSetup(setup, applyLightingSetupRotation || forceApplyRotation);
        if(callDestroy){
            lightsPropertyGroup.DestroyPropFields(0, false);
        }
        lightsPropertyGroup.AddColorProperty("Ambient Light", setup.ambientColor, (c) => {
            RenderSettings.ambientLight = c;
        });
        int i=0;
        foreach(var l in setup){
            int iCopy = i;
            lightsPropertyGroup.AddColorProperty(l.name, l.color, (c) => {renderViewController.UpdateLightColor(iCopy, c);});
            i++;
        }
        lightsPropertyGroup.RebuildContent();
        lightsPropertyGroup.LoadColors(ColorScheme.current);
        RebuildContent();
    }

}
