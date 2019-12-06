using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class FieldEditor : MonoBehaviour {

    [Header("Components")]
    [SerializeField] Button insertButtonTemplate;
    [SerializeField] Button doneButton;
    [SerializeField] TextMeshProUGUI doneButtonText;
    [SerializeField] TMP_InputField expressionInputField;
    [SerializeField] Image expressionInputFieldBackground;
    [SerializeField] TextMeshProUGUI editingInfo;
    [SerializeField] ScrollRect varAndFuncScrollView;
    [SerializeField] RectTransform varAndFuncScrollViewRT;
    [SerializeField] RectTransform varAndFuncScrollViewContentRT;
    [SerializeField] RectTransform varAndFuncHeaderParent;
    [SerializeField] TextMeshProUGUI variableHeader;
    [SerializeField] TextMeshProUGUI functionHeader;
    [SerializeField] RectTransform varButtonParent;
    [SerializeField] RectTransform funcButtonParent;

    [Header("Settings")]
    [SerializeField] float maxVarAndFuncAreaWidth;
    [SerializeField] float buttonVerticalOffset;
    
    System.Action<string> onDoneEditing;

    List<InsertButton> varButtons;
    List<InsertButton> funcButtons;
    bool subscribedToInputSystem;

    public void Initialize () {
        editingInfo.text = "Editing is only possible in free mode";
        variableHeader.text = "Variables";
        functionHeader.text = "Functions";
        ((TextMeshProUGUI)(expressionInputField.placeholder)).text = "Enter an expression here";
        insertButtonTemplate.SetGOActive(false);
        varButtons = new List<InsertButton>();
        funcButtons = new List<InsertButton>();
        var allFuncs = StringExpressions.Functions.GetAllFunctions();
        CreateInsertButtons(allFuncs.Length, funcButtonParent, funcButtons, (ind, btn) => {
            var funcName = allFuncs[ind].functionName;
            var funcDesc = allFuncs[ind].description;
            var funcCall = allFuncs[ind].exampleCall;
            btn.Setup(funcCall, funcDesc, $"{funcName}(");
        });
    }

    public void LoadColors (ColorScheme cs) {
        // done button is already accounted for in the viewer
        expressionInputFieldBackground.color = Color.white;
        expressionInputField.SetFadeTransition(0f, cs.UiMatrixFieldViewerDoneButton, cs.UiMatrixFieldViewerDoneButtonHover, cs.UiMatrixFieldViewerDoneButtonClick, Color.clear);
        expressionInputField.textComponent.color = cs.UiMatrixFieldViewerDoneButtonText;
        expressionInputField.placeholder.color = 0.5f * cs.UiMatrixFieldViewerDoneButtonText + 0.5f * cs.UiMatrixFieldViewerDoneButton;
        editingInfo.color = cs.UiMatrixFieldViewerDoneButtonText;
        variableHeader.color = cs.UiMatrixFieldViewerDoneButtonText;
        functionHeader.color = cs.UiMatrixFieldViewerDoneButtonText;
        foreach(var b in varButtons){
            b.LoadColors(cs);
        }
        foreach(var b in funcButtons){
            b.LoadColors(cs);
        }
        varAndFuncScrollView.verticalScrollbar.GetComponent<Image>().color = cs.UiMatrixBackground.AlphaOver(cs.UiMatrixFieldBackground);
        varAndFuncScrollView.verticalScrollbar.SetFadeTransition(0f, cs.UiMatrixFieldViewerDoneButton, cs.UiMatrixFieldViewerDoneButtonHover, cs.UiMatrixFieldViewerDoneButtonClick, cs.UiMatrixFieldBackgroundDisabled);
        varAndFuncScrollView.verticalScrollbar.handleRect.GetComponent<Image>().color = Color.white;
    }

    public void Open (string expression, bool editable, Dictionary<string, float> variables, System.Action<string> onDoneEditing) {
        EventSystem.current.SetSelectedGameObject(null);
        gameObject.SetActive(true);
        expressionInputField.text = expression;
        expressionInputField.interactable = editable;       // TODO onEndEdit check maybe? just red or white?
        expressionInputFieldBackground.enabled = editable;
        editingInfo.SetGOActive(!editable);
        SetupDoneButton();
        varAndFuncScrollViewRT.SetSizeDeltaX(Mathf.Min(Screen.width, maxVarAndFuncAreaWidth));
        varAndFuncHeaderParent.SetSizeDeltaX(Mathf.Min(Screen.width, maxVarAndFuncAreaWidth));
        string[] varNames = new string[variables.Count];
        float[] varValues = new float[variables.Count];
        int varIndex = 0;
        foreach(var key in variables.Keys){
            varNames[varIndex] = key;
            varValues[varIndex] = variables[key];
            varIndex++;
        }
        CreateInsertButtons(variables.Count, varButtonParent, varButtons, (ind, btn) => {
            string varName = varNames[ind];
            float varVal = varValues[ind];
            btn.Setup(varName, varVal.ToString(), varName);
        });
        varAndFuncScrollViewContentRT.SetSizeDeltaY(Mathf.Max(varButtonParent.rect.height, funcButtonParent.rect.height));
        foreach(var b in varButtons){
            b.interactable = editable;
            b.LoadColors(ColorScheme.current);
        }
        foreach(var b in funcButtons){
            b.interactable = editable;
        }
        InputSystem.Subscribe(this, new InputSystem.KeyEvent(KeyCode.Escape, onKeyDown: Close));
        subscribedToInputSystem = true;
        this.onDoneEditing = onDoneEditing;
    }

    public void Close () {
        for(int i=varButtons.Count-1; i>=0; i--){
            Destroy(varButtons[i].gameObject);
        }
        varButtons.Clear();
        EventSystem.current.SetSelectedGameObject(null);    // deselecting the input field. might be unnecessary
        gameObject.SetActive(false);
        if(subscribedToInputSystem){                        // because this gets called on init basically and we're not subscribed to anything yet there...
            InputSystem.UnSubscribe(this);
            subscribedToInputSystem = false;
        }
        BottomLog.ClearDisplay();
        onDoneEditing?.Invoke(expressionInputField.text);
    }
	
    void SetupDoneButton () {
        doneButton.onClick.RemoveAllListeners();
        doneButton.onClick.AddListener(() => {Close();});
        doneButtonText.text = "Done";
    }

    void CreateInsertButtons (int buttonCount, RectTransform parentRT, List<InsertButton> list, System.Action<int, InsertButton> setupButton) {
        float y = 0;
        for(int i=0; i<buttonCount; i++){
            var cloned = Instantiate(insertButtonTemplate);
            cloned.SetGOActive(true);
            cloned.gameObject.AddComponent(typeof(InsertButton));
            var newButton = cloned.GetComponent<InsertButton>();
            newButton.Initialize(
                targetInputField: expressionInputField,
                button: cloned,
                label: cloned.gameObject.GetComponentInChildren<TextMeshProUGUI>(),
                backgroundImage: cloned.GetComponentInChildren<Image>()
            );
            var newButtonRT = cloned.GetComponent<RectTransform>();
            newButtonRT.SetParent(parentRT, false);
            newButtonRT.anchoredPosition = new Vector2(0, y);
            setupButton(i, newButton);
            list.Add(newButton);
            y -= newButtonRT.rect.height + buttonVerticalOffset;   
        }
        parentRT.SetSizeDeltaY(Mathf.Abs(y));
    }

    private class InsertButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        private Button m_button;
        private TextMeshProUGUI m_label;
        private Image m_backgroundImage;
        private TMP_InputField targetInputField;
        private string hoverMessage;

        public bool interactable {
            get {
                return m_button.interactable;
            } set {
                m_button.interactable = value;
                m_backgroundImage.enabled = value;
            }
        }

        public void Initialize (TMP_InputField targetInputField, Button button, TextMeshProUGUI label, Image backgroundImage){
            this.targetInputField = targetInputField;
            this.m_button = button;
            this.m_label = label;
            this.m_backgroundImage = backgroundImage;
        }

        public void Setup (string labelText, string hoverMessage, string insert) {
            m_label.text = labelText;
            this.hoverMessage = hoverMessage;
            m_button.onClick.RemoveAllListeners();
            m_button.onClick.AddListener(() => {
                targetInputField.text += insert;
                EventSystem.current.SetSelectedGameObject(targetInputField.gameObject);
                targetInputField.MoveTextEnd(false);
            });
        }

        public void OnPointerEnter (PointerEventData eventData) {
            BottomLog.DisplayMessage(hoverMessage);
        }

        public void OnPointerExit (PointerEventData eventData) {
            BottomLog.ClearDisplay();
        }

        // TODO refactor the whole colorscheme so there's fewer overall colors (there's a LOT of overlap...)
        public void LoadColors (ColorScheme cs) {
            m_backgroundImage.color = Color.white;
            m_button.SetFadeTransition(0f, cs.UiMatrixFieldViewerDoneButton, cs.UiMatrixFieldViewerDoneButtonHover, cs.UiMatrixFieldViewerDoneButtonClick, Color.clear);
            m_label.color = cs.UiMatrixFieldViewerDoneButtonText;
        }

    }

}
