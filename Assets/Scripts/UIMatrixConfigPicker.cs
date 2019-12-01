using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIMatrixConfigPicker : MonoBehaviour {

    private static UIMatrixConfigPicker instance;

    [SerializeField] Image backgroundRaycastCatcher;

    System.Action<UIMatrixConfig> currentOnPickAction;
    RectTransform TEMP;

    void Awake () {
        if(instance != null){
            Debug.LogError($"Singleton violation! Instance of {nameof(UIMatrixConfigPicker)} wasn't null! Aborting...");
            return;
        }
        instance = this;
        backgroundRaycastCatcher.raycastTarget = true;
        backgroundRaycastCatcher.color = Color.clear;
        backgroundRaycastCatcher.gameObject.AddComponent(typeof(BackgroundRaycastCatcher));
        backgroundRaycastCatcher.gameObject.GetComponent<BackgroundRaycastCatcher>().Initialize(this);
        TEMP = new GameObject("asdf", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        TEMP.SetParent(backgroundRaycastCatcher.GetComponent<RectTransform>(), false);
        TEMP.sizeDelta = Vector2.one * 10f;
        TEMP.anchorMax = Vector2.zero;
        TEMP.anchorMin = Vector2.zero;
        TEMP.pivot = Vector2.one * 0.5f;
        Hide();
    }

    void OnDestroy () {
        if(instance == this){
            instance = null;
        }
    }

    public static void Open (System.Action<UIMatrixConfig> onConfigPicked) {
        instance.Unhide(onConfigPicked);
    }

    void Unhide (System.Action<UIMatrixConfig> onConfigPicked) {
        EventSystem.current.SetSelectedGameObject(null);    // TODO set the first button selected
        gameObject.SetActive(true);
        TEMP.anchoredPosition = Input.mousePosition;
        currentOnPickAction = onConfigPicked;
    }

    void Hide () {
        gameObject.SetActive(false);
    }

    void BackgroundClicked () {
        currentOnPickAction.Invoke(null);
        currentOnPickAction = null;
        Hide();
    }

    private class BackgroundRaycastCatcher : MonoBehaviour, IPointerClickHandler {
        
        private UIMatrixConfigPicker parent;

        public void Initialize (UIMatrixConfigPicker parent) {
            this.parent = parent;
        }

        public void OnPointerClick (PointerEventData eventData) {
            parent.BackgroundClicked();
        }
    }

}
