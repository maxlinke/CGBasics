﻿using System.Collections;
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
    [SerializeField] bool to255;

    bool initialized = false;
    ColorPickerChannelSlider rSlider;
    ColorPickerChannelSlider gSlider;
    ColorPickerChannelSlider bSlider;
    ColorPickerChannelSlider aSlider;

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
        CreateSliders();
        backgroundRaycastCatcher.gameObject.AddComponent<UIBackgroundAbortRaycastCatcher>().onClick += HideAndReset;

        gameObject.SetActive(false);
        this.initialized = true;

        void CreateSliders () {

        }
    }

    void RebuildContent () {

    }
    
    public static void Open (Color initColor, bool includeAlpha, System.Action<Color> onClose, System.Action<Color> whileOpen) {
        instance.Unhide(initColor, includeAlpha, onClose, whileOpen);
    }

    void Unhide (Color initColor, bool includeAlpha, System.Action<Color> onClose, System.Action<Color> whileOpen) {
        if(gameObject.activeSelf){
            Debug.LogWarning("Duplicate colorpickers aren't supported! Aborting...");
            return;
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
