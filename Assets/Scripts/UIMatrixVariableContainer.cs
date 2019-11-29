using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMatrixVariableContainer : MonoBehaviour {

    [SerializeField] UIMatrix parentMatrix;

    [Header("Components")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Button expandButton;
    [SerializeField] TextMeshProUGUI label;

    List<Variable> variables = new List<Variable>();

    bool m_expanded;
    bool m_editable;

    void Awake () {

    }

    public void ToggleExpand () {
        if(m_expanded){
            Retract();
        }else{
            Expand();
        }
    }

    public void Expand () {
        // also has to message the parent to resize itself
    }

    public void Retract () {
        // also has to message the parent to resize itself
    }

    public void LoadColors (ColorScheme cs) {
        backgroundImage.color = cs.UiMatrixVariablesBackground;
        expandButton.SetFadeTransitionDefaultAndDisabled(cs.UiMatrixVariablesLabelAndIcons, Color.magenta);     // should NEVER be disabled!
        // the add-button can be disabled tho. the usual half alpha i guess.
        label.color = cs.UiMatrixVariablesLabelAndIcons;
    }

    public void UpdateEditability () {
        this.m_editable = parentMatrix.editable;
        if(m_editable){

        }else{

        }   
    }

    public Dictionary<string, float> GetVariableMap () {
        return null;
    }

    public void AddVariable (string varName, float varValue, bool updateEverything = true) {
        if(TryGetVariable(varName, out _)){
            Debug.LogWarning("TODO also do a user-warning!");   // TODO also do a user-warning!
            return;
        }
        variables.Add(new Variable(varName, varValue));
        UpdateOrSetDirty(updateEverything);
    }

    public void EditVariable (string oldName, string newName, float newValue, bool updateEverything = true) {
        if(TryGetVariable(oldName, out var foundVar)){
            foundVar.name = newName;
            foundVar.floatValue = newValue;
            UpdateOrSetDirty(updateEverything);
        }else{
            ThrowVarNotFoundException(oldName);
        }
    }

    public void RemoveVariable (string varName, bool updateEverything = true) {
        if(TryGetVariable(varName, out var foundVar)){
            variables.Remove(foundVar);
            UpdateOrSetDirty(updateEverything);
        }else{
            ThrowVarNotFoundException(varName);
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
        UpdateOrSetDirty(updateEverything);
    }

    // instead of this, just reference the tmps?
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
