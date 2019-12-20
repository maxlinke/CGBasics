using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LightingModels {

    public class PropGroup : MonoBehaviour {

        [Header("Components")]
        [SerializeField] RectTransform m_rectTransform;
        [SerializeField] RectTransform headerArea;
        [SerializeField] RectTransform contentArea;
        [SerializeField] TextMeshProUGUI header;
        [SerializeField] TextMeshProUGUI bottomText;
        [SerializeField] Image bottomImage;

        [Header("Settings")]
        [SerializeField] float buttonSize;
        [SerializeField] float headerTextLeftMargin;
        [SerializeField] float spaceBetweenPropsAndBottomThings;
        [SerializeField] float spaceBetweenBottomTextAndImage;
        
        public RectTransform rectTransform => m_rectTransform;
        
        bool initialized = false;
        Button configButton;
        Image configButtonIcon;
        List<UIPropertyField> propfields;

        void Update () {
            // eh.. if the images are not too wide, it shouldn't be a problem on most devices. 480x640 is really low either way
            // if(bottomImage.gameObject.activeSelf && bottomImage.sprite != null){
            //     var tex = bottomImage.sprite.texture;
            //     var texDimensions = new Vector2(tex.width, tex.height);
            //     bool bottomImageFits = bottomImage.sprite.texture.width <= contentArea.rect.width;
            //     bool bottomImageSetToFill = bottomImage.rectTransform.AverageAnchor() != bottomImage.rectTransform.anchorMin;
            //     if(bottomImageFits && bottomImageSetToFill){
            //         var newAnchor = new Vector2(0.5f, 0f);
            //         bottomImage.rectTransform.SetAnchor(newAnchor);
            //         bottomImage.rectTransform.pivot = newAnchor;
            //         bottomImage.rectTransform.sizeDelta = texDimensions;
            //     }else if(!bottomImageFits && !bottomImageSetToFill){

            //     }
            // }
        }

        public void LoadColors (ColorScheme cs) {

        }
        
        // public void Initialize (string initName, IEnumerable<System.Func<) { // func prop, gameobject?
        public void Initialize (string initHeader, bool rebuildContent = true) {
            if(initialized){
                Debug.LogError("already initialized! aborting!", this.gameObject);
                return;
            }
            header.rectTransform.SetToFillWithMargins(0f, 0f, 0f, headerTextLeftMargin);
            header.text = initHeader;
            bottomImage.SetGOActive(false);
            bottomText.SetGOActive(false);
            propfields = new List<UIPropertyField>();
            this.initialized = true;
            ConditionalRebuildContent(rebuildContent);
        }

        bool NotYetInitAbort () {
            if(!initialized){
                Debug.LogError("Not yet initialized! Aborting...");
            }
            return !initialized;
        }

        public void SetName (string newName) {
            if(NotYetInitAbort()){
                return;
            }
            if(newName == null){
                Debug.LogError("Name can't be null!");
                return;
            }
            newName = newName.Trim();
            if(newName.Length < 1){
                Debug.LogError("Name can't be empty!");
                return;
            }
            header.text = newName;
        }

        public void RebuildContent () {
            if(NotYetInitAbort()){
                return;
            }
            var activeCache = gameObject.activeSelf;
            gameObject.SetActive(true);
            if(!gameObject.activeInHierarchy){
                Debug.LogWarning($"{nameof(PropGroup)} \"{gameObject.name}\" is not active in hierarchy! Heights of TMPs might be off!", this.gameObject);
            }
            float y = 0;
            foreach(var field in propfields){
                if(field.gameObject.activeSelf){
                    field.rectTransform.SetAnchoredPositionY(0);
                    y -= field.rectTransform.rect.height;
                }
            }
            var textActive = bottomText.gameObject.activeSelf;
            var imgActive = bottomImage.gameObject.activeSelf;
            if(textActive || imgActive){
                y -= spaceBetweenPropsAndBottomThings;
            }
            if(textActive){
                bottomText.rectTransform.SetAnchorAndPivot(0.5f, 1f);
                bottomText.rectTransform.SetAnchoredPosition(0, y);
                bottomText.rectTransform.SetSizeDeltaY(bottomText.preferredHeight);
                y -= bottomText.rectTransform.rect.height;
                if(imgActive){
                    y -= spaceBetweenBottomTextAndImage;
                }
            }
            if(imgActive){
                bottomImage.rectTransform.SetAnchorAndPivot(0.5f, 1f);
                bottomImage.rectTransform.SetAnchoredPosition(0, y);
                if(bottomImage.sprite != null){
                    var imgTex = bottomImage.sprite.texture;
                    bottomImage.rectTransform.SetSizeDelta(imgTex.width, imgTex.height);
                }
                y -= bottomImage.rectTransform.rect.height;
            }
            rectTransform.SetSizeDeltaY(headerArea.rect.height + Mathf.Abs(y));
            gameObject.SetActive(activeCache);
        }

        void ConditionalRebuildContent (bool rebuildContent) {
            if(rebuildContent){
                RebuildContent();
            }
        }

        public Button AddConfigButton (Sprite icon, System.Action onButtonClicked, string hoverMessage) {
            if(NotYetInitAbort()){
                return null;
            }
            if(configButton != null){
                Debug.LogError("Asked to add button, but there was already one! Aborting...");
                return null;
            }
            // main rectTransform. this is a lot of repeated code, i know but maaayyybbeeee i want to change it up later... maybe...
            var newBtnRT = new GameObject("Config Button", typeof(RectTransform), typeof(Button), typeof(Image)).GetComponent<RectTransform>();
            newBtnRT.SetParent(headerArea, false);
            newBtnRT.ResetLocalScale();
            newBtnRT.SetAnchor(1f, 0.5f);
            newBtnRT.pivot = newBtnRT.AverageAnchor();
            newBtnRT.sizeDelta = Vector2.one * buttonSize;
            newBtnRT.anchoredPosition = new Vector2(-1f * ((headerArea.rect.height - buttonSize) / 2f), 0f);
            // background image
            var newBtnBG = newBtnRT.GetComponent<Image>();
            newBtnBG.sprite = UISprites.UICircle;
            newBtnBG.color = Color.white;
            newBtnBG.raycastTarget = true;
            // the actual button
            configButton = newBtnRT.GetComponent<Button>();
            configButton.onClick.AddListener(() => {onButtonClicked?.Invoke();});
            // potential hover message
            if(hoverMessage != null){
                newBtnRT.gameObject.AddComponent<UIHoverEventCaller>().SetActions(
                    (ped) => {if(configButton.interactable) BottomLog.DisplayMessage(hoverMessage);},
                    (ped) => {if(configButton.interactable) BottomLog.ClearDisplay();}
                );
            }
            // icon rt
            var iconRT = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            iconRT.SetParent(newBtnRT, false);
            iconRT.ResetLocalScale();
            iconRT.SetToFill();
            // the actual icon
            configButtonIcon = iconRT.GetComponent<Image>();
            configButtonIcon.sprite = icon;
            configButtonIcon.raycastTarget = false;
            // spacing the header
            header.rectTransform.SetToFillWithMargins(0f, headerArea.rect.height, 0f, headerTextLeftMargin);
            // output
            return configButton;
        }

        public GameObject AddSliderProperty () {
            if(NotYetInitAbort()){
                return null;
            }
            return null;
        }

        public GameObject AddColorProperty () {
            if(NotYetInitAbort()){
                return null;
            }
            return null;
        }

        public void ShowImage (Sprite sprite, bool rebuildContent = true) {
            if(NotYetInitAbort()){
                return;
            }
            bottomImage.SetGOActive(true);
            bottomImage.sprite = sprite;
            ConditionalRebuildContent(rebuildContent);
        }

        public void HideImage (bool rebuildContent = true) {
            if(NotYetInitAbort()){
                return;
            }
            bottomImage.SetGOActive(false);
            ConditionalRebuildContent(rebuildContent);
        }

        public void ShowText (string textToShow, bool rebuildContent = true) {
            if(NotYetInitAbort()){
                return;
            }
            bottomText.SetGOActive(true);
            bottomText.text = textToShow;
            ConditionalRebuildContent(rebuildContent);
        }

        public void HideText (bool rebuildContent = true) {
            if(NotYetInitAbort()){
                return;
            }
            bottomText.SetGOActive(false);
            ConditionalRebuildContent(rebuildContent);
        }
    
    }

}