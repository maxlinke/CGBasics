using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverEventCaller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    System.Action<PointerEventData> onHoverEnter;
    System.Action<PointerEventData> onHoverExit;

    public void SetActions (System.Action<PointerEventData> onHoverEnter, System.Action<PointerEventData> onHoverExit) {
        this.onHoverEnter = onHoverEnter;
        this.onHoverExit = onHoverExit;
    }

    public void ResetActions () {
        this.onHoverEnter = null;
        this.onHoverExit = null;
    }

    public void OnPointerEnter (PointerEventData eventData) {
        onHoverEnter?.Invoke(eventData);
    }

    public void OnPointerExit (PointerEventData eventData) {
        onHoverExit?.Invoke(eventData);
    }

}
