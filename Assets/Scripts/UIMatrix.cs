using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIMatrix : MonoBehaviour {

    // TODO only calculate the matrix IF NEEDED!!!
    // maybe set a flag when it gets updated in some capacity and then if someone wants the actual matrix, recalculate it AND RESET THE FLAG

    // TODO UISprites scriptable object so i can do "UISprites.Get(UISprites.MOVELEFT)"

    [Header("Components")]
    [SerializeField] Image background;
    [SerializeField] Image outline;                 // what do i even need this for?
    [SerializeField] Image nameLabelBackground;
    [SerializeField] Image controlsBackground;
    [SerializeField] TextMeshProUGUI nameLabel;
    [SerializeField] TextMeshProUGUI nameLabelDropShadow;
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
    List<Variable> variables = new List<Variable>();
    
    Matrix4x4 calculatedMatrix;
    bool calculatedMatrixUpToDate;      // TODO if ANY modification, set this to false!!!
    Button[] headerButtons;
    Image[] headerButtonImages;
    Button[] controlsButtons;
    Image[] controlsButtonImages;

    bool m_editable;
    public bool editable {
        get {
            return m_editable;
        } set {
            foreach(var b in headerButtons){
                b.interactable = value;
            }
            foreach(var b in controlsButtons){
                b.interactable = value;
            }
            SetButtonImageColorAlpha(value);
            m_editable = value;
        }
    }

    public Matrix4x4 MatrixValue {
        get {
            if(!calculatedMatrixUpToDate){
                UpdateMatrixAndUI();
            }
            return calculatedMatrix;
        }
    }

    public bool IsInvertible => (MatrixValue.determinant != 0);

    void Awake () {
        if(!initialized){
            SelfInit();
        }
    }

    void SelfInit () {
        // Initialize(new string[]{
        //     "2", "0", "0", "200", 
        //     "0", "1", "0", "-200",
        //     "0", "0", "1", "-30000",
        //     "asdf", "0", "0", "1"
        // }, true);
        Initialize(new string[]{
            "1", "0", "0", "0", 
            "0", "1", "0", "0",
            "0", "0", "1", "0",
            "1", "1", "1", "1"
        }, true);
    }

    // NO COLOURS!!! that's all done in LoadColors!
    public void Initialize (string[] fieldInitializers, bool shouldBeEditable) {
        if(initialized){
            Debug.LogError($"Call to initialize although {nameof(UIMatrix)} is already initialized. Aborting.");
        }
        CreateUIFieldArray();
        CreateButtons();
        UpdateFieldStrings(fieldInitializers);
        UpdateMatrixAndUI();
        outline.SetGOActive(false);

        this.editable = shouldBeEditable;
        initialized = true;

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
            CreateButton(headerArea, "Left", TEMPBUTTONBACKGROUND, UISprites.GetSprite(UISprites.ID.MatrixLeft), true, 0, headerButtons, headerButtonImages, 0, null);                  // TODO these all call the vertex-menu to do things...
            CreateButton(headerArea, "Right", TEMPBUTTONBACKGROUND, UISprites.GetSprite(UISprites.ID.MatrixRight), false, 0, headerButtons, headerButtonImages, 1, null);
            controlsButtons = new Button[6];
            controlsButtonImages = new Image[6];
            CreateButton(controlsArea, "Add/Duplicate", TEMPBUTTONBACKGROUND, UISprites.GetSprite(UISprites.ID.MatrixAdd), true, 0, controlsButtons, controlsButtonImages, 0, null);
            CreateButton(controlsArea, "Rename", TEMPBUTTONBACKGROUND, UISprites.GetSprite(UISprites.ID.MatrixRename), true, 1, controlsButtons, controlsButtonImages, 1, null);        // except for this one. this one opens the rename thingy.
            CreateButton(controlsArea, "Delete", TEMPBUTTONBACKGROUND, UISprites.GetSprite(UISprites.ID.MatrixDelete), true, 2, controlsButtons, controlsButtonImages, 2, null);
            CreateButton(controlsArea, "Set Identity", TEMPBUTTONBACKGROUND, UISprites.GetSprite(UISprites.ID.MatrixIdentity), false, 0, controlsButtons, controlsButtonImages, 3, SetIdentity);
            CreateButton(controlsArea, "Invert", TEMPBUTTONBACKGROUND, UISprites.GetSprite(UISprites.ID.MatrixInvert), false, 1, controlsButtons, controlsButtonImages, 4, Invert);
            CreateButton(controlsArea, "Transpose", TEMPBUTTONBACKGROUND, UISprites.GetSprite(UISprites.ID.MatrixTranspose), false, 2, controlsButtons, controlsButtonImages, 5, Transpose);

            void CreateButton (RectTransform parent, string newButtonName, Sprite newButtonBackgroundImage, Sprite newButtonMainImage, bool leftBound, int displayIndex, Button[] targetButtonArray, Image[] targetImageArray, int arrayIndex, System.Action onClickAction) {
                var newlyCreatedButtonRT = new GameObject(newButtonName, typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<RectTransform>();
                newlyCreatedButtonRT.SetParent(parent, false);
                // the actual layout here
                float parentHeight = parent.rect.height;
                float parentWidth = parent.rect.width;
                newlyCreatedButtonRT.SetToPoint();
                newlyCreatedButtonRT.sizeDelta = Vector2.one * parentHeight;    // i'm just assuming that we want the buttons to fill the height...
                float xPos = (leftBound ? -1 : 1) * ((parentWidth / 2) - ((0.5f + displayIndex) * parentHeight) - (displayIndex * buttonHorizontalOffset));
                newlyCreatedButtonRT.anchoredPosition = new Vector2(xPos, 0);
                newlyCreatedButtonRT.localScale = Vector3.one * buttonSize;
                // the background image
                var newlyCreatedButtonBG = newlyCreatedButtonRT.gameObject.GetComponent<Image>();
                newlyCreatedButtonBG.sprite = newButtonBackgroundImage;
                newlyCreatedButtonBG.type = Image.Type.Simple;
                // the button
                var newlyCreatedButton = newlyCreatedButtonRT.gameObject.GetComponent<Button>();
                newlyCreatedButton.targetGraphic = newlyCreatedButtonBG;
                newlyCreatedButton.onClick.AddListener(() => { onClickAction?.Invoke(); });
                // the foreground image
                var newlyCreatedButtonMainImageRT = new GameObject($"{newButtonBackgroundImage} Image", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                newlyCreatedButtonMainImageRT.SetParent(newlyCreatedButtonRT, false);
                newlyCreatedButtonMainImageRT.SetToFill();
                var newlyCreatedButtonMainImage = newlyCreatedButtonMainImageRT.gameObject.GetComponent<Image>();
                newlyCreatedButtonMainImage.raycastTarget = false;
                newlyCreatedButtonMainImage.sprite = newButtonMainImage;
                newlyCreatedButtonMainImage.type = Image.Type.Simple;
                // putting it into the arrays
                targetButtonArray[arrayIndex] = newlyCreatedButton;
                targetImageArray[arrayIndex] = newlyCreatedButtonMainImage;
            }            
        }
    }

    public void SetName (string newName) {
        this.gameObject.name = newName;
        nameLabel.text = newName;
        nameLabelDropShadow.text = newName;
    }

    public void AddVariable (string varName, float varValue, bool updateEverything = true) {
        if(TryGetVariable(varName, out _)){
            Debug.LogWarning("TODO also do a user-warning!");   // TODO also do a user-warning!
            return;
        }
        variables.Add(new Variable(varName, varValue));
        if(updateEverything){
            UpdateMatrixAndUI();
        }
    }

    public void EditVariable (string oldName, string newName, float newValue, bool updateEverything = true) {
        if(TryGetVariable(oldName, out var foundVar)){
            foundVar.name = newName;
            foundVar.floatValue = newValue;
            if(updateEverything){
                UpdateMatrixAndUI();
            }
        }else{
            ThrowVarNotFoundException(oldName);
        }
    }

    public void RemoveVariable (string varName, bool updateEverything = true) {
        if(TryGetVariable(varName, out var foundVar)){
            variables.Remove(foundVar);
            if(updateEverything){
                UpdateMatrixAndUI();
            }
        }else{
            ThrowVarNotFoundException(varName);
        }
    }

    void ThrowVarNotFoundException (string varName) {
        throw new System.IndexOutOfRangeException($"Couldn't find variable \"{varName}\"!");
    }

    public bool TryGetVariable (string varName, out Variable outputVariable) {
        foreach(var variable in variables){
            if(variable.name.Equals(varName)){
                outputVariable = variable;
                return true;
            }
        }
        outputVariable = null;
        return false;
    }   

    public void RemoveAllVariables (bool updateEverything = true) {
        variables.Clear();
        if(updateEverything){
            UpdateMatrixAndUI();
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
        UpdateMatrixAndUI();
    }

    public void SetIdentity () {
        RemoveAllVariables(false);
        stringFieldValues = new string[]{
            "1", "0", "0", "0",
            "0", "1", "0", "0",
            "0", "0", "1", "0",
            "0", "0", "0", "1"
        };
        UpdateMatrixAndUI();
    }

    public void Invert () {
        if(!IsInvertible){
            Debug.LogWarning("TODO put a debug message into the message thingy");   // TODO put a debug message into the message thingy
            return;
        }
        var temp = calculatedMatrix.inverse;
        RemoveAllVariables(false);
        for(int i=0; i<16; i++){
            stringFieldValues[i] = temp[i].ToString();
        }
        UpdateMatrixAndUI();
    }

    void LoadColors (ColorScheme cs) {
        background.color = cs.UiMatrixBackground;
        outline.color = cs.UiMatrixOutline;
        nameLabel.color = cs.UiMatrixLabel;
        nameLabelDropShadow.color = cs.UiMatrixLabelDropShadow;
        int nameHash = System.Math.Abs(this.nameLabel.text.GetHashCode());
        nameLabelBackground.color = cs.UiMatrixHeaders[nameHash % cs.UiMatrixHeaders.Length];
        controlsBackground.color = cs.UiMatrixControlsBackground;
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
        SetButtonImageColorAlpha(this.editable);    
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
        if(!initialized){
            SelfInit();
        }
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColors;
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

    void UpdateMatrixAndUI () {
        bool matrixValid = true;
        var newMatrix = Matrix4x4.identity;
        for(int i=0; i<16; i++){
            try{
                var parsed = StringExpressions.ParseExpression(stringFieldValues[i], null);  // TODO variables
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
                    var showVal = $"{parsed:F2}";
                    if(showVal.Contains(".")){
                        while(showVal[showVal.Length-1] == '0'){
                            showVal = showVal.Substring(0, showVal.Length - 1);
                        }
                        showVal = showVal.Substring(0, showVal.Length - 1);
                    }
                    fieldTextMeshes[i].text = showVal;
                }
            }catch(System.Exception){
                matrixValid = false;
                fieldTextMeshes[i].text = InvalidColors("ERR");
            }

            string InvalidColors (string actualStringValue) {
                return $"<color=red>{actualStringValue}</color>";       // TODO from colorscheme (and some color to hex (mesmer?))
            }
        }
        if(matrixValid){
            calculatedMatrix = newMatrix;
        }else{
            calculatedMatrix = Matrix4x4.identity;
        }
        calculatedMatrixUpToDate = true;
    }

    public class Variable {

        public string name;
        public float floatValue;

        public Variable (string inputName, float inputValue) {
            if(inputName == null){
                throw new System.NullReferenceException("Name can't be null!");
            }
            if(inputName.Length == 0){
                throw new System.ArgumentException("Name can't be empty!");
            }
            var ch = inputName[0];
            bool validFirstChar = (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
            if(!validFirstChar){
                throw new System.ArgumentException("Name MUST start with a letter!");
            }
            this.name = inputName;
            this.floatValue = inputValue;
        }

    }

}
