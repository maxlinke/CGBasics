using UnityEngine;

namespace LightingModels {

    public class FloatPropertyField : UIPropertyField {

        [SerializeField] SyncedSliderAndInputField sliderAndInputField;
        [SerializeField] float minSliderAndInputFieldWidth;
        [SerializeField] float maxSliderAndInputFieldWidth;
        [SerializeField] float maxWidthRectTransformWidth;
        [SerializeField] float minWidthRectTransformWidth;
        [SerializeField] float labelRightMargin;

        public override void LoadColors (ColorScheme cs) {
            // TODO color loading
        }

        void Update () {
            if(rectTransform.rect.width >= maxWidthRectTransformWidth && sliderAndInputField.rectTransform.rect.width == maxSliderAndInputFieldWidth){
                return;
            }
            if(rectTransform.rect.width <= minWidthRectTransformWidth && sliderAndInputField.rectTransform.rect.width == minSliderAndInputFieldWidth){
                return;
            }
            float lerpFactor = (rectTransform.rect.width - minWidthRectTransformWidth) / (maxWidthRectTransformWidth - minWidthRectTransformWidth);
            float targetWidth = Mathf.Lerp(minSliderAndInputFieldWidth, maxSliderAndInputFieldWidth, lerpFactor);
            sliderAndInputField.rectTransform.SetSizeDeltaX(targetWidth);
            m_label.rectTransform.SetToFillWithMargins(0f, labelRightMargin, 0f, 0f);
        }

        public void Initialize (ShaderProperty prop, System.Action<float> onValueChanged, System.Func<float, string> formatString, float scrollMultiplier = 1f) {
            Initialize(
                labelText: prop.niceName,
                initValue: prop.defaultValue,
                initMin: prop.minValue,
                initMax: prop.maxValue,
                clampInputField: true,
                onValueChanged: onValueChanged,
                formatString: formatString,
                scrollMultiplier: scrollMultiplier
            );
            this.initProperty = prop;
        }

        public void Initialize (string labelText, float initValue, float initMin, float initMax, bool clampInputField, System.Action<float> onValueChanged, System.Func<float, string> formatString, float scrollMultiplier = 1f) {
            m_label.text = labelText;
            sliderAndInputField.SetSliderRange(initMin, initMax);
            sliderAndInputField.currentValue = initValue;
            sliderAndInputField.formatString = formatString;
            sliderAndInputField.onValueUpdated += onValueChanged;
            sliderAndInputField.clampInputFieldValues = clampInputField;
            sliderAndInputField.inputFieldScrollMultiplier = scrollMultiplier;
        }

    }

}
