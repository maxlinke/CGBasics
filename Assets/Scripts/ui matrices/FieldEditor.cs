﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace UIMatrices {

    public class FieldEditor : MonoBehaviour {

        [Header("Components")]
        [SerializeField] Image solidBackground;
        [SerializeField] Button insertButtonTemplate;
        [SerializeField] Button doneButton;
        [SerializeField] TextMeshProUGUI doneButtonText;
        [SerializeField] TMP_InputField expressionInputField;
        [SerializeField] TextMeshProUGUI expressionInputFieldText;
        [SerializeField] Image expressionInputFieldBackground;
        [SerializeField] TextMeshProUGUI editingInfo;
        [SerializeField] TextMeshProUGUI fieldInfo;
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
        [SerializeField] float solidBGWidthAddition;

        System.Action<string> onDoneEditing;

        List<InsertButton> varButtons;
        List<InsertButton> funcButtons;
        bool subscribedToInputSystem;

        bool selectionEnded = false;
        (int start, int end) selectionLimits;

        public void Initialize () {
            editingInfo.text = "(Editing is only possible in free mode)";
            fieldInfo.text = "Field expression";
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
                var funcInsert = (allFuncs[ind].paramNumber == 0) ? $"{funcName}()" : $"{funcName}(";
                btn.Setup(funcCall, funcDesc, funcInsert);
            });

            expressionInputField.onEndTextSelection.AddListener((s, i1, i2) => {
                selectionEnded = true;
                selectionLimits = (start: i1, end: i2);
                StartCoroutine(ResetSelectionWhenAppropriate());
            });

            IEnumerator ResetSelectionWhenAppropriate () {
                yield return new WaitUntil(() => !Input.GetKey(KeyCode.Mouse0));
                yield return null;
                selectionEnded = false;
                selectionLimits = default;
            }
        }

        public void LoadColors (ColorScheme cs) {
            // done button is already accounted for in the viewer
            solidBackground.color = cs.UiMatrixFieldEditorSolidBackground;
            expressionInputFieldBackground.color = Color.white;
            // expressionInputField.SetFadeTransition(0f, cs.UiMatrixFieldViewerDoneButton, cs.UiMatrixFieldViewerDoneButtonHover, cs.UiMatrixFieldViewerDoneButtonClick, Color.clear);
            expressionInputField.SetFadeTransition(0f, cs.UiMatrixFieldViewerDoneButton, cs.UiMatrixFieldViewerDoneButtonHover, cs.UiMatrixFieldViewerDoneButtonClick, cs.UiMatrixFieldViewerDoneButton);
            expressionInputField.textComponent.color = cs.UiMatrixFieldViewerDoneButtonText;
            expressionInputField.placeholder.color = 0.5f * cs.UiMatrixFieldViewerDoneButtonText + 0.5f * cs.UiMatrixFieldViewerDoneButton;
            expressionInputField.selectionColor = cs.UiMatrixFieldEditorSelectionColor;
            editingInfo.color = cs.UiMatrixFieldViewerDoneButtonText.WithOpacity(0.5f);
            fieldInfo.color = cs.UiMatrixFieldViewerDoneButtonText;
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
            expressionInputField.interactable = editable;
            expressionInputFieldText.overflowMode = editable ? TextOverflowModes.ScrollRect : TextOverflowModes.Ellipsis;
            // expressionInputFieldBackground.enabled = editable;
            editingInfo.SetGOActive(!editable);
            SetupDoneButton();
            varAndFuncScrollViewRT.SetSizeDeltaX(Mathf.Min(Screen.width, maxVarAndFuncAreaWidth));
            varAndFuncHeaderParent.SetSizeDeltaX(Mathf.Min(Screen.width, maxVarAndFuncAreaWidth));
            solidBackground.gameObject.GetComponent<RectTransform>().SetSizeDeltaX(varAndFuncScrollViewRT.rect.width + (2 * solidBGWidthAddition));
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
                    backgroundImage: cloned.GetComponentInChildren<Image>(),
                    shouldReplace: () => { return selectionEnded; },
                    replaceStartAndEnd: () => { return selectionLimits; }
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
            private System.Func<bool> shouldReplace;
            private System.Func<(int, int)> replaceStartAndEnd;

            public bool interactable {
                get {
                    return m_button.interactable;
                } set {
                    m_button.interactable = value;
                    m_backgroundImage.enabled = value;
                }
            }

            public void Initialize (TMP_InputField targetInputField, Button button, TextMeshProUGUI label, Image backgroundImage, System.Func<bool> shouldReplace, System.Func<(int, int)> replaceStartAndEnd){
                this.targetInputField = targetInputField;
                this.m_button = button;
                this.m_label = label;
                this.m_backgroundImage = backgroundImage;
                this.shouldReplace = shouldReplace;
                this.replaceStartAndEnd = replaceStartAndEnd;
            }

            public void Setup (string labelText, string hoverMessage, string insert) {
                m_label.text = labelText;
                this.hoverMessage = hoverMessage;
                m_button.onClick.RemoveAllListeners();
                m_button.onClick.AddListener(() => {
                    int cPos;
                    if(shouldReplace.Invoke()){
                        var limits = replaceStartAndEnd();
                        var start = Mathf.Min(limits.Item1, limits.Item2);
                        var end = Mathf.Max(limits.Item1, limits.Item2);
                        targetInputField.text = targetInputField.text.Remove(start, end - start);
                        cPos = start;
                    }else{
                        cPos = targetInputField.caretPosition;
                    }
                    targetInputField.text = targetInputField.text.Insert(cPos, insert);
                    EventSystem.current.SetSelectedGameObject(targetInputField.gameObject);
                    targetInputField.caretPosition = cPos + insert.Length;
                });
            }

            public void OnPointerEnter (PointerEventData eventData) {
                BottomLog.DisplayMessage(hoverMessage);
            }

            public void OnPointerExit (PointerEventData eventData) {
                BottomLog.ClearDisplay();
            }

            public void LoadColors (ColorScheme cs) {
                m_backgroundImage.color = Color.white;
                m_button.SetFadeTransition(0f, cs.UiMatrixFieldViewerDoneButton, cs.UiMatrixFieldViewerDoneButtonHover, cs.UiMatrixFieldViewerDoneButtonClick, Color.clear);
                m_label.color = cs.UiMatrixFieldViewerDoneButtonText;
            }

        }

    }

}
