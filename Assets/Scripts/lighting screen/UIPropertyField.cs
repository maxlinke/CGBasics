﻿using UnityEngine;
using TMPro;

namespace LightingModels {

    public abstract class UIPropertyField : MonoBehaviour {

        [SerializeField] protected RectTransform m_rectTransform;
        [SerializeField] protected TextMeshProUGUI m_label;

        public RectTransform rectTransform => m_rectTransform;

        public ShaderProperty initProperty { get; protected set; }

        public virtual void LoadColors (ColorScheme cs) {
            m_label.color = cs.LightingScreenPropGroupPropertyLabels;
        }

        public abstract void ResetToDefault ();
    
    }

}