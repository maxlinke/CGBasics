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
    [SerializeField] TextMeshProUGUI editingInfo;
    [SerializeField] RectTransform varAndFuncParent;
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

    public void Initialize () {
        editingInfo.text = "Editing is only possible in free mode";
        variableHeader.text = "Variables";
        functionHeader.text = "Functions";
        insertButtonTemplate.SetGOActive(false);
        varButtons = new List<InsertButton>();
        funcButtons = new List<InsertButton>();
        var allFuncs = StringExpressions.Functions.GetAllFunctions();
        CreateInsertButtons(allFuncs.Length, funcButtonParent, funcButtons, (ind, btn) => {
            var funcName = allFuncs[ind].functionName;
            var funcDesc = allFuncs[ind].description;
            btn.Setup(funcName, funcDesc, $"{funcName}(");
        });
    }

    public void LoadColors (ColorScheme cs) {
        // done button is already accounted for in the viewer
        // TODO the rest of the colors
        variableHeader.color = cs.UiMatrixFieldText;
        functionHeader.color = cs.UiMatrixFieldText;
        foreach(var b in varButtons){
            b.LoadColors(cs);
        }
        foreach(var b in funcButtons){
            b.LoadColors(cs);
        }
    }

    public void Open (string expression, bool editable, Dictionary<string, float> variables, System.Action<string> onDoneEditing) {
        gameObject.SetActive(true);
        expressionInputField.text = expression;
        expressionInputField.interactable = editable;       // TODO onEndEdit check maybe? just red or white?
        editingInfo.SetGOActive(!editable);
        SetupDoneButton();
        varAndFuncParent.SetSizeDeltaX(Mathf.Min(Screen.width, maxVarAndFuncAreaWidth));
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
        foreach(var b in varButtons){
            b.interactable = editable;
        }
        foreach(var b in funcButtons){
            b.interactable = editable;
        }
        this.onDoneEditing = onDoneEditing;
    }

    public void Close () {
        for(int i=varButtons.Count-1; i>=0; i--){
            Destroy(varButtons[i].gameObject);
        }
        varButtons.Clear();
        EventSystem.current.SetSelectedGameObject(null);    // deselecting the input field. might be unnecessary
        gameObject.SetActive(false);
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
            y -= newButtonRT.rect.height - buttonVerticalOffset;
        }
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
            m_backgroundImage.color = cs.UiMatrixBackground;
            m_button.SetFadeTransition(0f, cs.UiMatrixFieldBackground, cs.UiMatrixFieldBackgroundHighlighted, cs.UiMatrixFieldBackgroundClicked, Color.clear);
            m_label.color = cs.UiMatrixFieldText;
        }

    }

}
