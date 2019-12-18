using System.Collections;
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
        [SerializeField] TextMeshProUGUI m_header;

        [Header("Settings")]
        [SerializeField] float buttonSize;
        [SerializeField] float contentElementVerticalMargin;
        
        public RectTransform rectTransform => m_rectTransform;
        
        bool initialized = false;
        Button configButton;
        Image configButtonIcon;

        // TODO list of "properties" (all have recttransforms)

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
            m_header.text = newName;
        }

        public void RebuildContent () {
            float y = 0;
            for(int i=0; i<contentArea.childCount; i++){
                var child = (RectTransform)(contentArea.GetChild(i));
                if(!child.gameObject.activeSelf){
                    continue;
                }
                child.anchoredPosition = new Vector2(child.anchoredPosition.x, y);
                float deltaY = child.rect.height;
                var childTMP = child.GetComponent<TextMeshProUGUI>();
                if(childTMP != null){
                    if(!childTMP.gameObject.activeSelf){
                        Debug.LogWarning("Gameobject of TMP is not active, preferredheight will not be accurate!", childTMP.gameObject);
                    }else{
                        deltaY = childTMP.preferredHeight;
                    }
                }
                y -= (deltaY + ((i+1 < contentArea.childCount) ? contentElementVerticalMargin : 0));
            }
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
            // output
            return configButton;
        }

        public GameObject AddSliderProperty (bool rebuildContent = true) {
            return null;
        }

        public GameObject AddColorProperty (bool rebuildContent = true) {
            return null;
        }

    // TODO how do i handle all this...
    
        // public GameObject AddImage (
    
    }

}