using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LightingModels;

public class LightingScreen : CloseableScreen {

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
    [SerializeField] PropertyWindowOverlay windowOverlay;
    [SerializeField] IntensityGraphDrawer intensityGraphDrawer;

    [Header("Settings")]
    [SerializeField] float scrollRectElementVerticalMargin;
    [SerializeField] bool colorPropertiesAreModelProperties;
    [SerializeField] bool modelPropsAreAlwaysVisible;
    [SerializeField] float additionalSpaceAtTheTop;
    [SerializeField] float additionalSpaceAtTheBottom;
    [SerializeField] float groupLabelPrefixSize;
    [SerializeField] float groupLabelSuffixSize;
    [SerializeField] bool applyModelPresetColors;
    [SerializeField] float scrollYForHidingPropWindowHeader;

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
    ColorPropertyField diffuseColorPropertyField;
    ColorPropertyField specularColorPropertyField;
    Toggle diffuseInfoToggle;
    Toggle specularInfoToggle;

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
        bool diffOK = diffuseColorPropertyField != null;
        bool specOK = specularColorPropertyField != null;
        if(!diffOK || !specOK){
            Debug.LogError($"Either no shaderproperties for diffuse or specular color found! Diff OK: {diffOK}, Spec OK: {specOK}.");
        }
        renderViewController.Initialize(this);
        intensityGraphDrawer.Initialize(this);
        windowOverlay.Initialize(() => {LoadDefaultState(isInit: false);});
        LoadDefaultState(isInit: true);
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
                newMat.renderQueue = (int)(UnityEngine.Rendering.RenderQueue.Geometry);
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
                newMat.renderQueue = (int)(UnityEngine.Rendering.RenderQueue.Geometry) + 1;
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
                    var newField = modelPropertyGroup.AddColorProperty(prop, (c) => {colObj.value = c;});
                    SetColorPropAsDiffOrSpecPropIfApplicable(key, newField);
                }
            }
            modelPropertyGroup.AddHeaderButton(
                icon: UISprites.UIConfig, 
                onButtonClicked: () => {
                    ModelPicker.Open(
                        onModelPicked: LoadModel,
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
            lightsPropertyGroup.AddHeaderButton(UISprites.UIConfig, () => {Foldout.Create(foldoutSetups, null);}, "Load a lighting setup");
            lightsPropertyGroup.RebuildContent();
        }

        UIPropertyGroup CreateLightingModelGroup (Dictionary<LightingModel, Material> lmDictionary, System.Action<LightingModel> loadModelAction, LightingModel nullLM, string groupName, string configButtonHoverMessage, string infoToggleHoverMessage, ref Toggle infoToggle) {
            var newGroup = CreateNewPropGroup();
            newGroup.Initialize(groupName, false);
            List<Foldout.ButtonSetup> buttonSetups = new List<Foldout.ButtonSetup>();
            var nullSetup = new Foldout.ButtonSetup("None", "None", () => {loadModelAction(nullLM);}, true);
            buttonSetups.Add(nullSetup);
            foreach(var lm in lmDictionary.Keys){
                var lmCopy = lm;
                buttonSetups.Add(new Foldout.ButtonSetup(lm.name, lm.name, () => {loadModelAction(lmCopy);}, true));
            }
            newGroup.AddHeaderButton(UISprites.UIConfig, () => {Foldout.Create(buttonSetups, null, 1f);}, hoverMessage: configButtonHoverMessage);
            infoToggle = newGroup.AddHeaderToggle(UISprites.UIInfo, false, (b) => {newGroup.SetBottomImageShown(b); newGroup.SetBottomTextShown(b); newGroup.RebuildContent(); RebuildContent();}, infoToggleHoverMessage);
            return newGroup;
        }

        void SetColorPropAsDiffOrSpecPropIfApplicable (ShaderVariable shaderVar, UIPropertyField propField) {
            if(shaderVar.prop.specialIdentifier == ShaderProperty.SpecialIdentifier.DiffuseColor){
                if(diffuseColorPropertyField != null){
                    Debug.LogError("Duplicate diffuse color fields!");
                }
                diffuseColorPropertyField = (ColorPropertyField)propField;
            }
            if(shaderVar.prop.specialIdentifier == ShaderProperty.SpecialIdentifier.SpecularColor){
                if(specularColorPropertyField != null){
                    Debug.LogError("Duplicate specular color fields!");
                }
                specularColorPropertyField = (ColorPropertyField)propField;
            }
        }

        void AddPropertyFieldsToGroup (LightingModel.Type lmType, UIPropertyGroup propGroup) {
            if(!colorPropertiesAreModelProperties){
                foreach(var shaderVar in shaderColors.Keys){
                    if(shaderVar.lmType == lmType){
                        var colObj = shaderColors[shaderVar];
                        var newField = propGroup.AddColorProperty(shaderVar.prop, (c) => {colObj.value = c;});
                        SetColorPropAsDiffOrSpecPropIfApplicable(shaderVar, newField);
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
                    }else{
                        customStringFormat = (f) => { return $"{f:F2}".ShortenNumberString();};
                    }
                    float scrollMultiplier = absDelta / 2f;
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
                configButtonHoverMessage: "Load diffuse lighting model",
                infoToggleHoverMessage: "Show information",
                infoToggle: ref diffuseInfoToggle
            );
            AddPropertyFieldsToGroup(LightingModel.Type.Diffuse, diffusePropertyGroup);
        }

        void CreateSpecularGroup () {
            specularPropertyGroup = CreateLightingModelGroup(
                lmDictionary: specularMatMap,
                loadModelAction: LoadSpecularLightingModel,
                nullLM: nullSpecularLM,
                groupName: specGroupName,
                configButtonHoverMessage: "Load specular lighting model",
                infoToggleHoverMessage: "Show information",
                infoToggle: ref specularInfoToggle
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
        background.color = cs.ApplicationBackground;
        scrollRect.verticalScrollbar.GetComponent<Image>().color = cs.LightingScreenScrollbarBackground;
        scrollRect.verticalScrollbar.targetGraphic.color = Color.white;
        scrollRect.verticalScrollbar.SetFadeTransition(0f, cs.LightingScreenScrollbar, cs.LightingScreenScrollbarHover, cs.LightingScreenScrollbarClick, Color.magenta);
        foreach(var b in borders){
            b.color = cs.ScreenBorders;
        }
        foreach(var pg in allPropertyGroups){
            pg.LoadColors(cs);
        }
        renderViewController.LoadColors(cs);
        intensityGraphDrawer.LoadColors(cs);
        windowOverlay.LoadColors(cs);
    }

    void LoadDefaultState (bool isInit) {
        foreach(var propGroup in allPropertyGroups){
            foreach(var propField in propGroup){
                propField.ResetToDefault();
            }
        }
        LoadModel(new LoadedModel(defaultModel));
        LoadDiffuseLightingModel(defaultDiffuseModel);
        LoadSpecularLightingModel(defaultSpecularModel);
        LoadLightingSetup(defaultLightingSetup, true, !isInit);
        diffuseInfoToggle.isOn = false;
        specularInfoToggle.isOn = false;
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
        windowOverlay.headerShouldBeVisible = (scrollRect.content.anchoredPosition.y < scrollYForHidingPropWindowHeader);

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

    public Color GetMainLightColor () {
        if(renderViewController.lightCount > 0){
            return renderViewController.mainLightColor;
        }
        return Color.black;
    }

    public Vector3 GetCamViewDir () {
        return renderViewController.camDir;
    }

    public Vector3 GetMainLightDir () {
        if(renderViewController.lightCount > 0){
            return renderViewController.mainLightDir;
        }
        return Vector3.zero;
    }

    string CreateGroupName (string prefix, string suffix) {
        return $"<size={groupLabelPrefixSize}%>{prefix}: <size={groupLabelSuffixSize}%>{suffix}";
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
        // lighting props are independent from this
        foreach(var propField in modelPropertyGroup){
            propField.SetGOActive(modelPropsAreAlwaysVisible || validProperties.Contains(propField.initProperty));
        }
        foreach(var propField in diffusePropertyGroup){
            propField.SetGOActive(validProperties.Contains(propField.initProperty));
        }
        foreach(var propField in specularPropertyGroup){
            propField.SetGOActive(validProperties.Contains(propField.initProperty));
        }
        RebuildGroups();
        RebuildContent();
    }

    (Material diffMat, Material specMat) GetCurrentMaterials () {
        Material diffMat, specMat;
        if(currentDiffuseModel == null || !diffuseMatMap.TryGetValue(currentDiffuseModel, out diffMat)){
            diffMat = nullDiffuseMat;
        }
        if(currentSpecularModel == null || !specularMatMap.TryGetValue(currentSpecularModel, out specMat)){
            specMat = null;
        }
        return (diffMat: diffMat, specMat: specMat);
    }

    // void UpdateMaterialsOnRenderController () {
    //     Material diffMat, specMat;
    //     if(currentDiffuseModel == null || !diffuseMatMap.TryGetValue(currentDiffuseModel, out diffMat)){
    //         diffMat = nullDiffuseMat;
    //     }
    //     if(currentSpecularModel == null || !specularMatMap.TryGetValue(currentSpecularModel, out specMat)){
    //         specMat = null;
    //     }
    //     renderViewController.LoadMaterials(diffMat, specMat);
    // }

    void LoadModel (LoadedModel newModel) {
        if(newModel == null){
            return;
        }
        if(applyModelPresetColors){
            diffuseColorPropertyField.UpdateColor(newModel.color);
            specularColorPropertyField.UpdateColor(newModel.specularColor);
        }
        modelPropertyGroup.SetName(CreateGroupName(modelGroupName, newModel.name));
        modelPropertyGroup.SetBottomTextShown(true);
        modelPropertyGroup.UpdateBottomText(newModel.description);
        renderViewController.LoadMesh(newModel.mesh);
        RebuildContent();
    }

    void LoadLightingModel (LightingModel lm, LightingModel defaultLM, UIPropertyGroup propertyGroup, string groupName, ref LightingModel lmField) {
        if(lm == null){
            Debug.LogError("NULL!");
            return;
        }
        if(lm == defaultLM){
            propertyGroup.SetName(CreateGroupName(groupName, "None"));
            propertyGroup.UpdateBottomText("No lighting model selected", false);
            propertyGroup.forceHideBottomImage = true;
            propertyGroup.forceShowBottomText = true;
        }else{
            propertyGroup.SetName(CreateGroupName(groupName, lm.name));
            propertyGroup.UpdateBottomText(lm.description, false);
            propertyGroup.UpdateBottomImage(lm.equation);
            propertyGroup.forceHideBottomImage = (lm.equation == null);
            propertyGroup.forceShowBottomText = false;
        }
        lmField = lm;
        UpdatePropertyFieldActiveStatesAndRebuildEverything();
        // UpdateMaterialsOnRenderController();
        var newMats = GetCurrentMaterials();
        renderViewController.LoadMaterials(newMats.diffMat, newMats.specMat);
        intensityGraphDrawer.UpdateLightingModels(currentDiffuseModel, currentSpecularModel);
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
        lightsPropertyGroup.SetName(CreateGroupName(lightsGroupName, setup.name));
        lightsPropertyGroup.RebuildContent();
        lightsPropertyGroup.LoadColors(ColorScheme.current);
        RebuildContent();
    }

}
