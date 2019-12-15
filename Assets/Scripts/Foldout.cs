using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Foldout : MonoBehaviour {

    private static Foldout instance;

    [Header("Components")]
    [SerializeField] Image backgroundRaycastCatcher;
    [SerializeField] RectTransform buttonParent;

    [Header("Settings")]
    [SerializeField] float foldoutWidth;
    [SerializeField] int maxLineCount;

    [Header("Regular Button Generation")]
    [SerializeField] TMP_FontAsset buttonFont;
    [SerializeField] float buttonFontSize;
    [SerializeField] float buttonTextVerticalMargin;
    [SerializeField] float buttonTextHorizontalMargin;

    [Header("Up/Down Button Generation")]
    [SerializeField] float upDownButtonHeight;
    [SerializeField] float upDownButtonSpriteSize;

    bool initialized = false;
    bool subscribedToInputSystem = false;
    List<FoldoutButton> buttons;
    List<List<FoldoutButton>> buttonGroups;
    List<FoldoutButton> displayedButtons;

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
        buttonParent.SetSizeDeltaX(foldoutWidth);
        buttons = new List<FoldoutButton>();
        buttonGroups = new List<List<FoldoutButton>>();
        backgroundRaycastCatcher.enabled = true;
        backgroundRaycastCatcher.raycastTarget = true;
        backgroundRaycastCatcher.color = Color.clear;
        backgroundRaycastCatcher.gameObject.AddComponent(typeof(BackgroundRaycastCatcher));
        backgroundRaycastCatcher.GetComponent<BackgroundRaycastCatcher>().Initialize(this);
        HideAndReset();
        initialized = true;
    }

    void LoadColors (ColorScheme cs) {
        buttonParent.gameObject.GetComponent<Image>().color = cs.FoldoutBackground;
        if(displayedButtons == null){
            return;
        }
        for(int i=0; i<displayedButtons.Count; i++){
            var btn = displayedButtons[i].button;
            displayedButtons[i].background.color = Color.white;
            displayedButtons[i].foreground.color = (btn.interactable ? cs.FoldoutButtonsText : cs.FoldoutButtonsTextDisabled);
            var mainCol = cs.FoldoutButtons[i % cs.FoldoutButtons.Length];
            btn.SetFadeTransition(0f, mainCol, cs.FoldoutButtonsHover, cs.FoldoutButtonsClick, mainCol);
        }
    }

    void HideAndReset () {
        if(initialized){
            BottomLog.ClearDisplay();
        }
        for(int i=buttonParent.childCount-1; i>=0; i--){
            var child = buttonParent.GetChild(i).gameObject;
            Destroy(child);
        }
        buttons.Clear();
        buttonGroups.Clear();
        displayedButtons = null;
        onNotSelectAnything = null;
        gameObject.SetActive(false);
        if(subscribedToInputSystem){
            InputSystem.UnSubscribe(this);
            subscribedToInputSystem = false;
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
        gameObject.SetActive(true);                             // needs to happen before i build the elements because text meshes ABSOLUTELY DO NOT UPDATE THEIR VALUES unless enabled (even if you explicitly tell them to)
        this.onNotSelectAnything = onNotSelectAnything;
        foreach(var setup in setups){
            buttons.Add(CreateButtonFromSetup(setup));              
        }
        if(buttons.Count < 1){
            Debug.LogWarning("No buttons created, aborting!", this.gameObject);
            HideAndReset();
            return;
        }
        GroupButtonsAndSetupRectTransforms();
        ShowGroup(0);
        LoadColors(ColorScheme.current);
        InputSystem.Subscribe(this, new InputSystem.KeyEvent(KeyCode.Escape, onKeyDown: AbortSelection));
        subscribedToInputSystem = true;

        void GroupButtonsAndSetupRectTransforms () {
            float width = 0f;
            float height = 0f;
            for(int i=0; i<buttons.Count; i++){
                height += buttons[i].rectTransform.rect.height;
                if(width == 0f){
                    width = buttons[i].rectTransform.rect.width;
                }
            }
            float scaledWidth = scale * width;
            float scaledHeight = scale * height;

            float leftSpace = Input.mousePosition.x;
            float bottomSpace = Input.mousePosition.y;
            float rightSpace = Screen.width - leftSpace;
            float topSpace = Screen.height - bottomSpace;

            bool toRight = (rightSpace >= scaledWidth || leftSpace < scaledWidth);
            bool toBottom = (bottomSpace >= scaledHeight ? true : ((topSpace >= scaledHeight) ? false : (bottomSpace >= topSpace)));
            float pivotX = toRight ? 0 : 1;
            float pivotY = toBottom ? 1 : 0;

            buttonParent.SetPivot(pivotX, pivotY);
            buttonParent.anchoredPosition = Input.mousePosition;
            buttonParent.localScale = Vector3.one * scale;

            float offsetMultiplier = toBottom ? -1 : 1;
            float y, screenY;
            int groupIndex = 0;
            List<FoldoutButton> currentGroup = null;
            NextGroup();
            for(int i=0; i<buttons.Count; i++){
                var b = buttons[i];
                PlaceButtonUpdateYAndInsertIntoGroup(b);
                if(NeedToCreateNewGroup()){
                    var currentGroupIndex = groupIndex;
                    var nextGroupIndex = groupIndex + 1;
                    var nextArrow = CreateUpDownButton(toBottom ? UISprites.UIDirDown : UISprites.UIDirUp, (() => {ShowGroup(nextGroupIndex);}));
                    PlaceButtonUpdateYAndInsertIntoGroup(nextArrow);
                    NextGroup();
                    var prevArrow = CreateUpDownButton(toBottom ? UISprites.UIDirUp : UISprites.UIDirDown, (() => {ShowGroup(currentGroupIndex);}));
                    PlaceButtonUpdateYAndInsertIntoGroup(prevArrow);
                }

                float SignedOffset (FoldoutButton inputButton) { return inputButton.rectTransform.rect.height * offsetMultiplier; }
                float SignedScreenOffset (FoldoutButton inputButton) { return inputButton.rectTransform.rect.height * scale * offsetMultiplier; }

                void PlaceButtonUpdateYAndInsertIntoGroup (FoldoutButton inputButton) {
                    inputButton.rectTransform.SetAnchor(buttonParent.pivot);
                    inputButton.rectTransform.pivot = buttonParent.pivot;
                    inputButton.rectTransform.anchoredPosition = new Vector2(0, y);
                    y += SignedOffset(inputButton);
                    screenY += SignedScreenOffset(inputButton);
                    currentGroup.Add(inputButton);
                }

                bool NeedToCreateNewGroup () {
                    if(i+1 >= buttons.Count){           // check if we're already done
                        return false;
                    }
                    float tempScreenY = screenY + SignedScreenOffset(buttons[i+1]);
                    if(tempScreenY >= 0 && tempScreenY <= Screen.height){   // check if next one fits
                        if(i+2 >= buttons.Count){                           // if it's the last one, we don't need a new group
                            return false;
                        }
                        // tempScreenY += SignedScreenOffset(buttons[i+2]);
                        tempScreenY += upDownButtonHeight * scale * offsetMultiplier;   // check if we can fit another page change button
                        return (tempScreenY < 0 || tempScreenY > Screen.height);        // it it fits, then we don't have to worry about it. the next call of this will insert said button if there is need for it
                    }
                    return true;
                }
            }
            buttonGroups.Add(currentGroup);
            buttonParent.anchoredPosition += new Vector2(toRight ? 1 : -1, toBottom ? -1 : 1);  // offsetting it by 1 pixel so the mouse isn't on the first element

            void NextGroup () {
                y = 0;
                screenY = Input.mousePosition.y;
                if(currentGroup != null){
                    buttonGroups.Add(currentGroup);
                    groupIndex++;
                }
                currentGroup = new List<FoldoutButton>();
            }
        }
    }

    FoldoutButton CreateButtonFromSetup (ButtonSetup setup) {
        CreateRawButton(out var btn, out var rt, out var bg);
        btn.gameObject.name = $"Button ({setup.buttonName})";
        btn.interactable = setup.buttonInteractable;
        btn.onClick.AddListener(() => {
            HideAndReset();
            setup.buttonClickAction?.Invoke();
        });
        btn.gameObject.AddComponent<UIHoverEventCaller>().SetActions((ped) => {BottomLog.DisplayMessage(setup.buttonHoverMessage);}, (ped) => {BottomLog.ClearDisplay();});
        var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        label.rectTransform.SetParent(rt, false);
        label.rectTransform.ResetLocalScale();
        label.rectTransform.SetToFillWithMargins(0, buttonTextHorizontalMargin, 0, buttonTextHorizontalMargin);
        label.alignment = TextAlignmentOptions.Left;
        label.font = buttonFont;
        label.fontSize = buttonFontSize;
        label.enableWordWrapping = true;
        label.overflowMode = TextOverflowModes.Overflow;
        label.raycastTarget = false;
        label.text = setup.buttonName;
        label.ForceMeshUpdate();
        int lineCount = label.textInfo.lineCount;
        if(lineCount > maxLineCount){
            rt.SetSizeDeltaY((label.preferredHeight / lineCount) * maxLineCount + 2 * buttonTextVerticalMargin);
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.ForceMeshUpdate(); 
        }else{
            rt.SetSizeDeltaY(label.preferredHeight + 2 * buttonTextVerticalMargin);
        }
        return new FoldoutButton(
            gameObject: btn.gameObject,
            button: btn,
            rectTransform: rt,
            background: bg,
            foreground: label
        );
    }

    FoldoutButton CreateUpDownButton (Sprite sprite, System.Action onClick) {
        CreateRawButton(out var btn, out var rt, out var bg);
        btn.interactable = true;
        btn.gameObject.name = $"Button ({sprite.name})";
        btn.onClick.AddListener(() => {
            onClick?.Invoke();
        });
        rt.SetSizeDeltaY(upDownButtonHeight);
        var iconRT = new GameObject("Icon", typeof(RectTransform)).GetComponent<RectTransform>();
        iconRT.SetParent(rt, false);
        iconRT.ResetLocalScale();
        iconRT.SetPivot(0.5f, 0.5f);
        iconRT.SetAnchor(0.5f, 0.5f);
        iconRT.sizeDelta = upDownButtonSpriteSize * Vector2.one;
        iconRT.anchoredPosition = Vector2.zero;
        var icon = iconRT.gameObject.AddComponent<Image>();
        icon.sprite = sprite;
        icon.raycastTarget = false;
        return new FoldoutButton(
            gameObject: btn.gameObject,
            button: btn,
            rectTransform: rt,
            background: bg,
            foreground: icon
        );
    }

    void CreateRawButton (out Button outputButton, out RectTransform outputRectTransform, out Image outputBackground) {
        outputRectTransform = new GameObject("New Button", typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<RectTransform>();
        outputRectTransform.SetParent(buttonParent, false);
        outputRectTransform.ResetLocalScale();
        outputRectTransform.SetAnchor(0, 1);
        outputRectTransform.SetPivot(0, 1);
        outputRectTransform.SetSizeDelta(foldoutWidth, 100f);
        outputButton = outputRectTransform.GetComponent<Button>();
        outputBackground = outputRectTransform.GetComponent<Image>();
        outputBackground.raycastTarget = true;
    }

    void ShowGroup (int groupIndex) {
        float height = 0f;
        for(int i=0; i<buttonGroups.Count; i++){
            if(i == groupIndex){
                foreach(var b in buttonGroups[i]){
                    b.gameObject.SetActive(true);
                    height += b.rectTransform.rect.height;
                }
            }else{
                foreach(var b in buttonGroups[i]){
                    b.gameObject.SetActive(false);
                }
            }
        }
        buttonParent.SetSizeDeltaY(height);
        displayedButtons = buttonGroups[groupIndex];
        LoadColors(ColorScheme.current);
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

    private class FoldoutButton {
        public readonly GameObject gameObject;
        public readonly Button button;
        public readonly RectTransform rectTransform;
        public readonly Image background;
        public readonly Graphic foreground;
        public FoldoutButton (GameObject gameObject, Button button, RectTransform rectTransform, Image background, Graphic foreground) {
            this.gameObject = gameObject;
            this.button = button;
            this.rectTransform = rectTransform;
            this.background = background;
            this.foreground = foreground;
        }
    }
	
}
