using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIMatrixConfigPicker : MonoBehaviour {

    [SerializeField] Image backgroundRaycastCatcher;

    void Awake () {
        backgroundRaycastCatcher.raycastTarget = true;
        backgroundRaycastCatcher.color = Color.clear;
        // backgroundRaycastCatcher.gameObject.AddComponent
    }

    public static void Open (System.Action<UIMatrixConfig> onConfigPicked) {
        EventSystem.current.SetSelectedGameObject(null);    // TODO set the first button selected
    }
	
}
