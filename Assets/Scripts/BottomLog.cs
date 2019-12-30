using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BottomLog : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private static BottomLog instance;

    [Header("Components")]
    [SerializeField] GameObject bottomBarContentParent;
    [SerializeField] Image bottomBackgroundImage;
    [SerializeField] ImageFlasher messageFlasher;
    [SerializeField] TextMeshProUGUI bottomTextField;

    [Header("Settings")]
    [SerializeField] bool flashOnMessage;
    [SerializeField] bool hideWhenNotNeeded;
    [SerializeField] bool lowerOpacityWhenNotNeeded;
    [SerializeField] bool showOnHover;
    [SerializeField] float opacityWhenNotNeeded;
    [SerializeField] float opacityFadeOutTime;

    private Log currentlyDisplayedMessage;
    private List<Log> logs;
    private Color messageColor;
    private Color warningColor;
    private Color errorColor;

    private float backgroundLerp;
    private Color backgroundColor;
    private Color backgroundColorNotNeeded;

    float hideTime;
    bool pointerHover = false;

    public void OnPointerEnter (PointerEventData eventData) {
        pointerHover = true;
    }

    public void OnPointerExit (PointerEventData eventData) {
        pointerHover = false;
    }

    void Awake () {
        if(instance != null){
            Debug.LogError($"Singleton violation! Instance of {nameof(BottomLog)} was not null! Aborting...");
            return;
        }
        instance = this;
        logs = new List<Log>();
        hideTime = Mathf.Infinity;
        messageFlasher.Initialize(messageFlasher.gameObject.GetComponent<Image>());
        Clear();
    }

    void Update () {
        if(Time.time > hideTime){
            ClearDisplay();
            hideTime = Mathf.Infinity;
        }
        bool noMessage = currentlyDisplayedMessage == null || currentlyDisplayedMessage.message == null || currentlyDisplayedMessage.message.Length < 1;
        bool notNeeded = !(pointerHover && showOnHover) && noMessage;
        if(notNeeded){
            if(hideWhenNotNeeded){
                bottomBarContentParent.SetActive(false);
            }
            backgroundLerp = Mathf.Clamp01(backgroundLerp - (Time.deltaTime / opacityFadeOutTime));
            if(lowerOpacityWhenNotNeeded){
                bottomBackgroundImage.color = Color.Lerp(backgroundColorNotNeeded, backgroundColor, backgroundLerp);
            }
        }else{
            bottomBarContentParent.SetActive(true);
            bottomBackgroundImage.color = backgroundColor;
            backgroundLerp = 1f;
        }
        bottomBackgroundImage.raycastTarget = showOnHover || bottomBackgroundImage.color.a > 0;
    }

    void OnEnable () {
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColors;
    }

    void LoadColors (ColorScheme cs) {
        backgroundColor = cs.BottomLogBackground;
        backgroundColorNotNeeded = backgroundColor * new Color(1, 1, 1, opacityWhenNotNeeded);
        // cs.BottomLogExpandedBackground
        messageColor = cs.BottomLogRegularText;
        warningColor = cs.BottomLogWarningText;
        errorColor = cs.BottomLogErrorText;
        if(currentlyDisplayedMessage != null){
            Display(currentlyDisplayedMessage, hideTime - Time.time);     // to redo the colors
        }
        messageFlasher.UpdateFlashColor(cs.BottomLogMessageFlash);
    }

    ///<summary>Only displays a message, doesn't add it to the log</summary>
    public static void DisplayMessage (string message) {
        instance.Display(message, LogType.REGULAR, Mathf.Infinity);
    }

    ///<summary>Displays a message that automatically disappears after a given time</summary>
    public static void DisplayMessage (string message, float duration) {
        instance.Display(message, LogType.REGULAR, duration);
    }

    // ///<summary>Displays and logs a message</summary>
    // public static void LogMessage (string message) {
    //     instance.Display(message, LogType.REGULAR, Mathf.Infinity);
    //     instance.logs.Add(new Log(message, LogType.REGULAR));
    // }

    // ///<summary>Displays and logs a warning</summary>
    // public static void LogWarning (string message) {
    //     instance.Display(message, LogType.WARNING, Mathf.Infinity);
    //     instance.logs.Add(new Log(message, LogType.WARNING));
    // }

    // ///<summary>Displays and logs an error</summary>
    // public static void LogError (string message) {
    //     instance.Display(message, LogType.ERROR, Mathf.Infinity);
    //     instance.logs.Add(new Log(message, LogType.ERROR));
    // }

    ///<summary>Clears the display and the log</summary>
    public static void Clear () {
        ClearDisplay();
        instance.logs.Clear();
    }

    ///<summary>Clears the display</summary>
    public static void ClearDisplay () {
        instance.Display(string.Empty, LogType.REGULAR, Mathf.Infinity);
    }

    void Display (Log log, float duration) {
        Display(log.message, log.type, duration);
    }

    void Display (string message, LogType logType, float duration) {
        bottomTextField.text = GetColoredString(message, logType);
        currentlyDisplayedMessage = new Log(message, logType);
        hideTime = Time.time + duration;
        if(flashOnMessage && message.Trim().Length > 0){
            messageFlasher.Flash(0.5f);
        }
    }

    Color GetColorForLogType (LogType logType) {
        switch(logType){
            case LogType.REGULAR:
                return messageColor;
            case LogType.WARNING:
                return warningColor;
            case LogType.ERROR:
                return errorColor;
            default:
                throw new System.ArgumentException($"Unknown {nameof(LogType)} \"{logType}\"!");
        }
    }

    string GetColoredString (string message, LogType logType) {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(GetColorForLogType(logType))}>{message}</color>";        
    }

    void OnDestroy () {
        if(instance == this){
            instance = null;
        }
    }

    private enum LogType {
        REGULAR,
        WARNING,
        ERROR
    }

    private class Log {
        
        public readonly string message;
        public readonly LogType type;

        public Log (string message, LogType type) {
            this.message = message;
            this.type = type;
        }

    }
	
}
