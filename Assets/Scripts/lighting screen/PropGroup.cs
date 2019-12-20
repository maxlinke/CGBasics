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
        [SerializeField] RectTransform bottomImageRT;

        [Header("Settings")]
        [SerializeField] float buttonSize;
        [SerializeField] float contentElementVerticalMargin;
        
        public RectTransform rectTransform => m_rectTransform;
        
        bool initialized = false;
        Button configButton;
        Image configButtonIcon;
        List<UIProp> props;

        public void LoadColors (ColorScheme cs) {

        }
        
        // public void Initialize (string initName, IEnumerable<System.Func<) { // func prop, gameobject?
        public void Initialize () {
            if(initialized){
                Debug.LogError("already initialized! aborting!", this.gameObject);
                return;
            }


            this.initialized = true;
        }

        public void SetName (string newName) {
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
            var activeCache = gameObject.activeSelf;
            gameObject.SetActive(true);
            if(!gameObject.activeInHierarchy){
                Debug.LogWarning($"{nameof(PropGroup)} \"{gameObject.name}\" is not active in hierarchy! Heights of TMPs might be off!", this.gameObject);
            }
            float y = 0;
            for(int i=0; i<contentArea.childCount; i++){
                var child = (RectTransform)(contentArea.GetChild(i));
                if(!child.gameObject.activeSelf || child == bottomText.rectTransform || child == bottomImageRT){
                    continue;
                }
                child.anchoredPosition = new Vector2(child.anchoredPosition.x, y);
                float deltaY = child.rect.height;
                y -= (deltaY + ((i+1 < contentArea.childCount) ? contentElementVerticalMargin : 0));
            }
            gameObject.SetActive(activeCache);
        }

        public Button AddConfigButton (Sprite icon, System.Action onButtonClicked, string hoverMessage) {
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
            header.rectTransform.SetToFillWithMargins(0f, headerArea.rect.height, 0f, 0f);
            // output
            return configButton;
        }

        public GameObject AddSliderProperty () {
            return null;
        }

        public GameObject AddColorProperty () {
            return null;
        }

        public void ShowImage (Sprite sprite) {

        }

        public void HideImage () {

        }

        public void ShowText (string textToShow) {

        }

        public void HideText () {

        }
    
    }

}