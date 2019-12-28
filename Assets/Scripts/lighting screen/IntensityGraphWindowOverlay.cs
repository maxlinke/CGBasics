namespace LightingModels {

    public class IntensityGraphWindowOverlay : LightingScreenWindowOverlay {

        public void Initialize (string resetButtonHoverMessage, System.Action onResetButtonClicked, System.Action<bool> onCirleToggleToggled) {
            InitializeLists();
            CreateResetButtonAndLabel("Intensity Graph", resetButtonHoverMessage, onResetButtonClicked);
        }
        
    }

}
