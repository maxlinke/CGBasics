using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIMatrices {

    public class VariableContainer : MonoBehaviour {

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
        [SerializeField] VariableField varFieldTemplate;

        List<VariableField> variableFields = new List<VariableField>();

        UIMatrix.Editability m_editability;
        bool m_expanded;
        bool m_initialized;

        public RectTransform rectTransform => m_rectTransform;
        public float minHeight => headerArea.rect.height;
        public bool expanded => m_expanded;

        public void Initialize (IEnumerable<MatrixConfig.VarPreset> initialVariables, bool startExpanded) {
            if(m_initialized){
                Debug.LogError("Duplicate Init call, aborting!");
                return;
            }
            varFieldTemplate.SetGOActive(false);
            LoadConfig(initialVariables, false, startExpanded);
            UpdateLabel();
            addButton.onClick.AddListener(() => {AddVariable(true);});
            expandButton.onClick.AddListener(() => {ToggleExpand();});
            m_initialized = true;
        }

        public void LoadConfig (IEnumerable<MatrixConfig.VarPreset> variablesToLoad, bool updateEverything, bool expandAfterwards) {
            m_expanded = true;              // just so that the variable creation doesn't complain that variables can only created when expanded
            if(variablesToLoad != null){
                foreach(var varPreset in variablesToLoad){
                    var newField = CreateNewVariableField();
                    newField.Initialize(this, true, varPreset.varName, varPreset.varValue);
                }
            }
            UpdateOrSetDirty(updateEverything);
            if(expandAfterwards){           // the layout is done in here either way
                Expand();                   // with the variable fields being arranged properly here
            }else{
                Retract();                  // or not mattering here
            }
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

        void UpdateLabel () {
            label.text = $"Variables ({variableFields.Count})";
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
            label.color = cs.UiMatrixVariablesLabelAndIcons;
            expandArrow.color = cs.UiMatrixVariablesLabelAndIcons;
            varFieldTemplate.LoadColors(cs);
            varFieldTemplate.UpdateNameFieldColor(true);
            varFieldTemplate.UpdateValueFieldColor(true);
            foreach(var vf in variableFields){
                vf.LoadColors(cs);
            }
            UpdateFieldColors();
            addButton.SetFadeTransitionDefaultAndDisabled(cs.UiMatrixVariablesLabelAndIcons, cs.UiMatrixVariablesLabelAndIconsDisabled);
        }

        public void UpdateEditability () {
            this.m_editability = parentMatrix.editability;
            foreach(var varField in variableFields){
                varField.SetEditability(m_editability);
            }
            UpdateAddButtonInteractability();
        }

        void UpdateAddButtonInteractability () {
            addButton.interactable = (m_editability == UIMatrix.Editability.FULL) && (variableFields.Count < MAX_VARIABLE_COUNT);
        }

        public Dictionary<string, float> GetVariableMap () {
            var output = new Dictionary<string, float>();
            foreach(var varField in variableFields){
                bool validName = VariableNameIsValid(varField.enteredName, varField);
                bool validValue = float.TryParse(varField.enteredValue, out float parsedValue);
                if(varField.enteredValue == null || varField.enteredValue.Length < 1){
                    validValue = true;
                    parsedValue = 0;
                }
                if(validName && validValue){
                    output.Add(varField.enteredName, parsedValue);
                }
            }
            return output;
        }

        public VariableField AddVariable (bool updateEverything = true) {
            if(!CheckIfVariableCanBeAdded()){
                return null;
            }
            var newVar = CreateNewVariableField();
            newVar.Initialize(this, false);
            UpdateOrSetDirty(updateEverything);
            return newVar;
        }

        public VariableField AddVariable (string varName, float varValue, bool updateEverything = true) {
            if(!CheckIfVariableCanBeAdded()){
                return null;
            }
            var newVar = CreateNewVariableField();
            newVar.Initialize(this, true, varName, varValue);
            UpdateOrSetDirty(updateEverything);
            return newVar;
        }

        private VariableField CreateNewVariableField () {
            if(!CheckIfVariableCanBeAdded()){
                throw new System.AccessViolationException("WHAT?!?!?!?!");
            }
            var newVar = Instantiate(varFieldTemplate);
            variableFields.Add(newVar);
            newVar.SetGOActive(true);
            newVar.rectTransform.SetParent(varFieldArea, false);
            newVar.LoadColors(ColorScheme.current);
            newVar.SetEditability(m_editability);
            UpdateAddButtonInteractability();
            UpdateLabel();
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
            bool done = false;
            foreach(var varField in variableFields){
                if(varField.enteredName == varName){
                    if(!done){
                        varField.SetFloatValue(newValue, updateEverything);
                        done = true;
                    }else{
                        Debug.LogError($"Duplicate variable \"{varName}\"!");
                    }
                }
            }
            UpdateOrSetDirty(updateEverything);
            if(!done){
                ThrowVarNotFoundException(varName);
            }
        }

        public void RemoveVariable (VariableField field, bool updateEverything = true) {
            if(variableFields.Contains(field)){
                variableFields.Remove(field);
                Destroy(field.gameObject);
                UpdateLabel();
                Expand();       // takes care of resizing
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
            UpdateLabel();
            Expand();           // takes care of resizing
            UpdateOrSetDirty(updateEverything);
        }

        public void VariableUpdated (VariableField field, bool updateEverything = true) {
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

        public bool VariableNameIsValid (string potentialVarName, VariableField askingField) {
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

}