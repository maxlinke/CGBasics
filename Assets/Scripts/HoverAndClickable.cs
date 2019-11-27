using UnityEngine;
using UnityEngine.EventSystems;

public class HoverAndClickable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    System.Action<PointerEventData> onClick;
    System.Action<PointerEventData> onPointerEnter;
    System.Action<PointerEventData> onPointerExit;

    public void Initialize (System.Action<PointerEventData> onClick, System.Action<PointerEventData> onPointerEnter, System.Action<PointerEventData> onPointerExit) {
        this.onClick = onClick;
        this.onPointerEnter = onPointerEnter;
        this.onPointerExit = onPointerExit;
    }

    public void OnPointerClick (PointerEventData eventData) {
        onClick(eventData);
    }

    public void OnPointerEnter (PointerEventData eventData) {
        onPointerEnter(eventData);
    }

    public void OnPointerExit (PointerEventData eventData) {
        onPointerExit(eventData);
    }

}
