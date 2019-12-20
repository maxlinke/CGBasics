using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPickerChannelSlider : MonoBehaviour {

    [SerializeField] RectTransform m_rectTransform;

    [Header("Label")]
    [SerializeField] TextMeshProUGUI channelLabel;

    [Header("Slider")]
    [SerializeField] Slider slider;
    [SerializeField] Image sliderBG;
    [SerializeField] Image sliderFill;
    [SerializeField] Image sliderHandle;

    [Header("Input Field")]
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Image inputFieldBG;
    [SerializeField] Graphic inputFieldMainText;

    bool initialized = false;
    bool blockSyncCalls;
    public float currentValue {
        get {
            return slider.value;
        } set {
            slider.value = Mathf.Clamp(value, slider.minValue, slider.maxValue);
        }
    }

    public float normalizedValue {
        get {
            return slider.normalizedValue;
        } set {
            slider.value = slider.minValue + (Mathf.Clamp01(value) * (slider.maxValue - slider.minValue));
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
        slider.maxValue = maxValue;
        slider.value = initValue;
        inputField.text = GetInputFieldText(slider.value);
        inputField.gameObject.AddComponent<ScrollableNumberInputField>().Initialize(inputField);
        slider.onValueChanged.AddListener(SliderValueChanged);
        inputField.onEndEdit.AddListener(InputFieldEndEdit);
        channelLabel.text = labelText.Trim();
        blockSyncCalls = false;

        void SliderValueChanged (float newVal) {
            if(blockSyncCalls){
                return;
            }
            newVal = Mathf.Clamp(newVal, slider.minValue, slider.maxValue);
            blockSyncCalls = true;
            inputField.text = GetInputFieldText(newVal);
            blockSyncCalls = false;
        }

        void InputFieldEndEdit (string newStringVal) {
            if(blockSyncCalls){
                return;
            }
            if(!float.TryParse(newStringVal, out var parsed)){
                blockSyncCalls = true;
                slider.value = 0f;
                inputField.text = GetInputFieldText(slider.value);
                blockSyncCalls = false;
                return;
            }
            parsed = Mathf.Clamp(parsed, slider.minValue, slider.maxValue);
            var processed = GetInputFieldText(parsed);
            blockSyncCalls = true;
            inputField.text = processed;
            slider.value = parsed;
            blockSyncCalls = false;
        }
    }

    string GetInputFieldText (float inputValue) {
        return $"{inputValue:F3}".ShortenNumberString();
    }

    public void LoadColors (ColorScheme cs) {
        channelLabel.color = cs.ColorPickerSliderLabel;
        sliderBG.color = cs.ColorPickerSliderBackground;
        sliderFill.color = cs.ColorPickerSliderFill;
        sliderHandle.color = Color.white;
        slider.SetFadeTransition(0f, cs.ColorPickerSliderHandle, cs.ColorPickerSliderHandleHover, cs.ColorPickerSliderHandleClick, Color.magenta);
        inputFieldMainText.color = cs.ColorPickerInputFieldText;
        inputField.selectionColor = cs.ColorPickerInputFieldSelection;
        inputFieldBG.color = Color.white;
        inputField.SetFadeTransition(0f, cs.ColorPickerInputFieldBackground, cs.ColorPickerInputFieldBackgroundHover, cs.ColorPickerInputFieldBackgroundClick, Color.magenta);
    }
	
}
