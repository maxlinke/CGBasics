using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MatrixScreenUtils {

    public class MatrixWindowOverlay : WindowOverlay {

        private const string resetWarning = "(Resets matrix view)";

        private bool initialized = false;

        public Toggle glToggle { get; private set; }
        public Toggle orthoToggle { get; private set; }
        public Toggle vectorToggle { get; private set; }

        public void Initialize (MatrixScreen matrixScreen, bool glInit, System.Action<bool> onGLToggled, bool orthoInit, System.Action<bool> onOrthoToggled, bool vectorInit, System.Action<bool> onVectorToggled) {
            toggles = new List<Toggle>();
            toggleBackgrounds = new List<Image>();
            toggleIcons = new List<Image>();

            int toggleIndex = 0;
            windowDresser.Begin(uiParent, new Vector2(1, 1), new Vector2(0, -1), new Vector2(0, 0));
            glToggle = CreateSpecialToggle(
                ref toggleIndex, 
                icon: UISprites.MatrixScreenGL, 
                toggleName: "OpenGLMode", 
                hoverMessage: $"Toggle Open GL Mode {resetWarning}", 
                onStateChange: onGLToggled,
                initialState: glInit, 
                offsetAfter: false, 
                invokeStateChange: false
            );
            orthoToggle = CreateSpecialToggle(
                ref toggleIndex, 
                icon: UISprites.MatrixScreenOrtho, 
                toggleName: "OrthoMode", 
                hoverMessage: $"Toggles between orthographic and perspective projection {resetWarning}", 
                onStateChange: onOrthoToggled,
                initialState: orthoInit, 
                offsetAfter: false, 
                invokeStateChange: false
            );
            vectorToggle = CreateSpecialToggle(
                ref toggleIndex, 
                icon: UISprites.MatrixScreenVector, 
                toggleName: "VectorMode", 
                hoverMessage: $"Disables the mesh and instead shows a configurable vector", 
                onStateChange: onVectorToggled,
                initialState: vectorInit, 
                offsetAfter: false, 
                invokeStateChange: false
            );
            windowDresser.End();
            CreateResetButtonAndLabel("Matrix View", "Resets the view", matrixScreen.PanAndZoomController.ResetView);

            initialized = true;
        }

        public override void LoadColors (ColorScheme cs) {
            if(!initialized){
                return;
            }
            base.LoadColors(cs);
        }

    }

}