using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LightingModels {

    public class UIPropertyGroup : MonoBehaviour {

        [Header("Prefabs")]
        [SerializeField] ColorPropertyField colorFieldPrefab;
        [SerializeField] FloatPropertyField floatFieldPrefab; 

        [Header("Components")]
        [SerializeField] RectTransform m_rectTransform;
        [SerializeField] RectTransform headerArea;
        [SerializeField] RectTransform contentArea;
        [SerializeField] TextMeshProUGUI header;
        [SerializeField] TextMeshProUGUI headerDropShadow;
        [SerializeField] TextMeshProUGUI bottomText;
        [SerializeField] Image bottomImage;

        [Header("Settings")]
        [SerializeField] float buttonSize;
        [SerializeField] float headerTextLeftMargin;
        [SerializeField] float spaceBetweenPropsAndBottomThings;
        [SerializeField] float spaceBetweenBottomTextAndImage;
        
        public RectTransform rectTransform => m_rectTransform;
        
        bool initialized = false;
        List<(Selectable selectable, Image icon)> headerSelectables;
        List<UIPropertyField> propFields;
        Color toggleOnColor;
        Color toggleOffColor;

        bool m_bottomImageShouldBeShown;
        bool m_forceHideBottomImage;
        public bool forceHideBottomImage {
            get {
                return m_forceHideBottomImage;
            } set {
                m_forceHideBottomImage = value;
                UpdateBottomImageVisibility();
            }
        }

        bool m_bottomTextShouldBeShown;
        bool m_forceShowBottomText;
        public bool forceShowBottomText {
            get {
                return m_forceShowBottomText;
            } set {
                m_forceShowBottomText = value;
                UpdateBottomTextVisibility();
            }
        }


        public IEnumerator<UIPropertyField> GetEnumerator () {
            foreach(var propField in propFields){
                yield return propField;
            }
        }

        public void LoadColors (ColorScheme cs) {
            header.color = cs.LightingScreenPropGroupHeaders;
            headerDropShadow.color = cs.LightingScreenDropShadows;
            bottomText.color = cs.LightingScreenPropGroupBottomText;
            bottomImage.color = cs.LightingScreenPropGroupBottomImage;
            toggleOnColor = cs.LightingScreenButtonIcon;
            toggleOffColor = toggleOnColor * new Color(1, 1, 1, 0.333f);
            for(int i=0; i<headerSelectables.Count; i++){
                var hs = headerSelectables[i].selectable;
                var hsi = headerSelectables[i].icon;
                hs.targetGraphic.color = Color.white;
                hs.SetFadeTransition(0f, cs.LightingScreenButton, cs.LightingScreenButtonHover, cs.LightingScreenButtonClick, Color.magenta);
                if(hs is Toggle hsToggle){
                    hsi.color = hsToggle.isOn ? toggleOnColor : toggleOffColor;
                }else{
                    hsi.color = cs.LightingScreenButtonIcon;
                }
            }
            foreach(var propField in propFields){
                propField.LoadColors(cs);
            }
        }
        
        public void Initialize (string initHeader, bool rebuildContent = true) {
            if(initialized){
                Debug.LogError("already initialized! aborting!", this.gameObject);
                return;
            }
            propFields = new List<UIPropertyField>();
            headerSelectables = new List<(Selectable selectable, Image icon)>();
            RebuildHeader();
            header.text = initHeader;
            headerDropShadow.text = header.text;
            headerDropShadow.rectTransform.MatchOther(header.rectTransform);
            headerDropShadow.rectTransform.anchoredPosition += new Vector2(1, -1);
            bottomImage.SetGOActive(false);
            bottomText.SetGOActive(false);
            bottomImage.sprite = null;
            bottomText.text = string.Empty;
            this.m_forceHideBottomImage = false;
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
            headerDropShadow.text = newName;
        }

        public void RebuildContent () {
            if(NotYetInitAbort()){
                return;
            }
            var activeCache = gameObject.activeSelf;
            gameObject.SetActive(true);
            if(!gameObject.activeInHierarchy){
                Debug.LogWarning($"{nameof(UIPropertyGroup)} \"{gameObject.name}\" is not active in hierarchy! Heights of TMPs might be off!", this.gameObject);
            }
            float y = 0;
            foreach(var field in propFields){
                if(field.gameObject.activeSelf){
                    field.rectTransform.SetAnchoredPositionY(y);
                    y -= field.rectTransform.rect.height;
                }
            }
            var textActive = bottomText.gameObject.activeSelf;
            var imgActive = bottomImage.gameObject.activeSelf;
            if(textActive || imgActive){
                y -= spaceBetweenPropsAndBottomThings;
            }
            if(textActive){
                bottomText.rectTransform.anchorMin = new Vector2(0f, 1f);
                bottomText.rectTransform.anchorMax = new Vector2(1f, 1f);
                bottomText.rectTransform.SetPivot(0.5f, 1f);
                bottomText.rectTransform.SetAnchoredPosition(0, y);
                bottomText.ForceMeshUpdate();
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
                    var widthRatio = contentArea.rect.width / imgTex.width;
                    var imgSizeDelta = new Vector2(imgTex.width, imgTex.height) * Mathf.Clamp01(widthRatio);
                    bottomImage.rectTransform.sizeDelta = imgSizeDelta;
                }
                y -= bottomImage.rectTransform.rect.height;
            }
            rectTransform.SetSizeDeltaY(headerArea.rect.height + Mathf.Abs(y));
            RebuildHeader();
            gameObject.SetActive(activeCache);
        }

        void RebuildHeader () {
            int i = 0;
            float offset = headerArea.rect.height;
            foreach(var hs in headerSelectables){
                var hsRT = hs.selectable.GetComponent<RectTransform>();
                hsRT.pivot = 0.5f * Vector2.one;
                hsRT.SetAnchor(1f, 0.5f);
                hsRT.sizeDelta = Vector2.one * buttonSize;
                hsRT.anchoredPosition = new Vector2(-(i + 0.5f) * offset, 0f);
                i++;
            }
            header.rectTransform.SetToFillWithMargins(0f, i * offset, 0f, headerTextLeftMargin);
            headerDropShadow.rectTransform.MatchOther(header.rectTransform);
            headerDropShadow.rectTransform.anchoredPosition += new Vector2(1, -1);
        }

        void ConditionalRebuildContent (bool rebuildContent) {
            if(rebuildContent){
                RebuildContent();
            }
        }

        Selectable AddHeaderSelectable (Sprite icon, System.Func<GameObject, (Image bg, Image icon), Selectable> setupSelectable, string hoverMessage) {
            if(NotYetInitAbort()){
                return null;
            }
            // main rectTransform. this is a lot of repeated code, i know but maaayyybbeeee i want to change it up later... maybe...
            var newSelectableRT = new GameObject("Header Selectable", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            newSelectableRT.SetParent(headerArea, false);
            newSelectableRT.ResetLocalScale();
            // background image
            var newSelectableBG = newSelectableRT.GetComponent<Image>();
            newSelectableBG.sprite = UISprites.UICircle;
            newSelectableBG.color = Color.white;
            newSelectableBG.raycastTarget = true;
            // icon rt
            var iconRT = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            iconRT.SetParent(newSelectableRT, false);
            iconRT.ResetLocalScale();
            iconRT.SetToFill();
            // the actual icon
            var newSelectableIcon = iconRT.GetComponent<Image>();
            newSelectableIcon.sprite = icon;
            newSelectableIcon.raycastTarget = false;
            // the actual selectable
            var newSelectable = setupSelectable(newSelectableRT.gameObject, (bg: newSelectableBG, icon: newSelectableIcon));
            // potential hover message
            if(hoverMessage != null && hoverMessage.Length > 0){
                newSelectableRT.gameObject.AddComponent<UIHoverEventCaller>().SetActions(
                    (ped) => {if(newSelectable.interactable) BottomLog.DisplayMessage(hoverMessage);},
                    (ped) => {if(newSelectable.interactable) BottomLog.ClearDisplay();}
                );
            }
            // list entry
            headerSelectables.Add((newSelectable, newSelectableIcon));
            // spacing the header
            RebuildHeader();
            // output
            return newSelectable;
        }

        public Button AddHeaderButton (Sprite icon, System.Action onButtonClicked, string hoverMessage) {
            return (Button)(AddHeaderSelectable(icon, SetupButton, hoverMessage));

            Button SetupButton (GameObject btnGO, (Image bg, Image icon) imgs) {
                var newBtn = btnGO.AddComponent<Button>();
                newBtn.targetGraphic = imgs.bg;
                newBtn.onClick.AddListener(() => {onButtonClicked?.Invoke();});
                return newBtn;
            }
        }

        public Toggle AddHeaderToggle (Sprite icon, bool initialState, System.Action<bool> onStateChanged, string hoverMessage) {
            return (Toggle)(AddHeaderSelectable(icon, SetupToggle, hoverMessage));

            Selectable SetupToggle (GameObject toggleGO, (Image bg, Image icon) imgs) {
                var newToggle = toggleGO.AddComponent<Toggle>();
                newToggle.targetGraphic = imgs.bg;
                newToggle.isOn = initialState;
                newToggle.onValueChanged.AddListener((newVal) => {
                    UpdateToggleIconColor(newToggle, imgs.icon);
                    onStateChanged?.Invoke(newVal);
                });
                return newToggle;
            }

            void UpdateToggleIconColor (Toggle inputToggle, Image inputImage) {
                inputImage.color = inputToggle.isOn ? toggleOnColor : toggleOffColor;
            }
        }

        public UIPropertyField AddFloatProperty (ShaderProperty prop, System.Action<float> onValueChanged, System.Func<float, string> formatString, float scrollMultiplier = 1f) {
            if(NotYetInitAbort()){
                return null;
            }
            var newField = Instantiate(floatFieldPrefab);
            newField.rectTransform.SetParent(contentArea, false);
            newField.rectTransform.ResetLocalScale();
            newField.Initialize(prop, onValueChanged, formatString, scrollMultiplier);
            propFields.Add(newField);
            return newField;
        }

        public UIPropertyField AddColorProperty (ShaderProperty prop, System.Action<Color> onColorChanged) {
            if(NotYetInitAbort()){
                return null;
            }
            var newField = Instantiate(colorFieldPrefab);
            newField.rectTransform.SetParent(contentArea, false);
            newField.rectTransform.ResetLocalScale();
            newField.Initialize(prop, onColorChanged);
            propFields.Add(newField);
            return newField;
        }

        public UIPropertyField AddColorProperty (string propName, Color initColor, System.Action<Color> onColorChanged) {
            if(NotYetInitAbort()){
                return null;
            }
            var newField = Instantiate(colorFieldPrefab);
            newField.rectTransform.SetParent(contentArea, false);
            newField.rectTransform.ResetLocalScale();
            newField.Initialize(propName, initColor, onColorChanged);
            propFields.Add(newField);
            return newField;
        }

        void NotYetInitAbortConditionalUpdateThingy (System.Action ifNotAborted, bool rebuildContent) {
            if(NotYetInitAbort()){
                return;
            }
            ifNotAborted?.Invoke();
            ConditionalRebuildContent(rebuildContent);
        }

        public void SetBottomImageShown (bool value, bool rebuildContent = true) {
            NotYetInitAbortConditionalUpdateThingy(() => {m_bottomImageShouldBeShown = value; UpdateBottomImageVisibility();}, rebuildContent);
        }

        void UpdateBottomImageVisibility (bool rebuildContent = true) {
            bottomImage.SetGOActive(m_bottomImageShouldBeShown && !m_forceHideBottomImage);
        }

        public void SetBottomTextShown (bool value, bool rebuildContent = true) {
            NotYetInitAbortConditionalUpdateThingy(() => {m_bottomTextShouldBeShown = value; UpdateBottomTextVisibility();}, rebuildContent);
        }

        void UpdateBottomTextVisibility (bool rebuildContent = true) {
            bottomText.SetGOActive(m_bottomTextShouldBeShown || m_forceShowBottomText);
        }

        public void UpdateBottomImage (Sprite newImageSprite, bool rebuildContent = true) {
            NotYetInitAbortConditionalUpdateThingy(() => {bottomImage.sprite = newImageSprite;}, rebuildContent);
        }

        public void UpdateBottomText (string newText, bool rebuildContent = true) {
            NotYetInitAbortConditionalUpdateThingy(() => {bottomText.text = newText;}, rebuildContent);
        }

        public void DestroyPropFields (int startIndex = 0, bool rebuildContent = true) {
            if(startIndex < 0 || startIndex >= propFields.Count){
                Debug.LogError("Index out of bounds! Aborting...", this.gameObject);
                return;
            }
            for(int i=propFields.Count-1; i>=startIndex; i--){
                Destroy(propFields[i].gameObject);
                propFields.RemoveAt(i);
            }
            ConditionalRebuildContent(rebuildContent);
        }
    
    }

}