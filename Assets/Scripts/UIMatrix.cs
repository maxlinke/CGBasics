using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIMatrix : MonoBehaviour {

    public enum Editability {
        FULL,
        VARIABLE_VALUES_ONLY,
        NONE
    }

    [SerializeField] bool shouldSelfInit;

    [Header("Components")]
    [SerializeField] RectTransform m_rectTransform;
    [SerializeField] UIMatrixVariableContainer variableContainer;
    [SerializeField] Image background;
    [SerializeField] Image outline;                 // what do i even need this for?
    [SerializeField] Image nameLabelBackground;
    [SerializeField] Image controlsBackground;
    [SerializeField] TextMeshProUGUI nameLabel;
    [SerializeField] TextMeshProUGUI nameLabelDropShadow;
    [SerializeField] TMP_InputField nameLabelInputField;
    [SerializeField] RectTransform headerArea;
    [SerializeField] RectTransform controlsArea;
    [SerializeField] RectTransform matrixArea;

    [Header("Settings")]
    [SerializeField, Tooltip("Top, Right, Bottom, Right")] Vector4 fieldArrayMargins;
    [SerializeField] float spaceBetweenMatrixFields;
    [SerializeField] int fieldFontSize;
    [SerializeField] TMP_FontAsset fieldFont;
    [SerializeField] Sprite fieldBackgroundTexture;
    [SerializeField, Range(0, 2)] float buttonSize;
    [SerializeField] float buttonHorizontalOffset;
    [SerializeField] Sprite TEMPBUTTONBACKGROUND;
    [SerializeField] Sprite TEMPBUTTONMAINIMAGE;

    bool initialized = false;
    string[] stringFieldValues = new string[16];
    TextMeshProUGUI[] fieldTextMeshes = new TextMeshProUGUI[16];
    Button[] fieldButtons = new Button[16];
    
    Matrix4x4 calculatedMatrix;
    bool calculatedMatrixUpToDate;
    bool calculatedMatrixIsDisplayedMatrix;
    Button[] headerButtons;
    Image[] headerButtonImages;
    Button[] controlsButtons;
    Image[] controlsButtonImages;
    Color stringFieldInvalidColor;
    Button matrixInvertButton;

    Editability m_editability;
    public Editability editability {
        get {
            return m_editability;
        } set {
            bool buttonsInteractable = (value == Editability.FULL);
            foreach(var b in headerButtons){
                b.interactable = buttonsInteractable;
            }
            foreach(var b in controlsButtons){
                if(b == matrixInvertButton){
                    b.interactable = buttonsInteractable && IsInvertible;
                }else{
                    b.interactable = buttonsInteractable;
                }
            }
            SetButtonImageColorAlpha(buttonsInteractable);
            this.m_editability = value;
            VariableContainer.UpdateEditability();
        }
    }

    public Matrix4x4 MatrixValue {
        get {
            if(!calculatedMatrixUpToDate){
                UpdateMatrixAndGridView();
            }
            return calculatedMatrix;
        }
    }

    public bool IsInvertible => (MatrixValue.determinant != 0 && calculatedMatrixIsDisplayedMatrix);
    public UIMatrixVariableContainer VariableContainer => variableContainer;        // spoken to by the camera i guess.
    public RectTransform rectTransform => m_rectTransform;
    public float minHeight => headerArea.rect.height + controlsArea.rect.height + matrixArea.rect.height + variableContainer.minHeight;

    void Awake () {
        if(!initialized && shouldSelfInit){
            SelfInit();
        }
    }

    void Update () {
        if(Input.GetKeyDown(KeyCode.Keypad0)){
            var asInt = (int)(this.editability);
            var enumValues = System.Enum.GetValues(typeof(Editability));
            asInt = (asInt + 1) % enumValues.Length;
            this.editability = (Editability)asInt;
            var logMsg = $"Now: {this.editability} ({Time.frameCount})";
            Debug.Log(logMsg);
        }
        if(Input.GetKeyDown(KeyCode.Keypad1)){
            UpdateFieldStrings(new string[]{
                "2", "0", "0", "200", 
                "0", "1", "0", "-200",
                "0", "0", "1", "-30000",
                "asdf", "0", "0", "1"
            });
            UpdateMatrixAndGridView();
        }else if(Input.GetKeyDown(KeyCode.Keypad2)){
            UpdateFieldStrings(new string[]{
                "1", "0", "0", "0", 
                "0", "1", "0", "0",
                "0", "0", "1", "0",
                "1", "1", "1", "1"
            });
            UpdateMatrixAndGridView();
        }else if(Input.GetKeyDown(KeyCode.Keypad3)){
            VariableContainer.EditVariable("asdf", (2 * Random.value - 1) * 10, false);
        }else if(Input.GetKeyDown(KeyCode.Keypad4)){
            VariableContainer.EditVariable("asdf", (2 * Random.value - 1) * 10, true);
        }else if(Input.GetKeyDown(KeyCode.Keypad5)){
            VariableContainer.AddVariable("asdf", Mathf.PI);
        }else if(Input.GetKeyDown(KeyCode.Keypad7)){
            var logMsg = string.Empty;
            for(int i='a'; i<='z'; i++){
                logMsg += (char)i;
            }
            BottomLog.DisplayMessage($"{logMsg} ({Time.frameCount})");
        }else if(Input.GetKeyDown(KeyCode.Keypad8)){
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }else if(Input.GetKeyDown(KeyCode.Keypad9)){
            UpdateMatrixAndGridView();
        }
    }

    void SelfInit () {
        // Initialize(
        //     inputName: nameLabel.text,            
        //     fieldInitializers: new string[]{
        //         "2", "0", "0", "200", 
        //         "0", "1", "0", "-200",
        //         "0", "0", "1", "-30000",
        //         "asdf", "0", "0", "1"},
        //     initialVariables: null,
        //     initialEditability: Editability.FULL
        // );
        Initialize(UIMatrixConfig.translationConfig, Editability.FULL, true);
        // SetStringFieldValuesFromMatrix(GLMatrixCreator.GetTranslationMatrix(Vector3.one), true);
    }

    public void Initialize (UIMatrixConfig config, Editability initialEditability, bool varContainerExpanded) {
        Initialize(config.name, config.fieldStrings, config.defaultVariables, initialEditability, varContainerExpanded);
    }

    public void Initialize (string inputName, string[] fieldInitializers, IEnumerable<UIMatrixConfig.VarPreset> initialVariables, Editability initialEditability, bool varContainerExpanded) {
        if(initialized){
            Debug.LogError($"Call to initialize although {nameof(UIMatrix)} is already initialized. Aborting.");
        }
        CreateUIFieldArray();
        CreateButtons();
        VariableContainer.Initialize(initialVariables, varContainerExpanded);
        UpdateFieldStrings(fieldInitializers);
        SetName(inputName, false);
        // UpdateMatrixAndGridView();       // obsolete? since colors are now loaded at the end, which also updates everything?
        outline.SetGOActive(false);
        nameLabelInputField.SetGOActive(false);
        nameLabelInputField.onEndEdit.AddListener((enteredName) => {
            if(enteredName != null){
                enteredName = enteredName.Trim();
                if(enteredName.Length > 0){
                    SetName(enteredName);
                }
            }
            nameLabel.SetGOActive(true);
            nameLabelDropShadow.SetGOActive(true);
            nameLabelInputField.SetGOActive(false);
        });

        this.editability = initialEditability;
        initialized = true;
        LoadColorsAndUpdateEverything(ColorScheme.current);

        void CreateUIFieldArray () {
            // create "secondary parent" for easy margins
            var actualParent = new GameObject("Actual Matrix Parent", typeof(RectTransform)).GetComponent<RectTransform>();
            actualParent.SetParent(matrixArea, false);
            actualParent.SetToFillWithMargins(fieldArrayMargins);
            // fill it
            for(int i=0; i<16; i++){
                float x = i % 4;
                float y = i / 4;
                // generate container
                var newFieldRT = new GameObject($"Field {i} (x: {x}, y: {y})", typeof(RectTransform)).GetComponent<RectTransform>();
                newFieldRT.SetParent(actualParent, false);
                newFieldRT.anchoredPosition = Vector2.zero;
                newFieldRT.pivot = 0.5f * Vector2.one;
                newFieldRT.anchorMin = new Vector2(x / 4f, (3-y) / 4f);
                newFieldRT.anchorMax = new Vector2((x+1) / 4f, (3-y+1) / 4f);
                newFieldRT.sizeDelta = Vector2.zero;
                // generate background
                var newFieldBGRT = new GameObject("Field BG Image", typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<RectTransform>();
                newFieldBGRT.SetParent(newFieldRT, false);
                newFieldBGRT.SetToFillWithMargins(spaceBetweenMatrixFields);
                var newFieldBG = newFieldBGRT.GetComponent<Image>();
                newFieldBG.sprite = fieldBackgroundTexture;
                newFieldBG.type = Image.Type.Sliced;
                var newFieldBGButton = newFieldBGRT.GetComponent<Button>();
                int btnIndex = i;                                                                                                       // just using i is a trap!
                newFieldBGButton.onClick.AddListener(() => {Debug.Log($"{btnIndex} was clicked! Do something with that info!");});      // TODO proper onclick
                fieldButtons[i] = newFieldBGButton;
                // generate textfield
                var newTMPRT = new GameObject("TMP Textfield", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<RectTransform>();
                newTMPRT.SetParent(newFieldRT, false);
                newTMPRT.SetToFill();
                var newTMP = newTMPRT.GetComponent<TextMeshProUGUI>();
                newTMP.raycastTarget = false;
                newTMP.alignment = TextAlignmentOptions.Center;
                newTMP.font = fieldFont;
                newTMP.fontSize = fieldFontSize;
                newTMP.enableWordWrapping = false;
                newTMP.overflowMode = TextOverflowModes.Ellipsis;
                fieldTextMeshes[i] = newTMP;
            }
        }

        void CreateButtons () {
            headerButtons = new Button[2];
            headerButtonImages = new Image[2];
            int arrayIndex = 0;
            var buttonParentRT = headerArea;
            var buttonArray = headerButtons;
            var buttonImageArray = headerButtonImages;
            CreateButton("Left", UISprites.MatrixLeft, true, 0, null);                  // TODO these all call the vertex-menu to do things...
            CreateButton("Right", UISprites.MatrixRight, false, 0, null);
            controlsButtons = new Button[7];
            controlsButtonImages = new Image[7];
            arrayIndex = 0;
            buttonParentRT = controlsArea;
            buttonArray = controlsButtons;
            buttonImageArray = controlsButtonImages;
            CreateButton("Add/Duplicate", UISprites.MatrixAdd, true, 0, null);
            CreateButton("Rename", UISprites.MatrixRename, true, 1, RenameButtonPressed);
            CreateButton("Delete", UISprites.MatrixDelete, true, 2, null);
            CreateButton("Set Identity", UISprites.MatrixIdentity, false, 0, SetIdentity);
            matrixInvertButton = CreateButton("Invert", UISprites.MatrixInvert, false, 1, Invert);
            CreateButton("Transpose", UISprites.MatrixTranspose, false, 2, Transpose);
            CreateButton("Load Config", UISprites.MatrixConfig, false, 3, () => {UIMatrixConfigPicker.Open(LoadConfig);});

            Button CreateButton (string newButtonName, Sprite newButtonMainImage, bool leftBound, int displayIndex, System.Action onClickAction) {
                var newlyCreatedButtonRT = new GameObject(newButtonName, typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<RectTransform>();
                newlyCreatedButtonRT.SetParent(buttonParentRT, false);
                // the actual layout here
                float parentHeight = buttonParentRT.rect.height;
                float parentWidth = buttonParentRT.rect.width;
                newlyCreatedButtonRT.SetToPoint();
                newlyCreatedButtonRT.sizeDelta = Vector2.one * parentHeight;    // i'm just assuming that we want the buttons to fill the height...
                float xPos = (leftBound ? -1 : 1) * ((parentWidth / 2) - ((0.5f + displayIndex) * parentHeight) - (displayIndex * buttonHorizontalOffset));
                newlyCreatedButtonRT.anchoredPosition = new Vector2(xPos, 0);
                newlyCreatedButtonRT.localScale = Vector3.one * buttonSize;
                // the background image
                var newlyCreatedButtonBG = newlyCreatedButtonRT.gameObject.GetComponent<Image>();
                newlyCreatedButtonBG.sprite = TEMPBUTTONBACKGROUND;
                newlyCreatedButtonBG.type = Image.Type.Simple;
                // the button
                var newlyCreatedButton = newlyCreatedButtonRT.gameObject.GetComponent<Button>();
                newlyCreatedButton.targetGraphic = newlyCreatedButtonBG;
                newlyCreatedButton.onClick.AddListener(() => { onClickAction?.Invoke(); });
                // the foreground image
                var newlyCreatedButtonMainImageRT = new GameObject("Image", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                newlyCreatedButtonMainImageRT.SetParent(newlyCreatedButtonRT, false);
                newlyCreatedButtonMainImageRT.SetToFill();
                var newlyCreatedButtonMainImage = newlyCreatedButtonMainImageRT.gameObject.GetComponent<Image>();
                newlyCreatedButtonMainImage.raycastTarget = false;
                newlyCreatedButtonMainImage.sprite = newButtonMainImage;
                newlyCreatedButtonMainImage.type = Image.Type.Simple;
                // putting it into the arrays
                buttonArray[arrayIndex] = newlyCreatedButton;
                buttonImageArray[arrayIndex] = newlyCreatedButtonMainImage;
                arrayIndex++;
                return newlyCreatedButton;
            }            
        }
    }

    public void AutoResize () {
        float totalHeight = 0;
        totalHeight += headerArea.rect.height;
        totalHeight += controlsArea.rect.height;
        totalHeight += matrixArea.rect.height;
        totalHeight += VariableContainer.rectTransform.rect.height;
        rectTransform.SetSizeDeltaY(totalHeight);
    }

    void LoadConfig (UIMatrixConfig configToLoad) {
        if(configToLoad == null){
            Debug.Log("Didn't load any config because input was null! This might be intentional!");
            return;
        }
        variableContainer.RemoveAllVariables();
        variableContainer.LoadConfig(configToLoad.defaultVariables, false, variableContainer.expanded);
        UpdateFieldStrings(configToLoad.fieldStrings);
        SetName(configToLoad.name);
        UpdateMatrixAndGridView();
    }

    void RenameButtonPressed () {
        nameLabel.SetGOActive(false);
        nameLabelDropShadow.SetGOActive(false);
        nameLabelInputField.SetGOActive(true);
        nameLabelInputField.text = nameLabel.text;
        var es = UnityEngine.EventSystems.EventSystem.current;
        es.SetSelectedGameObject(nameLabelInputField.gameObject);
    }

    public void SetName (string newName, bool updateColors = true) {
        this.gameObject.name = newName;
        nameLabel.text = newName;
        nameLabelDropShadow.text = newName;
        if(updateColors){
            SetNameLabelColorBasedOnNameHash(ColorScheme.current);
        }
    }

    void SetStringFieldValuesFromMatrix (Matrix4x4 sourceMatrix, bool updateEverything = true) {
        var correctedSource = sourceMatrix.transpose;   // Matrix4x4 consists of column vectors, so this is necessary
        for(int i=0; i<16; i++){
            stringFieldValues[i] = correctedSource[i].ToString();
        }
        if(updateEverything){
            UpdateMatrixAndGridView();
        }
    }

    public void Transpose () {
        for(int y=0; y<3; y++){
            for(int x = y+1; x<4; x++){
                int src = 4 * y + x;
                int dst = 4 * x + y;
                var dstCache = stringFieldValues[dst];
                stringFieldValues[dst] = stringFieldValues[src];
                stringFieldValues[src] = dstCache;
            }
        }
        calculatedMatrixUpToDate = false;
        UpdateMatrixAndGridView();
    }

    public void SetIdentity () {
        VariableContainer.RemoveAllVariables(false);
        SetStringFieldValuesFromMatrix(Matrix4x4.identity, true);
    }

    public void Invert () {
        if(!IsInvertible){
            Debug.LogError("Call for inversion was received even though matrix is not invertible! This should NOT happen!");
            return;
        }
        if(!calculatedMatrixUpToDate){
            Debug.LogError("This case should never happen, so this SHOULD be pointless. But since you're reading this, what went wrong?");
            UpdateMatrixAndGridView();
        }
        var inv = calculatedMatrix.inverse;
        VariableContainer.RemoveAllVariables(false);
        SetStringFieldValuesFromMatrix(inv, true);
    }

    void SetNameLabelColorBasedOnNameHash (ColorScheme cs) {
        int nameHash = System.Math.Abs(this.nameLabel.text.GetHashCode());
        nameLabelBackground.color = cs.UiMatrixHeaders[nameHash % cs.UiMatrixHeaders.Length];
    }

    void LoadColorsAndUpdateEverything (ColorScheme cs) {
        if(!initialized){
            return;
        }
        background.color = cs.UiMatrixBackground;
        outline.color = cs.UiMatrixOutline;
        nameLabel.color = cs.UiMatrixLabel;
        nameLabelDropShadow.color = cs.UiMatrixLabelDropShadow;
        nameLabelInputField.textComponent.color = cs.UiMatrixLabel;
        nameLabelInputField.selectionColor = cs.UiMatrixNameLabelInputFieldSelection;
        SetNameLabelColorBasedOnNameHash(cs);
        controlsBackground.color = cs.UiMatrixControlsBackground;
        stringFieldInvalidColor = cs.UiMatrixFieldTextInvalid;
        for(int i=0; i<fieldButtons.Length; i++){
            fieldButtons[i].SetFadeTransition(0, cs.UiMatrixFieldBackground, cs.UiMatrixFieldBackgroundHighlighted, cs.UiMatrixFieldBackgroundClicked, cs.UiMatrixFieldBackgroundDisabled);
        }
        for(int i=0; i<fieldTextMeshes.Length; i++){
            fieldTextMeshes[i].color = cs.UiMatrixFieldText;
        }
        foreach(var btn in headerButtons){
            btn.SetFadeTransition(0, cs.UiMatrixHeaderButtonBackgroundDefault, cs.UiMatrixHeaderButtonBackgroundHover, cs.UiMatrixHeaderButtonBackgroundClick, cs.UiMatrixHeaderButtonBackgroundDisabled);
        }
        foreach(var img in headerButtonImages){
            img.color = cs.UiMatrixHeaderButtonElement;
        }
        foreach(var btn in controlsButtons){
            btn.SetFadeTransition(0, cs.UiMatrixControlsButtonBackgroundDefault, cs.UiMatrixControlsButtonBackgroundHover, cs.UiMatrixControlsButtonBackgroundClick, cs.UiMatrixControlsButtonBackgroundDisabled);
        }
        foreach(var img in controlsButtonImages){
            img.color = cs.UiMatrixControlsButtonElement;
        }
        SetButtonImageColorAlpha(this.editability == Editability.FULL);
        variableContainer.LoadColors(cs);
        UpdateMatrixAndGridView();
    }

    void SetButtonImageColorAlpha (bool editableValue) {
        foreach(var i in headerButtonImages){
            i.color = new Color(i.color.r, i.color.g, i.color.b, (editableValue ? 1f : 0.5f));
        }
        foreach(var i in controlsButtonImages){
            i.color = new Color(i.color.r, i.color.g, i.color.b, (editableValue ? 1f : 0.5f));
        }
    }

    void OnEnable () {
        LoadColorsAndUpdateEverything(ColorScheme.current);
        ColorScheme.onChange += LoadColorsAndUpdateEverything;
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColorsAndUpdateEverything;
    }

    public void UpdateFieldStrings (string[] fieldStrings) {
        if(fieldStrings == null){
            throw new System.ArgumentException("Field string array must not be null!");
        }
        if(fieldStrings.Length != 16){
            throw new System.ArgumentException("Field string array must have 16 values!");
        }
        fieldStrings.CopyTo(stringFieldValues, 0);       // copy because i don't want anyone (especially me) creating a mess by doing whatever with the original input array
    }

    public void UpdateMatrixAndGridView () {
        bool matrixValid = true;
        var newMatrix = Matrix4x4.identity;
        for(int i=0; i<16; i++){
            try{
                var parsed = StringExpressions.ParseExpression(stringFieldValues[i], variableContainer.GetVariableMap());
                if(float.IsNaN(parsed)){
                    matrixValid = false;
                    fieldTextMeshes[i].text = InvalidColors("NaN");
                }else if(float.IsPositiveInfinity(parsed)){
                    matrixValid = false;
                    fieldTextMeshes[i].text = InvalidColors("+Inf");
                }else if(float.IsNegativeInfinity(parsed)){
                    matrixValid = false;
                    fieldTextMeshes[i].text = InvalidColors("-Inf");
                }else{
                    newMatrix[i] = parsed;
                    var showVal = $"{parsed:F3}";
                    if(showVal.Contains(".")){
                        while(showVal[showVal.Length-1] == '0'){
                            showVal = showVal.Substring(0, showVal.Length - 1);
                        }
                        if(showVal[showVal.Length-1] == '.'){
                            showVal = showVal.Substring(0, showVal.Length - 1);
                        }
                    }
                    fieldTextMeshes[i].text = showVal;
                }
            }catch(System.Exception){
                matrixValid = false;
                fieldTextMeshes[i].text = InvalidColors("ERR");
            }

            string InvalidColors (string actualStringValue) {
                string hex = ColorUtility.ToHtmlStringRGBA(stringFieldInvalidColor);
                return $"<color=#{hex}>{actualStringValue}</color>";
            }
        }
        if(matrixValid){
            calculatedMatrix = newMatrix.transpose;     // Matrix4x4 consists of column vectors, so this is the correction
            calculatedMatrixIsDisplayedMatrix = true;
            matrixInvertButton.interactable = (this.editability == Editability.FULL);
        }else{
            calculatedMatrix = Matrix4x4.identity;
            calculatedMatrixIsDisplayedMatrix = false;
            matrixInvertButton.interactable = false;
        }
        calculatedMatrixUpToDate = true;
    }

    public void SetMatrixNotUpToDate () {
        calculatedMatrixUpToDate = false;
    }    

}
