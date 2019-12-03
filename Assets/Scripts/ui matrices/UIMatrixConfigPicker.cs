using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIMatrixConfigPicker : MonoBehaviour {

    private static UIMatrixConfigPicker instance;

    [SerializeField] Image backgroundRaycastCatcher;
    [SerializeField] RectTransform configButtonParent;
    [SerializeField] Image configButtonParentBackground;
    [SerializeField] Button configButtonTemplate;

    Button[] buttons;
    TextMeshProUGUI[] buttonLabels;
    bool initialized;
    System.Action<UIMatrixConfig> currentOnPickAction;

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

    void LoadColors (ColorScheme cs) {
        configButtonParentBackground.color = cs.UiMatrixConfigPickerBackground;
        for(int i=0; i<buttonLabels.Length; i++){
            var defaultColor = cs.UiMatrixConfigPickerButtons[i % cs.UiMatrixConfigPickerButtons.Length];
            buttons[i].SetFadeTransition(0f, defaultColor, cs.UiMatrixConfigPickerButtonsHover, cs.UiMatrixConfigPickerButtonsClick, Color.magenta);
            buttonLabels[i].color = cs.UiMatrixConfigPickerText;
        }
    }

    void Initialize () {
        if(initialized){
            Debug.LogWarning("Duplicate initialization call! Aborting...", this.gameObject);
            return;
        }
        if(instance != null){
            Debug.LogError($"Singleton violation! Instance of {nameof(UIMatrixConfigPicker)} wasn't null! Aborting...");
            return;
        }
        instance = this;
        backgroundRaycastCatcher.SetGOActive(true);
        backgroundRaycastCatcher.raycastTarget = true;
        backgroundRaycastCatcher.color = Color.clear;
        backgroundRaycastCatcher.gameObject.AddComponent(typeof(BackgroundRaycastCatcher));
        backgroundRaycastCatcher.gameObject.GetComponent<BackgroundRaycastCatcher>().Initialize(this);
        configButtonTemplate.SetGOActive(false);
        var btnRT = configButtonTemplate.GetComponent<RectTransform>();
        var configTypes = System.Enum.GetValues(typeof(UIMatrixConfig.Type));
        configButtonParent.sizeDelta = new Vector2(btnRT.rect.width, configTypes.Length * btnRT.rect.height);
        configButtonParent.SetGOActive(true);
        buttons = new Button[configTypes.Length];
        buttonLabels = new TextMeshProUGUI[configTypes.Length];
        float btnHeight = btnRT.rect.height;
        float y = 0;
        int i = 0;
        foreach(var configType in configTypes){
            var config = UIMatrixConfig.GetForType((UIMatrixConfig.Type)configType);
            var newBtn = Instantiate(configButtonTemplate).GetComponent<Button>();
            newBtn.SetGOActive(true);
            newBtn.onClick.AddListener(() => {
                ButtonClicked(config);
            });
            newBtn.gameObject.AddComponent<UIHoverEventCaller>();
            var hoverCaller = newBtn.GetComponent<UIHoverEventCaller>();
            hoverCaller.SetActions((ped) => {BottomLog.DisplayMessage(config.description);}, (ped) => {BottomLog.ClearDisplay();});
            var newBtnRT = newBtn.GetComponent<RectTransform>();
            newBtnRT.SetParent(configButtonParent, false);
            newBtnRT.anchoredPosition = new Vector2(0, y);
            var newBtnText = newBtn.GetComponentInChildren<TextMeshProUGUI>();
            newBtnText.text = config.name;
            var newBtnBG = newBtn.GetComponent<Image>();
            newBtnBG.color = Color.white;

            buttons[i] = newBtn;
            buttonLabels[i] = newBtnText;
            y -= btnHeight;
            i++;
        }
        HideAndReset();
        initialized = true;
    }

    void OnDestroy () {
        if(instance == this){
            instance = null;
        }
    }

    public static void Open (System.Action<UIMatrixConfig> onConfigPicked, float scale) {
        instance.Unhide(onConfigPicked, scale);
    }

    void Unhide (System.Action<UIMatrixConfig> onConfigPicked, float scale) {
        EventSystem.current.SetSelectedGameObject(null);
        gameObject.SetActive(true);
        var scaledDimensions = scale * new Vector2(configButtonParent.rect.width, configButtonParent.rect.height);
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
        configButtonParent.pivot = new Vector2(pivotX, pivotY);
        configButtonParent.anchoredPosition = Input.mousePosition;
        configButtonParent.localScale = Vector3.one * scale;
        currentOnPickAction = onConfigPicked;
    }

    void HideAndReset () {
        currentOnPickAction = null;
        gameObject.SetActive(false);
    }

    void ButtonClicked (UIMatrixConfig config) {
        BottomLog.ClearDisplay();
        currentOnPickAction.Invoke(config);
        HideAndReset();
    }

    void BackgroundClicked () {
        BottomLog.ClearDisplay();
        currentOnPickAction.Invoke(null);
        HideAndReset();
    }

    private class BackgroundRaycastCatcher : MonoBehaviour, IPointerClickHandler {
        private UIMatrixConfigPicker parent;
        public void Initialize (UIMatrixConfigPicker parent) {
            this.parent = parent;
        }
        public void OnPointerClick (PointerEventData eventData) {
            parent.BackgroundClicked();
        }
    }

}
