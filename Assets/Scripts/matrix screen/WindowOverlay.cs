using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MatrixScreenUtils {

    public abstract class WindowOverlay : MonoBehaviour {

        [SerializeField] protected WindowDresser windowDresser;
        [SerializeField] protected RectTransform uiParent;

        protected Color buttonIconActive;
        protected Color buttonIconInactive;
        protected Color buttonBackgroundActive;
        protected Color buttonBackgroundInactive;
        protected Color buttonHover;
        protected Color buttonClick;

        protected List<Toggle> toggles;
        protected List<Image> toggleBackgrounds;
        protected List<Image> toggleIcons;

        protected Button resetButton;
        protected Image resetButtonBackground;
        protected Image resetButtonIcon;

        protected TextMeshProUGUI label;
        protected TextMeshProUGUI labelDropShadow;

        public string labelText {
            get {
                return label.text;
            } set {
                label.text = value;
                labelDropShadow.text = value;
            }
        }

        public bool resetButtonEnabled {
            get {
                return resetButton.interactable;
            } set {
                resetButton.interactable = value;
                SetColorsForActiveState(resetButtonBackground, resetButtonIcon, value);
                OnResetButtonActiveStateChanged(value);
            }
        }

        public virtual void LoadColors (ColorScheme cs) {
            label.color = cs.MatrixWindowLabel;
            labelDropShadow.color = cs.MatrixWindowLabelDropShadow;
            buttonIconActive = cs.MatrixWindowButtonIconActive;
            buttonIconInactive = cs.MatrixWindowButtonIconInactive;
            buttonBackgroundActive = cs.MatrixWindowButtonBackgroundActive;
            buttonBackgroundInactive = cs.MatrixWindowButtonBackgroundInactive;
            buttonHover = cs.MatrixWindowButtonHover;
            buttonClick = cs.MatrixWindowButtonClick;
            for(int i=0; i<toggles.Count; i++){
                toggles[i].SetFadeTransition(0f, Color.white, buttonHover, buttonClick, Color.magenta);
                SetColorsForActiveState(toggleBackgrounds[i], toggleIcons[i], toggles[i].isOn);
            }
            resetButton.SetFadeTransition(0f, Color.white, buttonHover, buttonClick, Color.white);
            SetColorsForActiveState(resetButtonBackground, resetButtonIcon, resetButton.interactable);
        }

        protected void SetColorsForActiveState (Image backgroundImage, Image iconImage, bool activeState) {
            if(activeState){
                backgroundImage.color = buttonBackgroundActive;
                iconImage.color = buttonIconActive;
            }else{
                backgroundImage.color = buttonBackgroundInactive;
                iconImage.color = buttonIconInactive;
            }
        }

        protected void SetToggleColors (int toggleIndex) {
            SetColorsForActiveState(toggleBackgrounds[toggleIndex], toggleIcons[toggleIndex], toggles[toggleIndex].isOn);
        }

        protected virtual void CreateResetButtonAndLabel (string initialLabelText, string hoverMessage, System.Action onResetButtonClicked) {
            windowDresser.Begin(uiParent, new Vector2(0, 1), new Vector2(1, 0), Vector2.zero);
            // the reset button
            var resetRT = windowDresser.CreateCircleWithIcon(UISprites.UIReset, "Reset", hoverMessage, out resetButtonIcon, out resetButtonBackground);
            resetRT.gameObject.AddComponent<Button>();
            resetButton = resetRT.GetComponent<Button>();
            resetButton.targetGraphic = resetButtonBackground;
            resetButton.onClick.AddListener(() => { onResetButtonClicked.Invoke(); });
            // the label
            label = windowDresser.CreateLabel();
            labelDropShadow = Instantiate(label, label.rectTransform.parent);
            labelDropShadow.rectTransform.SetSiblingIndex(label.rectTransform.GetSiblingIndex());
            labelDropShadow.rectTransform.anchoredPosition += new Vector2(1, -1);
            labelText = initialLabelText;
            windowDresser.End();
        }

        protected virtual Toggle CreateSpecialToggle (ref int toggleIndex, Sprite icon, string toggleName, string hoverMessage, System.Action<bool> onStateChange, bool initialState, bool offsetAfter = false, bool invokeStateChange = true) {
            // setting up position and looks
            var newToggleRT = windowDresser.CreateCircleWithIcon(icon, toggleName, hoverMessage, out var newToggleIcon, out var newToggleBackground, offsetAfter);
            // setting up the actual toggle
            newToggleRT.gameObject.AddComponent(typeof(Toggle));
            var newToggle = newToggleRT.GetComponent<Toggle>();
            newToggle.targetGraphic = newToggleBackground;
            newToggle.isOn = initialState;
            var indexCopy = toggleIndex;
            newToggle.onValueChanged.AddListener((newVal) => {
                SetToggleColors(indexCopy);
                onStateChange?.Invoke(newVal);
            });
            if(invokeStateChange){
                onStateChange?.Invoke(initialState);
            }
            // saving to the lists, updating index
            toggles.Add(newToggle);
            toggleBackgrounds.Add(newToggleBackground);
            toggleIcons.Add(newToggleIcon);
            toggleIndex++;
            // output
            return newToggle;
        }

        protected virtual void OnResetButtonActiveStateChanged (bool value) { }
    
    }

}