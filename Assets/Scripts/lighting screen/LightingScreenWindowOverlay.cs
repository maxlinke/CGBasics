namespace LightingModels {

    public class LightingScreenWindowOverlay : WindowOverlay {
        
        public override void LoadColors (ColorScheme cs) {
            label.color = cs.LSWOLabel;
            labelDropShadow.color = cs.LSWODropShadow;
            buttonIconActive = cs.LSWOButtonIconActive;
            buttonIconInactive = cs.LSWOButtonIconInactive;
            buttonBackgroundActive = cs.LSWOButtonBackgroundActive;
            buttonBackgroundInactive = cs.LSWOButtonBackgroundInactive;
            buttonHover = cs.LSWOButtonHover;
            buttonClick = cs.LSWOButtonClick;
            ApplyLoadedColorsToTogglesAndButtons();
        }

    }

}