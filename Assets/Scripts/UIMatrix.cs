using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIMatrix : MonoBehaviour {

    // TODO only calculate the matrix IF NEEDED!!!
    // maybe set a flag when it gets updated in some capacity and then if someone wants the actual matrix, recalculate it AND RESET THE FLAG

    [Header("Components")]
    [SerializeField] RectTransform m_rectTransform;
    [SerializeField] UIMatrixVariableContainer variableContainer;
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
    
    Matrix4x4 calculatedMatrix;
    bool calculatedMatrixUpToDate;      // TODO if ANY modification, set this to false!!!
    bool calculatedMatrixIsDisplayedMatrix;
    Button[] headerButtons;
    Image[] headerButtonImages;
    Button[] controlsButtons;
    Image[] controlsButtonImages;
    Color stringFieldInvalidColor;
    Button matrixInvertButton;

    bool m_editable;
    public bool editable {
        get {
            return m_editable;
        } set {
            foreach(var b in headerButtons){
                b.interactable = value;
            }
            foreach(var b in controlsButtons){
                if(b == matrixInvertButton){
                    b.interactable = value && IsInvertible;
                }else{
                    b.interactable = value;
                }
            }
            SetButtonImageColorAlpha(value);
            m_editable = value;
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

    void Awake () {
        if(!initialized){
            SelfInit();
        }
    }

    void Update () {
        if(Input.GetKeyDown(KeyCode.Keypad0)){
            this.editable = !this.editable;
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
        }
    }

    void SelfInit () {
        Initialize(new string[]{
            "2", "0", "0", "200", 
            "0", "1", "0", "-200",
            "0", "0", "1", "-30000",
            "asdf", "0", "0", "1"
        }, true);
        // Initialize(new string[]{
        //     "1", "0", "0", "0", 
        //     "0", "1", "0", "0",
        //     "0", "0", "1", "0",
        //     "1", "1", "1", "1"
        // }, true);
    }

    // NO COLOURS!!! that's all done in LoadColors!
    public void Initialize (string[] fieldInitializers, bool shouldBeEditable) {
        if(initialized){
            Debug.LogError($"Call to initialize although {nameof(UIMatrix)} is already initialized. Aborting.");
        }
        CreateUIFieldArray();
        CreateButtons();
        UpdateFieldStrings(fieldInitializers);
        UpdateMatrixAndGridView();
        outline.SetGOActive(false);
        VariableContainer.Initialize(true);

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
            matrixInvertButton = CreateButton(controlsArea, "Invert", TEMPBUTTONBACKGROUND, UISprites.GetSprite(UISprites.ID.MatrixInvert), false, 1, controlsButtons, controlsButtonImages, 4, Invert);
            CreateButton(controlsArea, "Transpose", TEMPBUTTONBACKGROUND, UISprites.GetSprite(UISprites.ID.MatrixTranspose), false, 2, controlsButtons, controlsButtonImages, 5, Transpose);

            Button CreateButton (RectTransform parent, string newButtonName, Sprite newButtonBackgroundImage, Sprite newButtonMainImage, bool leftBound, int displayIndex, Button[] targetButtonArray, Image[] targetImageArray, int arrayIndex, System.Action onClickAction) {
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

    public void SetName (string newName) {
        this.gameObject.name = newName;
        nameLabel.text = newName;
        nameLabelDropShadow.text = newName;
        SetNameLabelColorBasedOnNameHash(ColorScheme.current);
    }

    void SetStringFieldValuesFromMatrix (Matrix4x4 fromMatrix) {
        for(int i=0; i<16; i++){
            stringFieldValues[i] = fromMatrix[i].ToString();
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
        SetStringFieldValuesFromMatrix(Matrix4x4.identity);
        UpdateMatrixAndGridView();
    }

    public void Invert () {
        if(!IsInvertible){
            Debug.LogWarning("TODO put a debug message into the message thingy");   // TODO put a debug message into the message thingy
            return;                                                                 // or simply disable the invert button everytime the matrix is updated and non invertible... (sounds better and simpler)
        }
        var inv = calculatedMatrix.inverse;
        VariableContainer.RemoveAllVariables(false);
        SetStringFieldValuesFromMatrix(inv);
        UpdateMatrixAndGridView();
    }

    void SetNameLabelColorBasedOnNameHash (ColorScheme cs) {
        int nameHash = System.Math.Abs(this.nameLabel.text.GetHashCode());
        nameLabelBackground.color = cs.UiMatrixHeaders[nameHash % cs.UiMatrixHeaders.Length];
    }

    void LoadColorsAndUpdateEverything (ColorScheme cs) {
        background.color = cs.UiMatrixBackground;
        outline.color = cs.UiMatrixOutline;
        nameLabel.color = cs.UiMatrixLabel;
        nameLabelDropShadow.color = cs.UiMatrixLabelDropShadow;
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
        SetButtonImageColorAlpha(this.editable);
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
        if(!initialized){
            SelfInit();
        }
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
                var parsed = StringExpressions.ParseExpression(stringFieldValues[i], variableContainer.GetVariableMap());  // TODO variables
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
                return $"<color=#{hex}>{actualStringValue}</color>";                    // TODO it would be nice if i wouldn't have to call update colors together with all that
            }
        }
        if(matrixValid){
            calculatedMatrix = newMatrix;
            calculatedMatrixIsDisplayedMatrix = true;
            matrixInvertButton.interactable = editable;
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
