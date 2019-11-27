using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIMatrix : MonoBehaviour {

    // TODO only calculate the matrix IF NEEDED!!!
    // maybe set a flag when it gets updated in some capacity and then if someone wants the actual matrix, recalculate it AND RESET THE FLAG

    [Header("Components")]
    [SerializeField] Image background;
    [SerializeField] Image outline;
    [SerializeField] Image nameLabelBackground;
    [SerializeField] TextMeshProUGUI nameLabel;
    [SerializeField] RectTransform fieldArrayParent;

    [Header("Settings")]
    [SerializeField, Tooltip("Top, Right, Bottom, Right")] Vector4 fieldArrayMargins;
    [SerializeField] float spaceBetweenMatrixFields;
    [SerializeField] int fieldFontSize;
    [SerializeField] TMP_FontAsset fieldFont;

    bool initialized = false;
    string[] stringFieldValues = new string[16];
    TextMeshProUGUI[] fieldTextMeshes = new TextMeshProUGUI[16];
    Image[] fieldBackgrounds = new Image[16];
    Matrix4x4 calculatedMatrix;
    bool calculatedMatrixUpToDate;      // TODO if ANY modification, set this to false!!!

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
            "2", "0", "0", "2", 
            "0", "1", "0", "0",
            "0", "0", "1", "0",
            "0", "0", "0", "1"
        });
    }

    public void Initialize (string[] fieldInitializers) {
        if(initialized){
            Debug.LogError($"Call to initialize although {nameof(UIMatrix)} is already initialized. Aborting.");
        }
        CreateUIFieldArray();
        UpdateFieldStrings(fieldInitializers);
        UpdateMatrixAndUI();

        initialized = true;

        void CreateUIFieldArray () {
            // create "secondary parent" for easy margins
            var actualParent = new GameObject("Actual Matrix Parent", typeof(RectTransform)).GetComponent<RectTransform>();
            actualParent.SetParent(fieldArrayParent, false);
            actualParent.SetToFillWithMargins(fieldArrayMargins);
            // fill it
            for(int i=0; i<16; i++){
                int intX = i % 4;
                int intY = i / 4;
                // generate container
                var newFieldRT = new GameObject($"Field {i} (x: {intX}, y: {intY})", typeof(RectTransform)).GetComponent<RectTransform>();
                newFieldRT.SetParent(actualParent, false);
                newFieldRT.anchoredPosition = Vector2.zero;
                newFieldRT.pivot = 0.5f * Vector2.one;
                newFieldRT.anchorMin = CalcAnchor(intX, 3 - intY);
                newFieldRT.anchorMax = CalcAnchor(intX + 1, 3 - intY + 1);
                newFieldRT.sizeDelta = Vector2.zero;
                // generate background
                var newFieldBGRT = new GameObject("Field BG Image", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                newFieldBGRT.SetParent(newFieldRT, false);
                newFieldBGRT.SetToFillWithMargins(spaceBetweenMatrixFields);
                // newFieldBGRT.SetToFill();
                // newFieldBGRT.sizeDelta = -fieldArrayMargins * Vector2.one;

                var newFieldBG = newFieldBGRT.GetComponent<Image>();
                fieldBackgrounds[i] = newFieldBG;
                // generate textfield
                var newTMPRT = new GameObject("TMP Textfield", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(ClickableTextMeshPro)).GetComponent<RectTransform>();
                newTMPRT.SetParent(newFieldRT, false);
                newTMPRT.SetToFill();
                var newTMP = newTMPRT.GetComponent<TextMeshProUGUI>();
                fieldTextMeshes[i] = newTMP;
                var TMPclick = newTMP.gameObject.GetComponent<ClickableTextMeshPro>();
                TMPclick.Initialize((ped) => { Debug.Log("INIT ME PROPERLY!!!"); });   // TODO proper init
                newTMP.alignment = TextAlignmentOptions.Center;
                newTMP.font = fieldFont;
                newTMP.fontSize = fieldFontSize;

                Vector2 CalcAnchor (int inputX, int inputY) {
                    return new Vector2((float)inputX / 4.0f , (float)inputY / 4.0f);
                }
            }
        }
    }

    public void SetName (string name) {
        this.gameObject.name = name;
        nameLabel.text = name;          // TODO if transposed...
    }

    void LoadColors (ColorScheme cs) {
        background.color = cs.UiMatrixBackground;
        outline.color = cs.UiMatrixOutline;
        nameLabel.color = cs.UiMatrixLabel;
        nameLabelBackground.color = cs.UiMatrixHeaders.Random();    // TODO index from name hash i guess?
        for(int i=0; i<fieldBackgrounds.Length; i++){
            // if(fieldBackgrounds[i] != null)
            fieldBackgrounds[i].color = cs.UiMatrixFieldBackground;
        }
        for(int i=0; i<fieldTextMeshes.Length; i++){
            // if(fieldTextMeshes[i] != null)
            fieldTextMeshes[i].color = cs.UiMatrixFieldText;
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

    private class ClickableTextMeshPro : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

        TextMeshProUGUI tm;
        System.Action<PointerEventData> onClick;
        // System.Action<PointerEventData> onPointerEnter;
        // System.Action<PointerEventData> onPointerExit;

        // public void Initialize (System.Action<PointerEventData> onClick, System.Action<PointerEventData> onPointerEnter, System.Action<PointerEventData> onPointerExit) {
        public void Initialize (System.Action<PointerEventData> onClick) {
            tm = GetComponent<TextMeshProUGUI>();
            if(tm == null){
                throw new System.NullReferenceException($"No {nameof(TextMeshProUGUI)} on {gameObject.name}!");
            }
            this.onClick = onClick;
            // this.onPointerEnter = onPointerEnter;
            // this.onPointerExit = onPointerExit;
        }

        public void OnPointerClick (PointerEventData eventData) {
            onClick(eventData);
        }

        public void OnPointerEnter (PointerEventData eventData) {
            var temp = tm.text;
            temp = $"<u>{temp}</u>";
            tm.text = temp;
        }

        public void OnPointerExit (PointerEventData eventData) {
            RemoveAllUnderlineTags();
        }

        void RemoveAllUnderlineTags () {
            var temp = tm.text.Replace("<u>", "");
            temp = temp.Replace("</u>", "");
            tm.text = temp;
        }
    }

}
