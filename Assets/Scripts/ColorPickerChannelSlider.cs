using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPickerChannelSlider : MonoBehaviour {

    [SerializeField] TextMeshProUGUI channelLabel;

    [Header("Slider")]
    [SerializeField] RectTransform sliderRT;
    [SerializeField] Slider slider;
    [SerializeField] Image sliderBG;
    [SerializeField] Image sliderFill;
    [SerializeField] Image sliderHandle;

    [Header("Input Field")]
    [SerializeField] RectTransform inputFieldRT;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Image inputFieldBG;
    [SerializeField] Graphic inputFieldMainText;

    bool initialized = false;
    bool blockSyncCalls;
    public float currentValue => slider.value;
    public float normalizedValue => slider.normalizedValue;

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
        slider.onValueChanged.AddListener(SliderValueChanged);
        inputField.onEndEdit.AddListener(InputFieldEndEdit);
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
                inputField.text = "0";
                return;
            }
            parsed = Mathf.Clamp(parsed, slider.minValue, slider.maxValue);
            var processed = GetInputFieldText(parsed);
            if(processed.Equals(newStringVal)){
                return;
            }
            blockSyncCalls = true;
            inputField.text = processed;
            slider.value = parsed;
            blockSyncCalls = false;
        }
    }

    string GetInputFieldText (float inputValue) {
        return $"{inputValue:F3}";
    }

    public void LoadColors (ColorScheme cs) {

    }
	
}
