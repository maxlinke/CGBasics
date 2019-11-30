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
    public string enteredName => varNameField.text;
    public string enteredValue => varValueField.text;

    public bool interactable {
        set {
            varNameField.interactable = value;
            varValueField.interactable = value;
            deleteButton.interactable = value;
        }
    }

    public void Initialize (UIMatrixVariableContainer parentContainer, bool initWithValues = false, string initialName = "", float initialValue = 0) {
        if(m_initialized){
            Debug.LogWarning("Duplicate initialization call! Aborting...");
            return;
        }
        this.parentContainer = parentContainer;
        
        varNameField.onEndEdit.AddListener((input) => {
            parentContainer.VariableUpdated(this, true);
        });
        varValueField.onEndEdit.AddListener((input) => {
            parentContainer.VariableUpdated(this, true);
        });
        deleteButton.onClick.AddListener(() => {
            parentContainer.RemoveVariable(this, true);
        });

        if(initWithValues){
            varNameField.text = initialName;
            varValueField.text = initialValue.ToString();
            varNameField.onEndEdit.Invoke(varNameField.text);
            varValueField.onEndEdit.Invoke(varValueField.text);
        }

        this.m_initialized = true;
    }

    public void SetNameValue (string newValue, bool updateEverything = true) {
        varNameField.text = newValue;
        if(updateEverything){
            varNameField.onEndEdit.Invoke(varNameField.text);
        }
    }

    public void SetFloatValue (float newValue, bool updateEverything = true) {
        varValueField.text = newValue.ToString();
        if(updateEverything){
            varValueField.onEndEdit.Invoke(varValueField.text);
        }
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
        varNameField.selectionColor = cs.UiMatrixVariablesFieldSelection;
        // varNameField.textComponent.color = cs.UiMatrixVariablesFieldElement;
        varValueField.SetFadeTransition(0f, cs.UiMatrixVariablesFieldBackground, cs.UiMatrixVariablesFieldBackgroundHover, cs.UiMatrixVariablesFieldBackgroundClick, cs.UiMatrixVariablesFieldBackgroundDisabled);
        varValueField.placeholder.color = cs.UiMatrixVariablesFieldElement.WithHalfAlpha();
        varValueField.selectionColor = cs.UiMatrixVariablesFieldSelection;
        // varValueField.textComponent.color = cs.UiMatrixVariablesFieldElement;
        validTextColor = cs.UiMatrixVariablesFieldElement;
        invalidTextColor = cs.UiMatrixVariablesFieldElementInvalid;
    }
	
}
