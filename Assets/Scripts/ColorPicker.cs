using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] Image colorDisplayOutline;

    [Header("Settings")]
    [SerializeField] float extraBottomYOffset;
    [SerializeField] bool enableComparison;

    bool initialized = false;
    ColorPickerChannelSlider rSlider;
    ColorPickerChannelSlider gSlider;
    ColorPickerChannelSlider bSlider;
    ColorPickerChannelSlider aSlider;
    Image compareImage;

    System.Action<Color> onClose;
    System.Action<Color> whileOpen;

    Color currentColor {
        get {
            return new Color(
                r: GetSliderValue(rSlider),
                g: GetSliderValue(gSlider),
                b: GetSliderValue(bSlider),
                a: GetSliderValue(aSlider)
            );

            float GetSliderValue (ColorPickerChannelSlider slider) {
                return (slider.gameObject.activeSelf ? slider.normalizedValue : 1f);
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
        colorDisplayOutline.color = cs.ColorPickerColorOutline;
        
        rSlider.LoadColors(cs);
        gSlider.LoadColors(cs);
        bSlider.LoadColors(cs);
        aSlider.LoadColors(cs);
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
        rSlider = CreateSlider("R");
        gSlider = CreateSlider("G");
        bSlider = CreateSlider("B");
        aSlider = CreateSlider("A");
        backgroundRaycastCatcher.gameObject.AddComponent<UIBackgroundAbortRaycastCatcher>().onClick += HideAndReset;
        if(enableComparison){
            var oldRT = colorDisplay.GetComponent<RectTransform>();
            oldRT.anchorMin = new Vector2(0.5f, 0f);
            oldRT.anchorMax = new Vector2(1f, 1f);
            compareImage = Instantiate(colorDisplay);
            var newRT = compareImage.GetComponent<RectTransform>();
            newRT.SetParent(oldRT.parent, false);
            newRT.ResetLocalScale();
            newRT.anchorMin = new Vector2(0f, 0f);
            newRT.anchorMax = new Vector2(0.5f, 1f);
        }
        gameObject.SetActive(false);
        this.initialized = true;

        ColorPickerChannelSlider CreateSlider (string label) {
            var newSlider = Instantiate(sliderTemplate);
            newSlider.rectTransform.SetParent(containerRT, false);
            newSlider.rectTransform.ResetLocalScale();
            newSlider.Initialize(
                labelText: label, 
                maxValue: 1f, 
                initValue: 0f
            );
            newSlider.SetGOActive(true);
            return newSlider;
        }
    }

    void RebuildContent () {
        float y = 0;
        for(int i=0; i<containerRT.childCount; i++){
            var child = (RectTransform)(containerRT.GetChild(i));
            if(!child.gameObject.activeSelf){
                continue;
            }
            child.anchoredPosition = new Vector2(child.anchoredPosition.x, y);
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
        rSlider.normalizedValue = initColor.r;
        gSlider.normalizedValue = initColor.g;
        bSlider.normalizedValue = initColor.b;
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
        gameObject.SetActive(true);
    }

    void HideAndReset () {
        var cachedOnClose = onClose;
        gameObject.SetActive(false);
        onClose = null;
        whileOpen = null;
        cachedOnClose?.Invoke(currentColor);
    }
	
}
