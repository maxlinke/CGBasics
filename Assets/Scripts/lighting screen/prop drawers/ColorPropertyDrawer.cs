using UnityEngine;
using UnityEngine.UI;

namespace LightingModels {

    public class ColorPropertyDrawer : UIPropertyDrawer {

        [SerializeField] Button colorPickerOpenButton;
        [SerializeField] Image colorDisplay;
        [SerializeField] Image colorDisplayDropShadow;

        bool initialized = false;

        public override void LoadColors (ColorScheme cs) {
            // TODO color loading
        }

        public void Initialize (ShaderProperty prop, System.Action<Color> onColorChanged) {
            Initialize(
                labelText: prop.niceName,
                initColor: prop.defaultColor,
                onColorChanged: onColorChanged
            );
        }

        public void Initialize (string labelText, Color initColor, System.Action<Color> onColorChanged) {
            if(initialized){
                Debug.LogError("duplicate init call, aborting!");
                return;
            }
            m_label.text = labelText;
            colorDisplay.color = initColor;
            colorPickerOpenButton.onClick.AddListener(ButtonClicked);
            initialized = true;

            void ButtonClicked () {
                ColorPicker.Open(
                    initColor: colorDisplay.color,
                    includeAlpha: false,
                    onClose: (c) => {
                        colorDisplay.color = c;
                        onColorChanged?.Invoke(c);
                    }, whileOpen: (c) => {
                        colorDisplay.color = c;
                        onColorChanged?.Invoke(c);
                    }
                );
            }
        }

    }

}