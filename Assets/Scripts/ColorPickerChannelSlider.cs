using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPickerChannelSlider : MonoBehaviour {

    [SerializeField] RectTransform m_rectTransform;
    [SerializeField] TextMeshProUGUI channelLabel;
    [SerializeField] SyncedSliderAndInputField sliderAndInputField;

    bool initialized = false;
    public float currentValue {
        get {
            return sliderAndInputField.currentValue;
        } set {
            sliderAndInputField.currentValue = value;
        }
    }

    public float normalizedValue {
        get {
            return sliderAndInputField.normalizedValue;
        } set {
            sliderAndInputField.normalizedValue = value;
        }
    }

    public RectTransform rectTransform => m_rectTransform;

    public void Initialize (string labelText, float maxValue, float initValue) {
        if(initialized){
            Debug.LogWarning("Duplicate init call, aborting!", this.gameObject);
            return;
        }
        if(initValue > maxValue){
            Debug.LogWarning("Init value is greater than max value... U dun goofed!", this.gameObject);
            initValue = maxValue;
        }
        channelLabel.text = labelText.Trim();
        sliderAndInputField.formatString = (s) => {return $"{s:F3}".ShortenNumberString();};
        sliderAndInputField.SetSliderRange(0, maxValue);
        sliderAndInputField.currentValue = initValue;
    }

    public void LoadColors (ColorScheme cs) {
        channelLabel.color = cs.ColorPickerSliderLabel;
        sliderAndInputField.sliderBG.color = cs.ColorPickerSliderBackground;
        sliderAndInputField.sliderFill.color = cs.ColorPickerSliderFill;
        sliderAndInputField.sliderHandle.color = Color.white;
        sliderAndInputField.slider.SetFadeTransition(0f, cs.ColorPickerSliderHandle, cs.ColorPickerSliderHandleHover, cs.ColorPickerSliderHandleClick, Color.magenta);
        sliderAndInputField.inputFieldText.color = cs.ColorPickerInputFieldText;
        sliderAndInputField.inputFieldPlaceholder.color = Color.clear;
        sliderAndInputField.inputField.selectionColor = cs.ColorPickerInputFieldSelection;
        sliderAndInputField.inputFieldBG.color = Color.white;
        sliderAndInputField.inputField.SetFadeTransition(0f, cs.ColorPickerInputFieldBackground, cs.ColorPickerInputFieldBackgroundHover, cs.ColorPickerInputFieldBackgroundClick, Color.magenta);
    }
	
}
