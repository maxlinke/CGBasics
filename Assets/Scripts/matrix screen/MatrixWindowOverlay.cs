using UnityEngine;
using UnityEngine.UI;

namespace MatrixScreenUtils {

    public class MatrixWindowOverlay : WindowOverlay {

        private const string resetWarning = "(Resets matrix view)";

        private bool initialized = false;

        public Button neutralZoomButton { get; private set; }
        public Toggle glToggle { get; private set; }
        public Toggle orthoToggle { get; private set; }
        public Toggle vectorToggle { get; private set; }

        public void Initialize (MatrixScreen matrixScreen, bool glInit, System.Action<bool> onGLToggled, bool orthoInit, System.Action<bool> onOrthoToggled, bool vectorInit, System.Action<bool> onVectorToggled) {
            InitializeLists();
            windowDresser.Begin(uiParent, new Vector2(1, 1), new Vector2(0, -1), new Vector2(0, 0));
            neutralZoomButton = CreateSpecialButton(
                icon: UISprites.UIOneOne,
                buttonName: "1:1 Scale",
                hoverMessage: "Set the zoom level to 1",
                onClick: matrixScreen.PanAndZoomController.SetNeutralZoom,
                offsetAfter: true
            );
            glToggle = CreateSpecialToggle(
                icon: UISprites.MatrixScreenGL, 
                toggleName: "OpenGLMode", 
                hoverMessage: $"Toggle Open GL Mode {resetWarning}", 
                onStateChange: onGLToggled,
                initialState: glInit, 
                offsetAfter: false, 
                invokeStateChange: false
            );
            orthoToggle = CreateSpecialToggle(
                icon: UISprites.MatrixScreenOrtho, 
                toggleName: "OrthoMode", 
                hoverMessage: $"Toggles between orthographic and perspective projection {resetWarning}", 
                onStateChange: onOrthoToggled,
                initialState: orthoInit, 
                offsetAfter: false, 
                invokeStateChange: false
            );
            vectorToggle = CreateSpecialToggle(
                icon: UISprites.MatrixScreenVector, 
                toggleName: "VectorMode", 
                hoverMessage: $"Disables the mesh and instead shows a configurable vector", 
                onStateChange: onVectorToggled,
                initialState: vectorInit, 
                offsetAfter: false, 
                invokeStateChange: false
            );
            windowDresser.End();
            CreateResetButtonAndLabel("Matrix View", "Reset the view and matrices", () => {
                if(matrixScreen.FreeMode){
                    matrixScreen.FreeModeToggle.isOn = false;
                }else{
                    matrixScreen.ActivateNonFreeMode();
                }
                matrixScreen.PanAndZoomController.ResetView();
            });

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