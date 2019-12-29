using UnityEngine;
using UnityEngine.UI;

namespace LightingModels {

    public class IntensityGraphWindowOverlay : WindowOverlay {

        public Toggle planarModeToggle { get; private set; }
        public Toggle concentricLineToggle { get; private set; }
        public Toggle gizmoToggle { get; private set; }

        public void Initialize (string resetButtonHoverMessage, System.Action onResetButtonClicked) {
            InitializeLists();
            CreateResetButtonAndLabel("Intensity Graph (first light only)", resetButtonHoverMessage, onResetButtonClicked);
            windowDresser.Begin(uiParent, Vector2.one, new Vector2(0, -1), Vector2.zero);
            planarModeToggle = CreateSpecialToggle(UISprites.LSPlanarModeToggle, "Planar", "Activates planar mode, where the normal is fixed and the view direction is the pixel direction", null, true, false, false);
            concentricLineToggle = CreateSpecialToggle(UISprites.LSConcentricCircles, "Circles", "Toggles the concentric reference circles", null, true, false, false);
            gizmoToggle = CreateSpecialToggle(UISprites.UIInfo, "Gizmos", "Toggles drawing the gizmos", null, true, false, false);
            windowDresser.End();
        }
        
    }

}
