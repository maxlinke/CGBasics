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
            y+=varField.rectTransform.rect.height;
        }
        varFieldArea.SetSizeDeltaY(y);
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
        return new Dictionary<string, float>();
    }

    public void AddVariable (string varName, float varValue, bool updateEverything = true) {
        // if(TryGetVariable(varName, out _)){
        //     Debug.LogWarning("TODO also do a user-warning!");   // TODO also do a user-warning!
        //     return;
        // }
        // // variables.Add(new Variable(varName, varValue));
        // UpdateOrSetDirty(updateEverything);
    }

    public void EditVariable (string oldName, string newName, float newValue, bool updateEverything = true) {
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
        Debug.LogError($"Asked to delete an untracked {nameof(UIMatrixVariableField)} ({field.name})! This is an issue!");
    }

    public void RemoveVariable (string varName, bool updateEverything = true) {
        // if(TryGetVariable(varName, out var foundVar)){
        //     // variables.Remove(foundVar);
        //     UpdateOrSetDirty(updateEverything);
        // }else{
        //     ThrowVarNotFoundException(varName);
        // }
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
            if(!(ch >= 'a' &&  ch <= 'z') || !(ch >= 'A' && ch <= 'Z') || !(ch >= '0' && ch <= '9')){
                return false;
            }
        }
        foreach(var field in variableFields){
            if(field != askingField && field.enteredName == potentialVarName){
                return false;
            }
        }
        return true;
    }   

    public void RemoveAllVariables (bool updateEverything = true) {
        // variables.Clear();
        UpdateOrSetDirty(updateEverything);
    }
	
}
