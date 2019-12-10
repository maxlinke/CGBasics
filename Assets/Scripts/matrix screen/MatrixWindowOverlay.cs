using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MatrixScreenUtils {

    public class MatrixWindowOverlay : WindowOverlay {

        private bool initialized = false;

        public void Initialize (MatrixScreen matrixScreen) {
            toggles = new List<Toggle>();
            toggleBackgrounds = new List<Image>();
            toggleIcons = new List<Image>();

            int toggleIndex = 0;
            windowDresser.Begin(uiParent, new Vector2(1, 1), new Vector2(0, -1), new Vector2(0, 0));
            CreateSpecialToggle(ref toggleIndex, UISprites.UITemp, "OpenGLMode", "Toggle Open GL Mode (Resets matrix view)", (b) => {matrixScreen.OpenGLMode = b;}, matrixScreen.OpenGLMode, false, invokeStateChange: false);
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