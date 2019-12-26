using UnityEngine;

namespace LightingModels {

    public class PropertyWindowOverlay : LightingScreenWindowOverlay {

        [SerializeField] float headerHideUnhideTime;

        bool initialized = false;
        [System.NonSerialized] public bool headerShouldBeVisible = true;
        float hiddenPosLerp = 0;
        float initialHeaderLabelY;
        float initialResetButtonY;
        float initialDropShadowY;
        float hiddenHeaderLabelY;
        float hiddenResetButtonY;
        float hiddenDropShadowY;

        public void Initialize (System.Action onResetButtonClicked) {
            InitializeLists();
            CreateResetButtonAndLabel("Properties", "Reset all properties to their default values", onResetButtonClicked);
            initialHeaderLabelY = label.rectTransform.anchoredPosition.y;
            initialResetButtonY = resetButtonRT.anchoredPosition.y;
            initialDropShadowY = labelDropShadow.rectTransform.anchoredPosition.y;
            var hideOffset = resetButtonRT.rect.width * 3;
            hiddenHeaderLabelY = initialHeaderLabelY + hideOffset;
            hiddenResetButtonY = initialResetButtonY + hideOffset;
            hiddenDropShadowY = initialDropShadowY + hideOffset;
            this.initialized = true;
        }

        void Update () {
            float deltaDir;
            if(headerShouldBeVisible){
                if(hiddenPosLerp == 0){
                    return;
                }
                deltaDir = -1;
            }else{
                if(hiddenPosLerp == 1){
                    return;
                }
                deltaDir = 1;
            }
            hiddenPosLerp = Mathf.Clamp01(hiddenPosLerp + (deltaDir * Time.deltaTime / headerHideUnhideTime));
            label.rectTransform.SetAnchoredPositionY(Mathf.Lerp(initialHeaderLabelY, hiddenHeaderLabelY, hiddenPosLerp));
            labelDropShadow.rectTransform.SetAnchoredPositionY(Mathf.Lerp(initialDropShadowY, hiddenDropShadowY, hiddenPosLerp));
            resetButtonRT.SetAnchoredPositionY(Mathf.Lerp(initialResetButtonY, hiddenResetButtonY, hiddenPosLerp));
        }
        
    }

}