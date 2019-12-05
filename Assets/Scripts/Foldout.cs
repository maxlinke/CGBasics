using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Foldout : MonoBehaviour {

    private static Foldout instance;

    [SerializeField] Image backgroundRaycastCatcher;
    [SerializeField] RectTransform buttonParent;
    [SerializeField] Button buttonTemplate;

    bool initialized;
    bool subscribedToInputSystem;
    List<Button> buttons;
    System.Action onNotSelectAnything;

    void OnEnable () {
        if(!initialized){
            Initialize();
        }
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColors;
    }

    void Initialize () {
        if(initialized){
            Debug.LogWarning("Duplicate init call! Aborting!", this.gameObject);
            return;
        }
        if(instance != null){
            Debug.LogError($"Singleton violation! Instance of {nameof(Foldout)} is not null! Aborting!", this.gameObject);
            return;
        }
        instance = this;
        buttons = new List<Button>();
        backgroundRaycastCatcher.enabled = true;
        backgroundRaycastCatcher.raycastTarget = true;
        backgroundRaycastCatcher.color = Color.clear;
        backgroundRaycastCatcher.gameObject.AddComponent(typeof(BackgroundRaycastCatcher));
        backgroundRaycastCatcher.GetComponent<BackgroundRaycastCatcher>().Initialize(this);
        buttonTemplate.SetGOActive(false);
        buttonTemplate.onClick.RemoveAllListeners();
        initialized = true;
    }

    void LoadColors (ColorScheme cs) {
        buttonParent.gameObject.GetComponent<Image>().color = cs.FoldoutBackground;
        for(int i=0; i<buttons.Count; i++){
            var btn = buttons[i];
            btn.GetComponent<Image>().color = Color.white;
            btn.GetComponentInChildren<TextMeshProUGUI>().color = (btn.interactable ? cs.FoldoutButtonsText : cs.FoldoutButtonsTextDisabled);
            var mainCol = cs.FoldoutButtons[i % cs.FoldoutButtons.Length];
            btn.SetFadeTransition(0f, mainCol, cs.FoldoutButtonsHover, cs.FoldoutButtonsClick, mainCol);
        }
    }

    void HideAndReset () {
        BottomLog.ClearDisplay();
        for(int i=buttons.Count-1; i>=0; i--){
            Destroy(buttons[i].gameObject);
        }
        buttons.Clear();
        onNotSelectAnything = null;
        gameObject.SetActive(false);
        if(subscribedToInputSystem){
            InputSystem.UnSubscribe(this);
        }
    }

    void AbortSelection () {
        var actionCache = onNotSelectAnything;
        HideAndReset();
        actionCache?.Invoke();      // because this could for example put something in the bottomlog which is cleared in HideAndReset()
    }

    public static void Create (IEnumerable<ButtonSetup> setups, System.Action onNotSelectAnything, float scale = 1f) {
        instance.Open(setups, onNotSelectAnything, scale);
    }

    void Open (IEnumerable<ButtonSetup> setups, System.Action onNotSelectAnything, float scale) {
        if(buttons.Count > 0){
            Debug.LogWarning("Duplicate foldouts! This is not supported!", this.gameObject);
            return;
        }
        EventSystem.current.SetSelectedGameObject(null);
        SetupContainer();
        LoadColors(ColorScheme.current);
        InputSystem.Subscribe(this, new InputSystem.KeyEvent(KeyCode.Escape, onKeyDown: AbortSelection));
        subscribedToInputSystem = true;
        this.onNotSelectAnything = onNotSelectAnything;

        void SetupContainer () {
            int i = 0;
            float y = 0f;
            var btnRect = buttonTemplate.gameObject.GetComponent<RectTransform>().rect;
            foreach(var setup in setups){
                var newBtn = Instantiate(buttonTemplate);
                newBtn.SetGOActive(true);
                var newBtnRT = newBtn.GetComponent<RectTransform>();
                newBtnRT.SetParent(buttonParent, false);
                newBtnRT.anchoredPosition = new Vector2(0, y);
                var newBtnLabel = newBtn.GetComponentInChildren<TextMeshProUGUI>();
                newBtnLabel.text = setup.buttonName;
                newBtn.interactable = setup.buttonInteractable;
                var onClick = setup.buttonClickAction;
                newBtn.onClick.AddListener(() => {onClick?.Invoke();});
                var message = setup.buttonHoverMessage;
                newBtn.gameObject.AddComponent(typeof(UIHoverEventCaller));
                newBtn.GetComponent<UIHoverEventCaller>().SetActions((ped) => {BottomLog.DisplayMessage(message);}, (ped) => {BottomLog.ClearDisplay();});

                buttons.Add(newBtn);
                y -= newBtnRT.rect.height;
                i++;
            }
            buttonParent.sizeDelta = new Vector2(btnRect.width, i * btnRect.height);
            var scaledDimensions = scale * new Vector2(buttonParent.rect.width, buttonParent.rect.height);
            float leftSpace = Input.mousePosition.x;
            float bottomSpace = Input.mousePosition.y;
            float rightSpace = Screen.width - leftSpace;
            float topSpace = Screen.height - bottomSpace;
            float pivotX, pivotY;
            if(rightSpace < scaledDimensions.x && leftSpace >= scaledDimensions.x){
                pivotX = 1;
            }else{
                pivotX = 0;
            }
            if(bottomSpace < scaledDimensions.y && topSpace >= scaledDimensions.y){
                pivotY = 0;
            }else{
                pivotY = 1;
            }
            buttonParent.pivot = new Vector2(pivotX, pivotY);
            buttonParent.anchoredPosition = Input.mousePosition;
            buttonParent.localScale = Vector3.one * scale;
        }
    }


    public class ButtonSetup {
        public readonly string buttonName;
        public readonly string buttonHoverMessage;
        public readonly System.Action buttonClickAction;
        public readonly bool buttonInteractable;
        public ButtonSetup (string buttonName, string buttonHoverMessage, System.Action buttonClickAction, bool buttonInteractable) {
            this.buttonName = buttonName;
            this.buttonHoverMessage = buttonHoverMessage;
            this.buttonClickAction = buttonClickAction;
            this.buttonInteractable = buttonInteractable;
        }
    }

    private class BackgroundRaycastCatcher : MonoBehaviour, IPointerClickHandler {
        private Foldout parent;
        public void Initialize (Foldout parent) {
            this.parent = parent;
        }
        public void OnPointerClick (PointerEventData eventData) {
            parent.AbortSelection();
        }
    }
	
}
