﻿using UnityEngine;

namespace LightingModels {

    public class FloatPropertyField : UIPropertyField {

        [SerializeField] SyncedSliderAndInputField sliderAndInputField;
        [SerializeField] float minSliderAndInputFieldWidth;
        [SerializeField] float maxSliderAndInputFieldWidth;
        [SerializeField] float maxWidthRectTransformWidth;
        [SerializeField] float minWidthRectTransformWidth;
        [SerializeField] float labelRightMargin;

        float initMin;
        float initMax;
        float initValue;

        public override void LoadColors (ColorScheme cs) {
            base.LoadColors(cs);
            sliderAndInputField.sliderBG.color = cs.LightingScreenSliderBackground;
            sliderAndInputField.sliderFill.color = cs.LightingScreenSliderFill;
            sliderAndInputField.sliderHandle.color = Color.white;
            sliderAndInputField.slider.SetFadeTransition(0f, cs.LightingScreenSliderHandle, cs.LightingScreenSliderHandleHover, cs.LightingScreenSliderHandleClick, Color.magenta);
            sliderAndInputField.inputFieldPlaceholder.color = Color.clear;
            sliderAndInputField.inputFieldBG.color = Color.white;
            sliderAndInputField.inputFieldText.color = cs.LightingScreenInputFieldText;
            sliderAndInputField.inputField.SetFadeTransition(0f, cs.LightingScreenInputField, cs.LightingScreenInputFieldHover, cs.LightingScreenInputFieldClick, Color.magenta);
            sliderAndInputField.inputField.selectionColor = cs.LightingScreenInputFieldSelection;
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
            this.initMin = initMin;
            this.initMax = initMax;
            this.initValue = initValue;
        }

        public override void ResetToDefault () {
            sliderAndInputField.SetSliderRange(initMin, initMax);
            sliderAndInputField.currentValue = initValue;
        }

    }

}
