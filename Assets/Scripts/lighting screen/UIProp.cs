﻿using UnityEngine;
using TMPro;

namespace LightingModels {

    public class UIProp : MonoBehaviour {

        [SerializeField] RectTransform m_rectTransform;
        [SerializeField] TextMeshProUGUI label;

        public RectTransform rectTransform => m_rectTransform;

        // public void Initialize (string name, SOMETHING propDriver) {     // TODO all this

        // }
    
    }

}