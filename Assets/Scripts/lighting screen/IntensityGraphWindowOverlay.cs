﻿using UnityEngine;
using UnityEngine.UI;

namespace LightingModels {

    public class IntensityGraphWindowOverlay : LightingScreenWindowOverlay {

        public Toggle planarModeToggle { get; private set; }
        public Toggle concentricLineToggle { get; private set; }

        public void Initialize (string resetButtonHoverMessage, System.Action onResetButtonClicked) {
            InitializeLists();
            CreateResetButtonAndLabel("Intensity Graph (first light only)", resetButtonHoverMessage, onResetButtonClicked);
            windowDresser.Begin(uiParent, Vector2.one, new Vector2(0, -1), Vector2.zero);
            int tInd = 0;
            planarModeToggle = CreateSpecialToggle(ref tInd, UISprites.LSPlanarModeToggle, "Planar", "Activates planar mode, where the normal is fixed and the view direction is the pixel direction", null, true, false, false);
            concentricLineToggle = CreateSpecialToggle(ref tInd, UISprites.LSConcentricCircles, "Circles", "Toggles the concentric reference circles", null, true, false, false);
            windowDresser.End();
        }
        
    }

}
