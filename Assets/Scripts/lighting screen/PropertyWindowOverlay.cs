namespace LightingModels {

    public class PropertyWindowOverlay : LightingScreenWindowOverlay {

        bool initialized = false;

        public void Initialize (System.Action onResetButtonClicked) {
            InitializeLists();
            CreateResetButtonAndLabel("Properties", "Reset all properties to their default values", onResetButtonClicked);
            this.initialized = true;
        }

        public void SetHeaderShown (bool value) {
            if(!initialized){
                return;
            }
            labelGOActive = value;
            resetButtonGOActive = value;
        }
        
    }

}