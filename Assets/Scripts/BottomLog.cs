using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BottomLog : MonoBehaviour {

    private static BottomLog instance;

    [SerializeField] Image bottomBackgroundImage; 
    [SerializeField] TextMeshProUGUI bottomTextField;

    private Log currentlyDisplayedMessage;
    private List<Log> logs;
    private Color messageColor;
    private Color warningColor;
    private Color errorColor;

    float hideTime;

    // TODO the expanded log thing, the counters for the actual LOGS, flashing...

    void Awake () {
        if(instance != null){
            Debug.LogError($"Singleton violation! Instance of {nameof(BottomLog)} was not null! Aborting...");
            return;
        }
        instance = this;
        logs = new List<Log>();
        hideTime = Mathf.Infinity;
        Clear();
    }

    void Update () {
        if(Time.time > hideTime){
            ClearDisplay();
            hideTime = Mathf.Infinity;
        }
    }

    void OnEnable () {
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColors;
    }

    void LoadColors (ColorScheme cs) {
        bottomBackgroundImage.color = cs.BottomLogBackground;
        // cs.BottomLogExpandedBackground
        messageColor = cs.BottomLogRegularText;
        warningColor = cs.BottomLogWarningText;
        errorColor = cs.BottomLogErrorText;
        if(currentlyDisplayedMessage != null){
            Display(currentlyDisplayedMessage, hideTime - Time.time);     // to redo the colors
        }
    }

    // TODO a timer for the messages

    ///<summary>Only displays a message, doesn't add it to the log</summary>
    public static void DisplayMessage (string message) {
        instance.Display(message, LogType.REGULAR, Mathf.Infinity);
    }

    ///<summary>Displays a message that automatically disappears after a given time</summary>
    public static void DisplayMessage (string message, float duration) {
        instance.Display(message, LogType.REGULAR, duration);
    }

    ///<summary>Displays and logs a message</summary>
    public static void LogMessage (string message) {
        instance.Display(message, LogType.REGULAR, Mathf.Infinity);
        instance.logs.Add(new Log(message, LogType.REGULAR));           // TODO the scrolling display (only update if visible...)
    }

    ///<summary>Displays and logs a warning</summary>
    public static void LogWarning (string message) {
        instance.Display(message, LogType.WARNING, Mathf.Infinity);
        instance.logs.Add(new Log(message, LogType.WARNING));
    }

    ///<summary>Displays and logs an error</summary>
    public static void LogError (string message) {
        instance.Display(message, LogType.ERROR, Mathf.Infinity);
        instance.logs.Add(new Log(message, LogType.ERROR));
    }

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
