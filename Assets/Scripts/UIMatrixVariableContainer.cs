using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMatrixVariableContainer : MonoBehaviour {

    private const int MAX_VARIABLE_COUNT = 10;

    [SerializeField] UIMatrix parentMatrix;

    [Header("Components")]
    [SerializeField] RectTransform m_rectTransform;
    [SerializeField] RectTransform headerArea;
    [SerializeField] Image backgroundImage;
    [SerializeField] Button expandButton;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField, Tooltip("Should be facing down in standard rotation!")] Image expandArrow;
    [SerializeField] RectTransform varFieldArea;
    [SerializeField] RectTransform footerArea;
    [SerializeField] Button addButton;

    [Header("Var Field Template")]
    [SerializeField] UIMatrixVariableField varFieldTemplate;

    List<UIMatrixVariableField> variableFields = new List<UIMatrixVariableField>();

    bool m_expanded;
    bool m_editable;
    bool m_initialized;

    public RectTransform rectTransform => m_rectTransform;

    public void Initialize (bool startExpanded) {
        if(m_initialized){
            Debug.LogError("Duplicate Init call, aborting!");
            return;
        }
        // TODO some important stuff here i guess
        varFieldTemplate.SetGOActive(false);
        if(startExpanded){
            Expand();
        }else{
            Retract();
        }
        // addButton.onClick.AddListener(() => {AddVariable(true);});
        addButton.onClick.AddListener(() => {AddVariable("asdf", 5.25f);});
        expandButton.onClick.AddListener(() => {ToggleExpand();});
        m_initialized = true;
    }

    public void ToggleExpand () {
        if(m_expanded){
            Retract();
        }else{
            Expand();
        }
    }

    public void Expand () {
        varFieldArea.SetGOActive(true);
        UpdateVarFieldArea();
        footerArea.SetGOActive(true);
        expandArrow.transform.localEulerAngles = new Vector3(0, 0, 0);
        float totalHeight = 0;
        totalHeight += headerArea.rect.height;
        totalHeight += varFieldArea.rect.height;
        totalHeight += footerArea.rect.height;
        rectTransform.SetSizeDeltaY(totalHeight);
        m_expanded = true;
        parentMatrix.AutoResize();
    }

    void UpdateVarFieldArea () {
        float y = 0;
        foreach(var varField in variableFields){
            varField.rectTransform.anchoredPosition = new Vector2(0, y);
            y-=varField.rectTransform.rect.height;
        }
        varFieldArea.SetSizeDeltaY(Mathf.Abs(y));
    }

    public void Retract () {
        varFieldArea.SetGOActive(false);
        footerArea.SetGOActive(false);
        rectTransform.SetSizeDeltaY(headerArea.rect.height);
        expandArrow.transform.localEulerAngles = new Vector3(0, 0, 90);
        m_expanded = false;
        parentMatrix.AutoResize();
    }

    public void LoadColors (ColorScheme cs) {
        backgroundImage.color = cs.UiMatrixVariablesBackground;
        label.color = cs.UiMatrixVariablesLabelAndIcons;            // TODO text = "Variables (numberOfFields)"
        expandArrow.color = cs.UiMatrixVariablesLabelAndIcons;
        varFieldTemplate.LoadColors(cs);
        foreach(var vf in variableFields){
            vf.LoadColors(cs);
        }
        UpdateFieldColors();
        addButton.SetFadeTransitionDefaultAndDisabled(cs.UiMatrixVariablesLabelAndIcons, cs.UiMatrixVariablesLabelAndIconsDisabled);
    }

    public void UpdateEditability () {
        this.m_editable = parentMatrix.editable;
        foreach(var varField in variableFields){
            varField.interactable = m_editable;
        }
        UpdateAddButtonInteractability();
    }

    void UpdateAddButtonInteractability () {
        addButton.interactable = m_editable && (variableFields.Count < MAX_VARIABLE_COUNT);
    }

    public Dictionary<string, float> GetVariableMap () {
        var output = new Dictionary<string, float>();
        foreach(var varField in variableFields){
            bool validName = VariableNameIsValid(varField.enteredName, varField);
            bool validValue = float.TryParse(varField.enteredValue, out float parsedValue);
            if(validName && validValue){
                output.Add(varField.enteredName, parsedValue);
            }
        }
        return output;
    }

    public UIMatrixVariableField AddVariable (bool updateEverything = true) {
        if(!CheckIfVariableCanBeAdded()){
            return null;
        }
        var newVar = CreateNewVariableField();
        newVar.Initialize(this, false);
        UpdateOrSetDirty(updateEverything);
        return newVar;
    }

    public UIMatrixVariableField AddVariable (string varName, float varValue, bool updateEverything = true) {
        if(!CheckIfVariableCanBeAdded()){
            return null;
        }
        var newVar = CreateNewVariableField();
        newVar.Initialize(this, true, varName, varValue);
        UpdateOrSetDirty(updateEverything);
        return newVar;
    }

    private UIMatrixVariableField CreateNewVariableField () {
        if(!CheckIfVariableCanBeAdded()){
            throw new System.AccessViolationException("WHAT?!?!?!?!");
        }
        var newVar = Instantiate(varFieldTemplate);
        variableFields.Add(newVar);
        newVar.SetGOActive(true);
        newVar.rectTransform.SetParent(varFieldArea, false);
        newVar.LoadColors(ColorScheme.current);
        UpdateAddButtonInteractability();
        Expand();       // does all the resizing. and since variables can't be created when retracted, this isn't an issue
        return newVar;
    }

    bool CheckIfVariableCanBeAdded () {
        if(variableFields.Count >= MAX_VARIABLE_COUNT){
            Debug.LogError("Asked to create an additional variable, even though we're already at max number, this shouldn't happen! Aborting.");
            return false;
        }
        if(!m_expanded){
            Debug.LogError("Asked to add variable while not expanded, this is not allowed! Aborting.");
            return false;
        }
        return true;
    }

    public void EditVariable (string varName, float newValue, bool updateEverything = true) {
        // if(TryGetVariable(oldName, out var foundVar)){
        //     foundVar.name = newName;
        //     foundVar.floatValue = newValue;
        //     UpdateOrSetDirty(updateEverything);
        // }else{
        //     ThrowVarNotFoundException(oldName);
        // }
    }

    public void RemoveVariable (UIMatrixVariableField field, bool updateEverything = true) {
        if(variableFields.Contains(field)){
            variableFields.Remove(field);
            Destroy(field.gameObject);
            UpdateOrSetDirty(updateEverything);
            return;
        }
        ThrowVarNotFoundException(field.gameObject.name);
    }
    
    public void RemoveAllVariables (bool updateEverything = true) {
        for(int i=0; i<variableFields.Count; i++){
            Destroy(variableFields[i].gameObject);
        }
        variableFields.Clear();
        UpdateOrSetDirty(updateEverything);
    }

    public void VariableUpdated (UIMatrixVariableField field, bool updateEverything = true) {
        UpdateFieldColors();
        UpdateOrSetDirty(updateEverything);
    }

    void UpdateFieldColors () {
        foreach(var field in variableFields){
            field.UpdateNameFieldColor(VariableNameIsValid(field.enteredName, field) || field.enteredName == null || field.enteredName.Length < 1);
            field.UpdateValueFieldColor(float.TryParse(field.enteredValue, out _) || field.enteredValue == null || field.enteredValue.Length < 1);
        }
    }

    void ThrowVarNotFoundException (string varName) {
        throw new System.IndexOutOfRangeException($"Couldn't find variable \"{varName}\"!");
    }

    void UpdateOrSetDirty (bool updateEverything) {
        if(updateEverything){
            parentMatrix.UpdateMatrixAndGridView();
        }else{
            parentMatrix.SetMatrixNotUpToDate();
        }
    }

    public bool VariableNameIsValid (string potentialVarName, UIMatrixVariableField askingField) {
        if(potentialVarName == null){
            return false;
        }
        if(potentialVarName.Length < 1){
            return false;
        }
        var fc = potentialVarName[0];
        bool validFirstChar = (fc >= 'a' && fc <= 'z') || (fc >= 'A' && fc <= 'Z');
        if(!validFirstChar){
            return false;
        }
        foreach(var ch in potentialVarName){
            if(!(ch >= 'a' &&  ch <= 'z') && !(ch >= 'A' && ch <= 'Z') && !(ch >= '0' && ch <= '9')){
                return false;
            }
        }
        List<string> nameCache = new List<string>();
        for(int i=0; i<variableFields.Count; i++){
            var field = variableFields[i];
            if(field == askingField){
                return !nameCache.Contains(potentialVarName);
            }
            nameCache.Add(field.enteredName);
        }
        Debug.LogError("Iterated over entire collection and didn't find object ONCE! This is an issue!", askingField.gameObject);
        return false;
    }
	
}
