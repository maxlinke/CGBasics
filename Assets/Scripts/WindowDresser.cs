using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WindowDresser : MonoBehaviour {

    [Header("Label")]
    [SerializeField] TMP_FontAsset labelFont;
    [SerializeField] float labelFontSize;

    [Header("Buttons")]
    [SerializeField] float buttonSize;
    [SerializeField] float buttonOffset;
    [SerializeField] float buttonSpaceOffset;

    bool ready = false;

    RectTransform currentParentRT;
    Vector2 currentAnchorAndPivot;
    Vector2 currentOffsetDirection;
    Vector2 currentPos;

    public void Begin (RectTransform parentRT, Vector2 anchorAndPivot, Vector2 offsetDirection, Vector2 startPos) {
        if(ready){
            Debug.LogError("Already set up. Call end before beginning again! Aborting...", this.gameObject);
            return;
        }
        currentParentRT = parentRT;
        currentAnchorAndPivot = anchorAndPivot;
        currentOffsetDirection = offsetDirection;
        currentPos = startPos;
        ready = true;
    }

    public void End () {
        if(!ready){
            Debug.LogWarning("Nothing to end here...");
        }
        ready = false;
    }

    public RectTransform CreateCircleWithIcon (Sprite icon, string thingName, string hoverMessage, out Image iconImage, out Image backgroundImage, bool insertExtraSpaceAfter = false) {
        // main creation
        var newThingRT = new GameObject(thingName, typeof(RectTransform), typeof(Image), typeof(UIHoverEventCaller)).GetComponent<RectTransform>();
        SetupRectTransform(newThingRT);
        newThingRT.sizeDelta = Vector3.one * buttonSize;
        // hover init
        var newThingHover = newThingRT.gameObject.GetComponent<UIHoverEventCaller>();
        newThingHover.SetActions(
            onHoverEnter: (ped) => {BottomLog.DisplayMessage(hoverMessage);},
            onHoverExit: (ped) => {BottomLog.ClearDisplay();}
        );
        // initializing and assigning the background image
        backgroundImage = newThingRT.gameObject.GetComponent<Image>();
        backgroundImage.sprite = UISprites.UICircle;
        backgroundImage.raycastTarget = true;
        // the icon
        var newThingIconRT = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        newThingIconRT.SetParent(newThingRT, false);
        newThingIconRT.localScale = Vector3.one;
        newThingIconRT.SetToFill();
        // initializing and assigning the icon image
        iconImage = newThingIconRT.gameObject.GetComponent<Image>();
        iconImage.sprite = icon;
        iconImage.raycastTarget = false;
        // updating the position
        var rawOffset = Vector2.Scale(currentOffsetDirection, new Vector2(newThingRT.rect.width, newThingRT.rect.width));
        currentPos += rawOffset + (currentOffsetDirection * (buttonOffset + (insertExtraSpaceAfter ? buttonSpaceOffset : 0)));
        // output
        return newThingRT;
    }

    public TextMeshProUGUI CreateLabel () {
        var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        SetupRectTransform(label.rectTransform);
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Overflow;
        label.font = labelFont;
        label.fontSize = labelFontSize;
        return label;
    }

    void SetupRectTransform (RectTransform rt) {
        rt.SetParent(currentParentRT, false);
        rt.localScale = Vector3.one;
        rt.SetAnchor(currentAnchorAndPivot);
        rt.pivot = currentAnchorAndPivot;
        rt.anchoredPosition = currentPos;
    }
	
}
