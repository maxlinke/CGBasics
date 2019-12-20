using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SyncedSliderAndInputField : MonoBehaviour {

    [SerializeField] RectTransform m_rectTransform;

    [Header("Slider")]
    [SerializeField] Slider m_slider;
    [SerializeField] Image m_sliderBG;
    [SerializeField] Image m_sliderFill;
    [SerializeField] Image m_sliderHandle;

    [Header("Input Field")]
    [SerializeField] TMP_InputField m_inputField;
    [SerializeField] Image m_inputFieldBG;
    [SerializeField] TMP_Text m_inputFieldText;
    [SerializeField] TMP_Text m_inputFieldPlaceholder;

    public RectTransform rectTransform => m_rectTransform;

    public Slider slider => m_slider;
    public Graphic sliderBG => m_sliderBG;
    public Graphic sliderFill => m_sliderFill;
    public Graphic sliderHandle => m_sliderHandle;

    public TMP_InputField inputField => m_inputField;
    public Graphic inputFieldBG => m_inputFieldBG;
    public Graphic inputFieldText => m_inputFieldText;
    public TMP_Text inputFieldPlaceholder => m_inputFieldPlaceholder;

    bool initialized = false;
    bool blockSyncCalls = false;
    float m_currentValue = 0f;

    public float currentValue {
        get {
            return m_currentValue;
        } set {
            inputField.text = GetInputFieldText(value);
            inputField.onEndEdit.Invoke(inputField.text);
        }
    }

    public float normalizedValue {
        get {
            return (m_currentValue - slider.minValue) / (slider.maxValue - slider.minValue);
        } set {
            var newValue = slider.minValue + (Mathf.Clamp01(value) * (slider.maxValue - slider.minValue));
            inputField.text = GetInputFieldText(newValue);
            inputField.onEndEdit.Invoke(inputField.text);
        }
    }

    [System.NonSerialized] public bool clampInputFieldValues = true;
    public event System.Action<float> onValueUpdated = delegate {};
    public System.Func<float, string> formatString = null;

    void OnEnable () {
        if(!initialized){
            inputField.gameObject.AddComponent<ScrollableNumberInputField>().Initialize(inputField);
            inputField.onEndEdit.AddListener(InputFieldSubmit);
            slider.onValueChanged.AddListener(SliderValueChanged);
            initialized = true;

            void SliderValueChanged (float newSliderValue) {
                if(blockSyncCalls){
                    return;
                }
                DoWithoutSync(() => {
                    inputField.text = GetInputFieldText(newSliderValue);
                });
                m_currentValue = newSliderValue;
                onValueUpdated.Invoke(m_currentValue);
            }

            void InputFieldSubmit (string newString) {
                if(blockSyncCalls){
                    return;
                }
                if(!float.TryParse(newString, out var parsed)){
                    parsed = slider.minValue;
                }
                DoWithoutSync(() => {
                    slider.value = parsed;
                    inputField.text = GetInputFieldText(parsed);
                });
                m_currentValue = parsed;
                onValueUpdated.Invoke(m_currentValue);
            }
        }
    }

    public void SetSliderRange (float newMin, float newMax) {
        DoWithoutSync(() => {
            var newSliderValue = Mathf.Clamp(slider.value, newMin, newMax);
            slider.minValue = newMin;
            slider.maxValue = newMax;
            slider.value = newSliderValue;
            inputField.text = GetInputFieldText(newSliderValue);
        });
    }

    string GetInputFieldText (float valueToFormat) {
        if(clampInputFieldValues){
            valueToFormat = Mathf.Clamp(valueToFormat, slider.minValue, slider.maxValue);
        }
        if(formatString != null){
            return formatString(valueToFormat);
        }
        return valueToFormat.ToString().ShortenNumberString();
    }

    void DoWithoutSync (System.Action doThing) {
        blockSyncCalls = true;
        doThing();
        blockSyncCalls = false;
    }
	
}
