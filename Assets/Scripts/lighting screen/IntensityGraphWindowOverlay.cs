using UnityEngine;
using UnityEngine.UI;

namespace LightingModels {

    public class IntensityGraphWindowOverlay : LightingScreenWindowOverlay {

        public Toggle concentricLineToggle { get; private set; } 

        public void Initialize (string resetButtonHoverMessage, System.Action onResetButtonClicked) {
            InitializeLists();
            CreateResetButtonAndLabel("Intensity Graph", resetButtonHoverMessage, onResetButtonClicked);
            windowDresser.Begin(uiParent, Vector2.one, new Vector2(0, -1), Vector2.zero);
            int tInd = 0;
            concentricLineToggle = CreateSpecialToggle(ref tInd, UISprites.LSConcentricCircles, "Circles", "Toggles the concentric reference circles", null, true, false, false);
            windowDresser.End();
        }
        
    }

}
