﻿using UnityEngine;
using UnityEngine.UI;

namespace LightingModels {

    public class ColorPropertyField : UIPropertyField {

        [SerializeField] Button colorPickerOpenButton;
        [SerializeField] Image colorDisplay;
        [SerializeField] Image colorDisplayOutlineInside;
        [SerializeField] Image colorDisplayOutlineOutside;

        bool initialized = false;
        System.Action<Color> onColorChanged;
        Color initColor;

        public Color currentColor => colorDisplay.color;

        public override void LoadColors (ColorScheme cs) {
            base.LoadColors(cs);
            colorPickerOpenButton.SetFadeTransition(0f, Color.white, Color.white, Color.white, Color.magenta);
            colorDisplayOutlineInside.color = cs.LightingScreenColorPropOutlineInside;
            colorDisplayOutlineOutside.color = cs.LightingScreenColorPropOutlineOutside;
        }

        public void Initialize (ShaderProperty prop, System.Action<Color> onColorChanged) {
            Initialize(
                labelText: prop.niceName,
                initColor: prop.defaultColor,
                onColorChanged: onColorChanged
            );
            this.initProperty = prop;
        }

        public void Initialize (string labelText, Color initColor, System.Action<Color> onColorChanged) {
            if(initialized){
                Debug.LogError("duplicate init call, aborting!");
                return;
            }
            m_label.text = labelText;
            colorDisplay.color = initColor;
            colorPickerOpenButton.onClick.AddListener(ButtonClicked);
            this.onColorChanged = onColorChanged;
            this.initColor = initColor;
            initialized = true;

            void ButtonClicked () {
                ColorPicker.Open(
                    initColor: colorDisplay.color,
                    includeAlpha: false,
                    onClose: UpdateColor,
                    whileOpen: UpdateColor
                );
            }
        }

        public void UpdateColor (Color newColor) {
            colorDisplay.color = newColor;
            onColorChanged?.Invoke(newColor);
        }

        public override void ResetToDefault () {
            colorDisplay.color = initColor;
            onColorChanged?.Invoke(initColor);
        }

    }

}