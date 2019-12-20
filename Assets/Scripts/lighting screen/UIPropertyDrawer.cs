using UnityEngine;
using TMPro;

namespace LightingModels {

    public abstract class UIPropertyDrawer : MonoBehaviour {

        [SerializeField] protected RectTransform m_rectTransform;
        [SerializeField] protected TextMeshProUGUI m_label;

        public RectTransform rectTransform => m_rectTransform;

        public abstract void LoadColors (ColorScheme cs);
    
    }

}