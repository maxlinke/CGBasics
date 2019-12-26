using UnityEngine;

namespace MatrixScreenUtils {

    public class MatrixScreenWindowOverlay : WindowOverlay {

        public override void LoadColors (ColorScheme cs) {
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
        
    }

}