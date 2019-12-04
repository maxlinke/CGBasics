using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIMatrices {

    public class FieldViewerField : MonoBehaviour {

        [SerializeField] Image backgroundImage;
        [SerializeField] Button fieldEditButton;
        [SerializeField] TextMeshProUGUI fieldEditButtonTextField;
        [SerializeField] Image resultBackground;
        [SerializeField] TextMeshProUGUI resultTextField;

        FieldViewer viewer;
        bool initialized;
        string m_expression;
        int m_index;
        RectTransform m_rectTransform;

        public RectTransform rectTransform {
            get {
                if(m_rectTransform == null){
                    m_rectTransform = GetComponent<RectTransform>();
                }
                return m_rectTransform;
            }
        }

        public string expression {
            get {
                return m_expression;
            } set {
                m_expression = value;
                fieldEditButtonTextField.text = m_expression;
            }
        }

        public int index => m_index;

        public void Initialize (FieldViewer viewer, int index) {
            if(initialized){
                Debug.LogWarning("Duplicate init call! Aborting...", this.gameObject);
                return;
            }
            this.viewer = viewer;
            this.expression = string.Empty;
            this.m_index = index;
            fieldEditButton.onClick.AddListener(() => {viewer.FieldButtonClicked(this);});
            initialized = true;
        }

        public void LoadColors (ColorScheme cs) {
            backgroundImage.color = cs.UiMatrixBackground;
            fieldEditButton.SetFadeTransition(0f, cs.UiMatrixFieldBackground, cs.UiMatrixFieldBackgroundHighlighted, cs.UiMatrixFieldBackgroundClicked, cs.UiMatrixFieldBackgroundDisabled);
            fieldEditButtonTextField.color = cs.UiMatrixFieldText;
            // resultTextField.color = cs.          // gets its actual color from the rich text stuff
            resultBackground.color = cs.UiMatrixBackground.AlphaOver(cs.UiMatrixFieldBackground);
        }

        public void UpdateResultText (string resultText) {
            resultTextField.text = resultText;
        }
    
    }

}