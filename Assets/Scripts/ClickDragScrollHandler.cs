using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ClickDragScrollHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IScrollHandler {

    protected abstract void PointerDown (PointerEventData ped);
    protected abstract void PointerUp (PointerEventData ped);
    protected abstract void Scroll (PointerEventData ped);

    public void OnPointerDown (PointerEventData eventData) {
        PointerDown(eventData);
    }

    public void OnPointerUp (PointerEventData eventData) {
        PointerUp(eventData);
    }

    public void OnScroll (PointerEventData eventData) {
        Scroll(eventData);
    }

    protected PointerType PointerIDToType (int id) {
        switch(id){
            case -1: return PointerType.Left;
            case -2: return PointerType.Right;
            case -3: return PointerType.Middle;
            default: return PointerType.None;
        }
    }

    protected enum PointerType {
        None,
        Left,
        Right,
        Middle
    }
	
}
