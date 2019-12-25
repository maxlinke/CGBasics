using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPicker : MonoBehaviour {

    private static ColorPicker instance;

    [Header("Slider Template")]
    [SerializeField] ColorPickerChannelSlider sliderTemplate;

    [Header("Components")]
    [SerializeField] Image backgroundRaycastCatcher;
    [SerializeField] RectTransform containerRT;
    [SerializeField] Image containerBG;
    [SerializeField] Image colorAlphaGrid;
    [SerializeField] Image colorDisplay;
    [SerializeField] Image colorOutlineInside;
    [SerializeField] Image colorOutlineOutside;
    [SerializeField] Toggle rgbToggle;
    [SerializeField] TextMeshProUGUI rgbToggleLabel;
    [SerializeField] TextMeshProUGUI rgbToggleLabelDropShadow;

    [Header("Settings")]
    [SerializeField] float extraBottomYOffset;
    [SerializeField] bool enableComparison;
    [SerializeField] bool rgbIsDefault;

    bool initialized = false;
    bool subscribedToInputSystem = false;
    ColorPickerChannelSlider rhSlider;
    ColorPickerChannelSlider gsSlider;
    ColorPickerChannelSlider bvSlider;
    ColorPickerChannelSlider aSlider;
    Image compareImage;

    System.Action<Color> onClose;
    System.Action<Color> whileOpen;

    Color currentColor {
        get {
            if(rgbToggle.isOn){
                return new Color(
                    r: GetSliderValue(rhSlider),
                    g: GetSliderValue(gsSlider),
                    b: GetSliderValue(bvSlider),
                    a: GetSliderValue(aSlider)
                );
            }else{
                var rgb = Color.HSVToRGB(
                    H: GetSliderValue(rhSlider),
                    S: GetSliderValue(gsSlider),
                    V: GetSliderValue(bvSlider)
                );
                return new Color(rgb.r, rgb.g, rgb.b, GetSliderValue(aSlider));
            }

            float GetSliderValue (ColorPickerChannelSlider slider) {
                return (slider.gameObject.activeSelf ? slider.normalizedValue : (slider == aSlider ? 1f : 0f));
            }
        }
    }

    void OnDestroy () {
        if(instance == this){
            instance = null;
        }
    }

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
        containerBG.color = cs.ColorPickerBackground;
        colorAlphaGrid.color = cs.ColorPickerAlphaGridTint;
        colorOutlineInside.color = cs.ColorPickerColorOutlineInside;
        colorOutlineOutside.color = cs.ColorPickerColorOutlineOutside;
        rhSlider.LoadColors(cs);
        gsSlider.LoadColors(cs);
        bvSlider.LoadColors(cs);
        aSlider.LoadColors(cs);
        rgbToggle.targetGraphic.color = Color.white;
        rgbToggle.graphic.color = cs.ColorPickerSliderLabel;
        rgbToggle.SetFadeTransition(0f, cs.ColorPickerInputFieldBackground, cs.ColorPickerInputFieldBackgroundHover, cs.ColorPickerInputFieldBackgroundClick, Color.magenta);
        rgbToggleLabel.color = cs.ColorPickerSliderLabel;
        rgbToggleLabelDropShadow.color = cs.ColorPickerDropShadows;
    }

    void Update () {
        var cachedColor = currentColor;
        colorDisplay.color = cachedColor;
        whileOpen?.Invoke(cachedColor);
    }

    void Initialize () {
        if(instance != null){
            Debug.LogError($"Singleton violation! Instance of {nameof(ColorPicker)} is not null!", this.gameObject);
            return;
        }
        instance = this;
        sliderTemplate.SetGOActive(false);
        rhSlider = CreateSlider();
        gsSlider = CreateSlider();
        bvSlider = CreateSlider();
        aSlider = CreateSlider();
        backgroundRaycastCatcher.gameObject.AddComponent<UIBackgroundAbortRaycastCatcher>().onClick += HideAndReset;
        if(enableComparison){
            var oldRT = colorDisplay.GetComponent<RectTransform>();
            oldRT.anchorMin = new Vector2(0.5f, 0f);
            oldRT.anchorMax = new Vector2(1f, 1f);
            compareImage = Instantiate(colorDisplay);
            var newRT = compareImage.GetComponent<RectTransform>();
            newRT.SetParent(oldRT.parent, false);
            newRT.SetSiblingIndex(oldRT.GetSiblingIndex());
            newRT.ResetLocalScale();
            newRT.anchorMin = new Vector2(0f, 0f);
            newRT.anchorMax = new Vector2(0.5f, 1f);
        }
        rgbToggle.isOn = rgbIsDefault;
        rgbToggle.onValueChanged.AddListener(RGBToggleToggled);
        RGBToggleToggled(rgbToggle.isOn);
        gameObject.SetActive(false);
        this.initialized = true;

        ColorPickerChannelSlider CreateSlider () {
            var newSlider = Instantiate(sliderTemplate);
            newSlider.rectTransform.SetParent(containerRT, false);
            newSlider.rectTransform.ResetLocalScale();
            newSlider.Initialize(
                maxValue: 1f, 
                initValue: 0f
            );
            newSlider.SetGOActive(true);
            return newSlider;
        }
    }

    void RGBToggleToggled (bool newVal) {
        rhSlider.SetLabel(rgbToggle.isOn ? "R" : "H");
        gsSlider.SetLabel(rgbToggle.isOn ? "G" : "S");
        bvSlider.SetLabel(rgbToggle.isOn ? "B" : "V");
        var currentColor = colorDisplay.color;
        if(rgbToggle.isOn){
            rhSlider.normalizedValue = currentColor.r;
            gsSlider.normalizedValue = currentColor.g;
            bvSlider.normalizedValue = currentColor.b;
        }else{
            Color.RGBToHSV(currentColor, out var h, out var s, out var v);
            rhSlider.normalizedValue = h;
            gsSlider.normalizedValue = s;
            bvSlider.normalizedValue = v;
        }
        aSlider.SetLabel("A");
        aSlider.normalizedValue = currentColor.a;
    }

    void RebuildContent () {
        float y = 0;
        for(int i=0; i<containerRT.childCount; i++){
            var child = (RectTransform)(containerRT.GetChild(i));
            if(!child.gameObject.activeSelf){
                continue;
            }
            child.SetAnchoredPositionY(y);
            y -= child.rect.height;
        }
        containerRT.SetSizeDeltaY(Mathf.Abs(y) + extraBottomYOffset);
    }
    
    public static void Open (Color initColor, bool includeAlpha, System.Action<Color> onClose, System.Action<Color> whileOpen) {
        instance.Unhide(initColor, includeAlpha, onClose, whileOpen);
    }

    void Unhide (Color initColor, bool includeAlpha, System.Action<Color> onClose, System.Action<Color> whileOpen) {
        if(gameObject.activeSelf){
            Debug.LogWarning("Duplicate colorpickers aren't supported! Aborting...");
            return;
        }
        colorDisplay.color = initColor;
        RGBToggleToggled(rgbToggle.isOn);       // sets the sliders correctly
        aSlider.normalizedValue = initColor.a;
        aSlider.SetGOActive(includeAlpha);
        RebuildContent();
        var newPivot = UIUtils.GetFullscreenCursorBoxPivot(containerRT.sizeDelta);
        containerRT.pivot = newPivot;
        containerRT.anchoredPosition = Input.mousePosition;
        containerRT.anchoredPosition -= new Vector2(Mathf.Sign(newPivot.x - 0.5f), Mathf.Sign(newPivot.y - 0.5f));
        if(enableComparison){
            compareImage.color = initColor;
        }
        this.onClose = onClose;
        this.whileOpen = whileOpen;
        InputSystem.Subscribe(this, new InputSystem.KeyEvent(KeyCode.Escape, HideAndReset));
        subscribedToInputSystem = true;
        gameObject.SetActive(true);
    }

    void HideAndReset () {
        if(subscribedToInputSystem){
            InputSystem.UnSubscribe(this);
        }
        var cachedOnClose = onClose;
        gameObject.SetActive(false);
        onClose = null;
        whileOpen = null;
        cachedOnClose?.Invoke(currentColor);
    }
	
}
