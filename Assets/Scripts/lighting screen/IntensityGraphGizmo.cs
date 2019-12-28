using UnityEngine;
using UnityEngine.UI;

namespace LightingModels {

    public class IntensityGraphGizmo : MonoBehaviour {

        [SerializeField] RectTransform m_rectTransform;
        [SerializeField] RectTransform mainGizmoRT;
        [SerializeField] Image mainGizmo;
        [SerializeField] Image mainDropShadow;
        [SerializeField] RectTransform arrowGizmoRT;
        [SerializeField] RectTransform mainArrowRT;
        [SerializeField] Image mainArrow;
        [SerializeField] RectTransform arrowDropShadowRT;
        [SerializeField] Image arrowDropShadow;

        public RectTransform rectTransform => m_rectTransform;

        public void SetSprite (Sprite sprite) {
            mainGizmo.sprite = sprite;
            mainDropShadow.sprite = sprite;
        }

        public void LoadColors (ColorScheme cs) {
            var fg = cs.LSIGUIElements;
            var bg = cs.LSIGUIElementDropShadow;
            mainGizmo.color = fg;
            mainArrow.color = fg;
            mainDropShadow.color = bg;
            arrowDropShadow.color = bg;
        }

        public void SetRotation (float angle) {
            var euler = new Vector3(0, 0, angle);
            rectTransform.localEulerAngles = euler;
            mainGizmoRT.localEulerAngles = -euler;
            arrowGizmoRT.localEulerAngles = -euler;
            mainArrowRT.localEulerAngles = euler;
            arrowDropShadowRT.localEulerAngles = euler;
        }
        
    }

}