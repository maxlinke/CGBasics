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
            ApplyLoadedColorsToTogglesAndButtons();
        }
        
    }

}