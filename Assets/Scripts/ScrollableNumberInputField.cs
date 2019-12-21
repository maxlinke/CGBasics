using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ScrollableNumberInputField : MonoBehaviour, IScrollHandler {
    
    bool initialized;
    InputField field;
    TMP_InputField tmpField;

    public static string hintText => $"Scrollable Input Field. Hold \"{InputSystem.sensitivityIncreaseKey}\" for a faster increase and \"{InputSystem.sensitivityReduceKey}\" for finer control";

    [System.NonSerialized] public float scrollMultiplier = 1f;

    public void Initialize (InputField field) {
        if(initialized){
            Debug.LogError("Duplicate init, aborting!", this.gameObject);
            return;
        }
        this.field = field;
        this.initialized = true;
    }

    public void Initialize (TMP_InputField tmpField) {
        if(initialized){
            Debug.LogError("Duplicate init, aborting!", this.gameObject);
            return;
        }
        this.tmpField = tmpField;
        this.initialized = true;
    }

    public void OnScroll (PointerEventData eventData) {
        if(!initialized){
            return;
        }
        if(field != null && field.interactable){
            if(float.TryParse(field.text, out var parsed)){
                float incAmount = eventData.scrollDelta.y * 0.1f * InputSystem.shiftCtrlMultiplier * scrollMultiplier;
                field.text = (parsed + incAmount).ToString();
                field.onEndEdit.Invoke(field.text);
            }
        }
        if(tmpField != null && tmpField.interactable){
            if(float.TryParse(tmpField.text, out var parsed)){
                float incAmount = eventData.scrollDelta.y * 0.1f * InputSystem.shiftCtrlMultiplier * scrollMultiplier;
                tmpField.text = (parsed + incAmount).ToString();
                tmpField.onEndEdit.Invoke(tmpField.text);
            }
        }
    }
	
}
