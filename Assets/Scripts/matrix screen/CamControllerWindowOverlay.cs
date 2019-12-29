﻿using UnityEngine;
using UnityEngine.UI;

namespace MatrixScreenUtils {

    public class CamControllerWindowOverlay : WindowOverlay {

        const string renderCamLabelText = "Render View";
        const string externalCamLabelText = "External View";
        const string renderCamLockedSuffix = "(Locked, use the matrices or deactivate free mode)";

        private bool initialized = false;
        private CustomCameraUIController camController;
        private Toggle wireToggle;

        public Toggle WireToggle => wireToggle;

        public void Initialize (CustomCameraUIController camController) {
            this.camController = camController;
            InitializeLists();
            CreateRightSideToggles();
            var initLabelText = camController.IsExternalCamController ? externalCamLabelText : renderCamLabelText;
            CreateResetButtonAndLabel(initLabelText, "Resets the view", camController.ResetCamera);

            initialized = true;

            void CreateRightSideToggles () {
                var cam = camController.targetCam;
                var extCtrl = camController.IsExternalCamController;
                windowDresser.Begin(uiParent, new Vector2(1, 1), new Vector2(0, -1), Vector2.zero);
                wireToggle = CreateSpecialToggle(UISprites.MCamCtrlDrawWireframe, "Wireframe", "Toggles wireframe drawing", (b) => {
                    cam.drawObjectAsWireFrame = b;
                    camController.otherController.overlay.WireToggled(b);
                }, false, offsetAfter: true);
                CreateSpecialToggle(UISprites.MCamCtrlDrawFloor, "Grid", "Toggles drawing the grid floor", (b) => {cam.drawGridFloor = b;}, !extCtrl);
                CreateSpecialToggle(UISprites.MCamCtrlDrawOrigin, "Origin", "Toggles drawing the origin", (b) => {cam.drawOrigin = b;}, extCtrl);
                CreateSpecialToggle(UISprites.MCamCtrlDrawSeeThrough, "XRay", "Toggles see-through drawing for all wireframe gizmos", (b) => {cam.drawSeeThrough = b;}, false, offsetAfter: extCtrl);
                if(camController.IsExternalCamController){     
                    CreateSpecialToggle(UISprites.MCamCtrlDrawCamera, "Cam", "Toggles drawing the other camera", (b) => {cam.drawCamera = b;}, true);
                    CreateSpecialToggle(UISprites.MCamCtrlDrawClipBox, "ClipBox", "Toggles drawing the clip space area", (b) => {cam.drawClipSpace = b;}, true);
                    CreateSpecialToggle(UISprites.MCamCtrlShowCulling, "ShowClip", "Toggles culling visualization", (b) => {cam.showClipping = b;}, true);
                }
                windowDresser.End();
            }

        }

        void WireToggled (bool newVal) {
            if(wireToggle == null || wireToggle.isOn == newVal){
                return;
            }
            wireToggle.isOn = newVal;
        }

        public override void LoadColors (ColorScheme cs) {
            if(!initialized){
                return;
            }
            base.LoadColors(cs);
        }

        protected override void OnResetButtonActiveStateChanged (bool value) {
            base.OnResetButtonActiveStateChanged(value);
            if(!camController.IsExternalCamController){
                labelText = $"{renderCamLabelText}{(value ? string.Empty : $" {renderCamLockedSuffix}")}";
            }
        }
    
    }

}