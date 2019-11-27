using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    Image[] fieldBackgrounds = new Image[16];
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
        Initialize(new string[]{
            "2", "0", "0", "200", 
            "0", "1", "0", "-200",
            "0", "0", "1", "0",
            "0", "0", "0", "1"
        }, true);
    }

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
                var newFieldBGRT = new GameObject("Field BG Image", typeof(RectTransform), typeof(Image), typeof(HoverAndClickable)).GetComponent<RectTransform>();
                newFieldBGRT.SetParent(newFieldRT, false);
                newFieldBGRT.SetToFillWithMargins(spaceBetweenMatrixFields);
                var newFieldBG = newFieldBGRT.GetComponent<Image>();
                newFieldBG.sprite = fieldBackgroundTexture;
                newFieldBG.type = Image.Type.Sliced;
                fieldBackgrounds[i] = newFieldBG;
                var clickable = newFieldBGRT.GetComponent<HoverAndClickable>();     // TODO just make it a button...
                clickable.Initialize(
                    onClick: (ped) => {Debug.Log("ASDF");},
                    onPointerEnter: (ped) => {newFieldBG.color = ColorScheme.current.UiMatrixFieldBackgroundHighlighted;},      // TODO does this work properly when the current colorscheme changes?
                    onPointerExit: (ped) => {newFieldBG.color = ColorScheme.current.UiMatrixFieldBackground;}
                );
                // generate textfield
                var newTMPRT = new GameObject("TMP Textfield", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<RectTransform>();
                newTMPRT.SetParent(newFieldRT, false);
                newTMPRT.SetToFill();
                var newTMP = newTMPRT.GetComponent<TextMeshProUGUI>();
                newTMP.raycastTarget = false;
                newTMP.alignment = TextAlignmentOptions.Center;
                newTMP.font = fieldFont;
                newTMP.fontSize = fieldFontSize;
                newTMP.overflowMode = TextOverflowModes.Ellipsis;
                fieldTextMeshes[i] = newTMP;
            }
        }

        // TODO assign actions
        void CreateButtons () {
            headerButtons = new Button[2];
            headerButtonImages = new Image[2];
            CreateButton(headerArea, "Left", TEMPBUTTONBACKGROUND, TEMPBUTTONMAINIMAGE, true, 0, headerButtons, headerButtonImages, 0);
            CreateButton(headerArea, "Right", TEMPBUTTONBACKGROUND, TEMPBUTTONMAINIMAGE, false, 0, headerButtons, headerButtonImages, 1);
            controlsButtons = new Button[6];
            controlsButtonImages = new Image[6];
            CreateButton(controlsArea, "Add/Duplicate", TEMPBUTTONBACKGROUND, TEMPBUTTONMAINIMAGE, true, 0, controlsButtons, controlsButtonImages, 0);
            CreateButton(controlsArea, "Rename", TEMPBUTTONBACKGROUND, TEMPBUTTONMAINIMAGE, true, 1, controlsButtons, controlsButtonImages, 1);
            CreateButton(controlsArea, "Delete", TEMPBUTTONBACKGROUND, TEMPBUTTONMAINIMAGE, true, 2, controlsButtons, controlsButtonImages, 2);
            CreateButton(controlsArea, "Set Identity", TEMPBUTTONBACKGROUND, TEMPBUTTONMAINIMAGE, false, 0, controlsButtons, controlsButtonImages, 3);
            CreateButton(controlsArea, "Invert", TEMPBUTTONBACKGROUND, TEMPBUTTONMAINIMAGE, false, 1, controlsButtons, controlsButtonImages, 4);
            CreateButton(controlsArea, "Transpose", TEMPBUTTONBACKGROUND, TEMPBUTTONMAINIMAGE, false, 2, controlsButtons, controlsButtonImages, 5);

            void CreateButton (RectTransform parent, string newButtonName, Sprite newButtonBackgroundImage, Sprite newButtonMainImage, bool leftBound, int displayIndex, Button[] targetButtonArray, Image[] targetImageArray, int arrayIndex) {
                var newlyCreatedButtonRT = new GameObject(newButtonName, typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<RectTransform>();
                newlyCreatedButtonRT.SetParent(parent, false);
                // the actual layout here
                float parentHeight = parent.rect.height;
                float parentWidth = parent.rect.width;
                newlyCreatedButtonRT.SetToPoint();
                newlyCreatedButtonRT.sizeDelta = Vector2.one * parentHeight;    // i'm just assuming that we want the buttons to fill the height...
                float xPos;
                if(leftBound){
                    xPos = -(parentWidth / 2) + ((0.5f + displayIndex) * parentHeight) + (displayIndex * buttonHorizontalOffset);
                }else{
                    xPos = (parentWidth / 2) - ((0.5f + displayIndex) * parentHeight) - (displayIndex * buttonHorizontalOffset);
                }
                newlyCreatedButtonRT.anchoredPosition = new Vector2(xPos, 0);
                newlyCreatedButtonRT.localScale = Vector3.one * buttonSize;
                // the background image
                var newlyCreatedButtonBG = newlyCreatedButtonRT.gameObject.GetComponent<Image>();
                newlyCreatedButtonBG.sprite = newButtonBackgroundImage;
                newlyCreatedButtonBG.type = Image.Type.Simple;
                // the button
                var newlyCreatedButton = newlyCreatedButtonRT.gameObject.GetComponent<Button>();
                newlyCreatedButton.targetGraphic = newlyCreatedButtonBG;
                // the foreground image
                var newlyCreatedButtonMainImageRT = new GameObject($"{newButtonBackgroundImage} Image", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                newlyCreatedButtonMainImageRT.SetParent(newlyCreatedButtonRT, false);
                newlyCreatedButtonMainImageRT.SetToFill();
                var newlyCreatedButtonMainImage = newlyCreatedButtonMainImageRT.gameObject.GetComponent<Image>();
                newlyCreatedButtonMainImage.raycastTarget = false;
                newlyCreatedButtonMainImage.sprite = newButtonMainImage;
                newlyCreatedButtonMainImage.type = Image.Type.Simple;
                
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

    void LoadColors (ColorScheme cs) {
        background.color = cs.UiMatrixBackground;
        outline.color = cs.UiMatrixOutline;
        nameLabel.color = cs.UiMatrixLabel;
        nameLabelDropShadow.color = cs.UiMatrixLabelDropShadow;
        nameLabelBackground.color = cs.UiMatrixHeaders.Random();    // TODO index from name hash i guess?
        controlsBackground.color = cs.UiMatrixControlsBackground;
        for(int i=0; i<fieldBackgrounds.Length; i++){
            fieldBackgrounds[i].color = cs.UiMatrixFieldBackground;
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
                    fieldTextMeshes[i].text = "NaN";
                }else if(float.IsPositiveInfinity(parsed)){
                    matrixValid = false;
                    fieldTextMeshes[i].text = "+Inf";
                }else if(float.IsNegativeInfinity(parsed)){
                    matrixValid = false;
                    fieldTextMeshes[i].text = "-Inf";
                }else{
                    newMatrix[i] = parsed;
                    if(fieldTextMeshes[i] != null) fieldTextMeshes[i].text = $"{parsed:F2}";   // TODO remove the nullcheck
                }
            }catch(System.Exception){
                matrixValid = false;
                fieldTextMeshes[i].text = "ERR";
            }
        }
        if(matrixValid){
            calculatedMatrix = newMatrix;
        }else{
            calculatedMatrix = Matrix4x4.identity;
        }
        calculatedMatrixUpToDate = true;
    }

}
