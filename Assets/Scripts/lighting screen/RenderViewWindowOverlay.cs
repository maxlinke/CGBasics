namespace LightingModels {

    public class RenderViewWindowOverlay : LightingScreenWindowOverlay {

        public void Initialize (System.Action onResetButtonClicked) {
            InitializeLists();
            CreateResetButtonAndLabel("Render View", "Resets the camera", onResetButtonClicked);
        }
        
    }

}