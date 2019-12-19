using UnityEngine;
using UnityEngine.EventSystems;

public class UIBackgroundAbortRaycastCatcher : MonoBehaviour, IPointerClickHandler {

    public event System.Action onClick = delegate {};

    public void OnPointerClick (PointerEventData eventData) {
        onClick.Invoke();
    }
}
