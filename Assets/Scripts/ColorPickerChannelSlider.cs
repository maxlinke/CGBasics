using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPickerChannelSlider : MonoBehaviour {

    [SerializeField] RectTransform m_rectTransform;
    [SerializeField] TextMeshProUGUI channelLabel;
    [SerializeField] SyncedSliderAndInputField sliderAndInputField;

    // [Header("Slider")]
    // [SerializeField] Slider slider;
    // [SerializeField] Image sliderBG;
    // [SerializeField] Image sliderFill;
    // [SerializeField] Image sliderHandle;

    // [Header("Input Field")]
    // [SerializeField] TMP_InputField inputField;
    // [SerializeField] Image inputFieldBG;
    // [SerializeField] Graphic inputFieldMainText;

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
        // slider.maxValue = maxValue;
        // slider.value = initValue;
        // inputField.text = GetInputFieldText(slider.value);
        // inputField.gameObject.AddComponent<ScrollableNumberInputField>().Initialize(inputField);
        // slider.onValueChanged.AddListener(SliderValueChanged);
        // inputField.onEndEdit.AddListener(InputFieldEndEdit);
        
        // blockSyncCalls = false;

        // void SliderValueChanged (float newVal) {
        //     if(blockSyncCalls){
        //         return;
        //     }
        //     newVal = Mathf.Clamp(newVal, slider.minValue, slider.maxValue);
        //     blockSyncCalls = true;
        //     inputField.text = GetInputFieldText(newVal);
        //     blockSyncCalls = false;
        // }

        // void InputFieldEndEdit (string newStringVal) {
        //     if(blockSyncCalls){
        //         return;
        //     }
        //     if(!float.TryParse(newStringVal, out var parsed)){
        //         blockSyncCalls = true;
        //         slider.value = 0f;
        //         inputField.text = GetInputFieldText(slider.value);
        //         blockSyncCalls = false;
        //         return;
        //     }
        //     parsed = Mathf.Clamp(parsed, slider.minValue, slider.maxValue);
        //     var processed = GetInputFieldText(parsed);
        //     blockSyncCalls = true;
        //     inputField.text = processed;
        //     slider.value = parsed;
        //     blockSyncCalls = false;
        // }
    }

    // string GetInputFieldText (float inputValue) {
    //     return $"{inputValue:F3}".ShortenNumberString();
    // }

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
