using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIVector : MonoBehaviour {

    [Header("Components")]
    [SerializeField] RectTransform m_rectTransform;
    [SerializeField] Image background;
    [SerializeField] Image outline;
    [SerializeField] GameObject inputFieldTemplate;

    [Header("Settings")]
    [SerializeField] Vector2 fieldSize;
    [SerializeField] float fieldSpacing;
    [SerializeField] Vector2 outsideMargin;

    bool initialized = false;
    bool m_editable;
    bool m_columnMode;
    TMP_InputField[] inputFields;
    RectTransform[] inputFieldRTs;
    float defaultFontSize;

    public RectTransform rectTransform => m_rectTransform;

    public bool editable {
        get {
            return m_editable;
        } set {
            m_editable = value;
            UpdateFieldEditability();
        }
    }

    public bool columnMode {
        get {
            return m_columnMode;
        } set {
            m_columnMode = value;
            ColumnModeUpdated();
        }
    }

    public Vector4 VectorValue {
        get {
            return CalculateOutputVector();
        } set {
            SetFieldsFromVector(value);
        }
    }

    void Initialize (Vector4 vectorValue, bool initEditability, bool initColumn) {
        if(initialized){
            Debug.LogError("Duplicate init call, aborting!", this.gameObject);
            return;
        }
        this.m_editable = initEditability;
        this.m_columnMode = initColumn;
        inputFields = new TMP_InputField[4];
        inputFieldRTs = new RectTransform[4];
        inputFieldTemplate.SetActive(false);
        SetupInputFields();
        ColumnModeUpdated();
        initialized = true;

        void SetupInputFields () {
            for(int i=0; i<4; i++){
                var newField = Instantiate(inputFieldTemplate).GetComponent<TMP_InputField>();
                newField.SetGOActive(true);
                defaultFontSize = newField.textComponent.fontSize;
                inputFields[i] = newField;
                var newFieldRT = newField.GetComponent<RectTransform>();
                newFieldRT.SetParent(this.m_rectTransform, false);
                newFieldRT.ResetLocalScale();
                newFieldRT.pivot = new Vector2(0, 1);
                newFieldRT.SetAnchor(newFieldRT.pivot);
                newFieldRT.sizeDelta = fieldSize;
                inputFieldRTs[i] = newFieldRT;
                newField.GetComponent<Image>().color = Color.white;
                newField.gameObject.AddComponent<ScrollableNumberInputField>().Initialize(newField);
                newField.gameObject.AddComponent<UIHoverEventCaller>().SetActions(
                    onHoverEnter: (ped) => {if(newField.interactable){BottomLog.DisplayMessage(ScrollableNumberInputField.hintText);}},
                    onHoverExit: (ped) => {if(newField.interactable){BottomLog.ClearDisplay();}}
                );
                newField.text = FormatFloatForField(vectorValue[i]);
                FormatFieldText(i);
                int indexCopy = i;
                newField.onEndEdit.AddListener((s) => {
                    FormatFieldText(indexCopy);
                });
                newField.interactable = this.editable;
            }
        }
    }

    void OnEnable () {
        if(!initialized){
            Initialize(new Vector4(-2, 0, 56, 0.3f), true, true);
        }
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
        outline.color = cs.UiMatrixOutline;
        background.color = cs.UiMatrixBackground;
        foreach(var field in inputFields){
            field.SetFadeTransition(0f, cs.UiMatrixFieldBackground, cs.UiMatrixFieldBackgroundHighlighted, cs.UiMatrixFieldBackgroundClicked, Color.clear);
            field.textComponent.color = cs.UiMatrixFieldText;
            field.selectionColor = cs.UiMatrixVariablesFieldSelection;
        }
    }

    void ColumnModeUpdated () {
        float x = outsideMargin.x;
        float y = -outsideMargin.y;
        for(int i=0; i<4; i++){
            inputFieldRTs[i].anchoredPosition = new Vector2(x, y);
            if(i+1<4){
                if(columnMode){
                    y -= (inputFieldRTs[i].rect.height + fieldSpacing);
                }else{
                    x += (inputFieldRTs[i].rect.width + fieldSpacing);
                }
            }else{
                if(columnMode){
                    x += inputFieldRTs[i].rect.width;
                    y -= inputFieldRTs[i].rect.height;
                }else{
                    x += inputFieldRTs[i].rect.width;
                    y -= inputFieldRTs[i].rect.height;
                }
            }
        }
        x += outsideMargin.x;
        y += -outsideMargin.y;
        rectTransform.SetSizeDelta(Mathf.Abs(x), Mathf.Abs(y));
    }

    void UpdateFieldEditability () {
        foreach(var field in inputFields){
            field.interactable = this.editable;
        }
    }

    Vector4 CalculateOutputVector () {
        var output = new Vector4();
        for(int i=0; i<4; i++){
            var text = inputFields[i].text;
            if(float.TryParse(inputFields[i].text, out var parsed)){
                output[i] = parsed;
            }else{
                output[i] = 0f;
            }
        }
        return output;
    }

    void SetFieldsFromVector (Vector4 inputVector) {
        for(int i=0; i<4; i++){
            inputFields[i].text = FormatFloatForField(inputVector[i]);
        }
    }

    string FormatFloatForField (float inputValue) {
        return $"{inputValue:F2}";
    }

    void FormatFieldText (int fieldIndex) {
        var field = inputFields[fieldIndex];
        var fieldRT = inputFieldRTs[fieldIndex];
        if(float.TryParse(field.text, out var parsed)){
            field.text = FormatFloatForField(parsed).ShortenNumberString();
        }else{
            field.text = "0";
        }
        field.textComponent.fontSize = defaultFontSize;
        field.textComponent.ForceMeshUpdate();
        if(field.textComponent.preferredWidth > fieldRT.rect.width){
            float scale = fieldRT.rect.width / field.textComponent.preferredWidth;
            field.textComponent.fontSize = defaultFontSize * scale;
        }
        field.textComponent.rectTransform.SetToFill();
    }
	
}
