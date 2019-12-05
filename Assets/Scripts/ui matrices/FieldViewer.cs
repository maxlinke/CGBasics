using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIMatrices {

    public class FieldViewer : MonoBehaviour {

        private static FieldViewer instance;

        [Header("Components")]
        [SerializeField] FieldEditor fieldEditor;
        [SerializeField] GameObject mainScreen;
        [SerializeField] FieldViewerField fieldTemplate;
        [SerializeField] Image backgroundImage;
        [SerializeField] RectTransform fieldParent;
        [SerializeField] Button doneButton;
        [SerializeField] TextMeshProUGUI doneButtonText;

        [Header("Settings")]
        [SerializeField] float spaceBetweenFields;

        FieldViewerField[] actualFields;

        bool initialized;
        bool subscribedToInputSystem;
        UIMatrix currentCallingMatrix;

        void OnEnable () {
            if(!initialized){
                Initialize();
            }
            LoadColors(ColorScheme.current);
            ColorScheme.onChange += LoadColors;
        }

        void OnDisable () {
            ColorScheme.onChange -= LoadColors;
        }

        void OnDestroy () {
            if(instance == this){
                instance = null;
            }
        }

        void LoadColors (ColorScheme cs) {
            backgroundImage.color = cs.UiMatrixFieldViewerBackground;
            doneButton.SetFadeTransition(0f, cs.UiMatrixFieldViewerDoneButton, cs.UiMatrixFieldViewerDoneButtonHover, cs.UiMatrixFieldViewerDoneButtonClick, cs.UiMatrixFieldViewerDoneButtonDisabled);
            doneButtonText.color = cs.UiMatrixFieldViewerDoneButtonText;
            foreach(var field in actualFields){
                field.LoadColors(cs);
            }
            if(currentCallingMatrix != null){
                UpdateFieldTexts();
            }
            fieldEditor.LoadColors(cs);
        }

        void UpdateFieldTexts () {
            for(int i=0; i<actualFields.Length; i++){
                var field = actualFields[i];
                var expression = currentCallingMatrix[i];
                field.expression = expression;
                field.UpdateResultText(currentCallingMatrix.FieldExpressionToColoredResult(expression, out _, out _));
            }
        }

        void Initialize () {
            if(initialized){
                Debug.LogWarning("Duplicate init call, aborting!", this.gameObject);
                return;
            }
            if(instance != null){
                Debug.LogError($"Singleton violation, instance of {nameof(FieldViewer)} is not null!");
                return;
            }
            instance = this;
            fieldTemplate.SetGOActive(false);
            SetupDoneButton();
            CreateIndividualFieldArray();
            fieldEditor.Initialize();
            fieldEditor.Close();
            initialized = true;
            HideAndReset();

            void CreateIndividualFieldArray () {
                actualFields = new FieldViewerField[16];
                for(int i=0; i<16; i++){
                    float x = i % 4;
                    float y = i / 4;
                    // generate container
                    var newFieldRT = new GameObject($"Field {i} (x: {x}, y: {y})", typeof(RectTransform)).GetComponent<RectTransform>();
                    newFieldRT.SetParent(fieldParent, false);
                    newFieldRT.anchoredPosition = Vector2.zero;
                    newFieldRT.pivot = 0.5f * Vector2.one;
                    newFieldRT.anchorMin = new Vector2(x / 4f, (3-y) / 4f);
                    newFieldRT.anchorMax = new Vector2((x+1) / 4f, (3-y+1) / 4f);
                    newFieldRT.sizeDelta = Vector2.zero;
                    //actual field thingy
                    var newField = Instantiate(fieldTemplate);
                    newField.SetGOActive(true);
                    newField.Initialize(this, i);
                    newField.rectTransform.SetParent(newFieldRT, false);
                    newField.rectTransform.SetToFillWithMargins(spaceBetweenFields);
                    actualFields[i] = newField;
                }
            }
        }

        // no onendedit, just straight up replace the string field array everytime a change is made..
        public static void Open (UIMatrix callingMatrix, bool directlyEditField = false, int fieldIndex = -1) {
            instance.Unhide(callingMatrix, directlyEditField, fieldIndex);
        }

        void Unhide (UIMatrix callingMatrix, bool directlyEditField, int fieldIndex) {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            this.currentCallingMatrix = callingMatrix;
            gameObject.SetActive(true);                 // because activating loads the colors and that updates the fields, the matrix needs to be set before activation!
            InputSystem.Subscribe(this, new InputSystem.KeyEvent(KeyCode.Escape, onKeyDown: HideAndReset));
            subscribedToInputSystem = true;
            if(directlyEditField){
                FieldButtonClicked(actualFields[fieldIndex]);
            }
        }

        void SetupDoneButton () {
            doneButton.onClick.RemoveAllListeners();
            doneButton.onClick.AddListener(() => {HideAndReset();});
            doneButtonText.text = "Exit";
        }

        void HideAndReset () {
            currentCallingMatrix = null;
            gameObject.SetActive(false);
            if(subscribedToInputSystem){
                InputSystem.UnSubscribe(this);
                subscribedToInputSystem = false;
            }
        }

        public void FieldButtonClicked (FieldViewerField field) {
            mainScreen.SetActive(false);
            fieldEditor.Open(
                expression: field.expression, 
                editable: currentCallingMatrix.editability == UIMatrix.Editability.FULL, 
                variables: currentCallingMatrix.VariableContainer.GetVariableMap(),
                onDoneEditing: (exp) => {
                    currentCallingMatrix.UpdateSingleFieldString(field.index, exp, true);
                    mainScreen.SetActive(true);
                    UpdateFieldTexts();
                    SetupDoneButton();
                }
            );
        }
    
    }

}