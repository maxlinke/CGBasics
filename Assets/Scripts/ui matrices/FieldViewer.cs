using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIMatrices {

    public class FieldViewer : MonoBehaviour {

        private static FieldViewer instance;

        [Header("Components")]
        [SerializeField] FieldViewerField fieldTemplate;
        [SerializeField] Image backgroundImage;
        [SerializeField] RectTransform fieldParent;
        [SerializeField] Button doneButton;

        [Header("Settings")]
        [SerializeField] float spaceBetweenFields;

        FieldViewerField[] actualFields;

        bool initialized;
        UIMatrix currentCallingMatrix;
        System.Action<string[]> currentOnEndEditAction;

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

        // TODO rememember to do settext on the fields!
        void LoadColors (ColorScheme cs) {
            backgroundImage.color = cs.UiMatrixFieldViewerBackground;

            foreach(var field in actualFields){
                field.LoadColors(cs);
            }
            if(currentCallingMatrix != null){
                UpdateFieldTexts();
            }
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
            doneButton.onClick.AddListener(() => {HideAndReset();});
            fieldTemplate.SetGOActive(false);
            CreateIndividualFieldArray();

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
                    newField.Initialize(this, (currentCallingMatrix != null ? currentCallingMatrix[i] : string.Empty));
                    newField.rectTransform.SetParent(newFieldRT, false);
                    newField.rectTransform.SetToFillWithMargins(spaceBetweenFields);
                    actualFields[i] = newField;
                }
            }
        }

        // no onendedit, just straight up replace the string field array everytime a change is made..
        public static void Open (UIMatrix callingMatrix) {
            instance.Unhide(callingMatrix);
        }

        void Unhide (UIMatrix callingMatrix) {
            this.currentCallingMatrix = callingMatrix;
            gameObject.SetActive(true);                 // because activating loads the colors and that updates the fields, the matrix needs to be set before activation!

        }

        void HideAndReset () {
            currentCallingMatrix = null;
            currentOnEndEditAction = null;
            gameObject.SetActive(false);
        }

        public void FieldButtonClicked (FieldViewerField field) {
            // open the editor with the field's expression (and the matrix' editing privileges)
        }
    
    }

}