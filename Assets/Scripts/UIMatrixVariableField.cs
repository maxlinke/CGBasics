using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMatrixVariableField : MonoBehaviour {

    [SerializeField] RectTransform m_rectTransform;
    [SerializeField] TMP_InputField varNameField;
    [SerializeField] TMP_InputField varValueField;
    [SerializeField] Button deleteButton;

    UIMatrixVariableContainer parentContainer;
    Color validTextColor;
    Color invalidTextColor;
    bool m_initialized;

    public RectTransform rectTransform => m_rectTransform;
    private string m_enteredName;
    public string enteredName => m_enteredName;
    private string m_enteredValue;
    public string enteredValue => m_enteredValue;

    public bool interactable {
        set {
            varNameField.interactable = value;
            varValueField.interactable = value;
            deleteButton.interactable = value;
        }
    }

    public void Initialize (UIMatrixVariableContainer parentContainer, string initialName, float initialValue) {
        if(m_initialized){
            Debug.LogWarning("Duplicate initialization call! Aborting...");
            return;
        }
        this.parentContainer = parentContainer;
        varNameField.text = initialName;
        varValueField.text = initialValue.ToString();
        
        varNameField.onEndEdit.AddListener((input) => {
            m_enteredName = input;
        });
        varValueField.onEndEdit.AddListener((input) => {
            m_enteredValue = input;
        });
        deleteButton.onClick.AddListener(() => {
            parentContainer.RemoveVariable(this, true);
        });

        this.m_initialized = true;
    }

    public void UpdateNameFieldColor (bool isValid) {
        varNameField.textComponent.color = isValid ? validTextColor : invalidTextColor;
    }

    public void UpdateValueFieldColor (bool isValid) {
        varValueField.textComponent.color = isValid ? validTextColor : invalidTextColor;
    }

    public void LoadColors (ColorScheme cs) {
        deleteButton.SetFadeTransitionDefaultAndDisabled(cs.UiMatrixVariablesLabelAndIcons, cs.UiMatrixVariablesLabelAndIconsDisabled);
        varNameField.SetFadeTransition(0f, cs.UiMatrixVariablesFieldBackground, cs.UiMatrixVariablesFieldBackgroundHover, cs.UiMatrixVariablesFieldBackgroundClick, cs.UiMatrixVariablesFieldBackgroundDisabled);
        varNameField.placeholder.color = cs.UiMatrixVariablesFieldElement.WithHalfAlpha();
        // varNameField.textComponent.color = cs.UiMatrixVariablesFieldElement;
        varValueField.SetFadeTransition(0f, cs.UiMatrixVariablesFieldBackground, cs.UiMatrixVariablesFieldBackgroundHover, cs.UiMatrixVariablesFieldBackgroundClick, cs.UiMatrixVariablesFieldBackgroundDisabled);
        varValueField.placeholder.color = cs.UiMatrixVariablesFieldElement.WithHalfAlpha();
        // varValueField.textComponent.color = cs.UiMatrixVariablesFieldElement;
        validTextColor = cs.UiMatrixVariablesFieldElement;
        invalidTextColor = cs.UiMatrixVariablesFieldElementInvalid;
    }
	
}
