using UnityEngine;
using UnityEngine.UI;

namespace LightingModels {

    public class RenderViewWindowOverlay : LightingScreenWindowOverlay {

        public Toggle bgIsAmientColorToggle { get; private set; }

        public void Initialize (System.Action onResetButtonClicked, bool initialAmbientToggleState) {
            InitializeLists();
            CreateResetButtonAndLabel("Render View", "Resets the camera", onResetButtonClicked);
            
            var toggleIndex = 0;
            windowDresser.Begin(uiParent, new Vector2(1, 1), new Vector2(0, -1), new Vector2(0, 0));
            bgIsAmientColorToggle = CreateSpecialToggle(
                toggleIndex: ref toggleIndex,
                icon: UISprites.LSRenderAmbientLight,
                toggleName: "Ambient Toggle", 
                hoverMessage: "Toggles whether the background should be the ambient color",
                onStateChange: null,
                initialState: initialAmbientToggleState,
                offsetAfter: false,
                invokeStateChange: false
            );
            windowDresser.End();
        }
        
    }

}