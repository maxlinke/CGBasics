using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace MatrixScreenUtils {

    public class CenterBottomPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        [Header("Components")]
        [SerializeField] RectTransform m_rectTransform;
        [SerializeField] RectTransform backgroundImageRT;
        [SerializeField] Image backgroundImage;
        [SerializeField] Toggle freeModeToggle;
        [SerializeField] RectTransform toggleRT;
        [SerializeField] Image toggleRing;
        [SerializeField] Image toggleCheckmark;
        [SerializeField] TextMeshProUGUI toggleLabel;
        [SerializeField] RectTransform dividerRT;
        [SerializeField] Image dividerImage;
        [SerializeField] RectTransform sliderArea;
        [SerializeField] RectTransform matrixSliderRT;
        [SerializeField] Slider matrixSlider;
        [SerializeField] TextMeshProUGUI sliderLabel;
        [SerializeField] Image sliderBackground;
        [SerializeField] Image sliderFill;

        [Header("Settings")]
        [SerializeField] float hiddenYOffset;
        [SerializeField] float toggleLabelMarginLeft;
        [SerializeField] float toggleLabelMarginRight;
        [SerializeField] float dividerLeftMargin;
        [SerializeField] float dividerRightMargin;
        [SerializeField] float matrixSliderMargin;
        [SerializeField] float sliderStepWidth;

        MatrixScreen matrixScreen;

        Vector2 defaultBGPos;
        Vector2 hiddenBGPos;

        public bool initialized { get; private set; }
        public RectTransform rectTransform => m_rectTransform;

        public void Initialize (MatrixScreen matrixScreen, System.Action<float> onSliderValueChanged) {
            this.matrixScreen = matrixScreen;
            defaultBGPos = backgroundImageRT.anchoredPosition;
            hiddenBGPos = new Vector2(defaultBGPos.x, defaultBGPos.y - rectTransform.rect.height + hiddenYOffset);
            matrixSlider.onValueChanged.AddListener((newVal) => {onSliderValueChanged?.Invoke(newVal);});
            freeModeToggle.isOn = matrixScreen.freeModeActivated;
            freeModeToggle.onValueChanged.AddListener((b) => {
                if(b){
                    matrixScreen.ActivateFreeMode();
                }else{
                    matrixScreen.ActivateNonFreeMode();
                }
            });
            toggleLabel.text = "Free Mode";
            toggleLabel.rectTransform.anchoredPosition = new Vector2(toggleLabelMarginLeft, 0f);
            toggleRT.SetSizeDeltaX(toggleLabel.preferredWidth + toggleLabelMarginLeft + toggleLabelMarginRight + toggleRing.GetComponent<RectTransform>().rect.width);
            dividerRT.anchoredPosition = new Vector2(toggleRT.rect.width + dividerLeftMargin, 0f);
            sliderLabel.text = "Matrix Weights";
            Hide();
            initialized = true;
        }

        public void Hide () {
            backgroundImageRT.anchoredPosition = hiddenBGPos;
            freeModeToggle.SetGOActive(false);
            dividerRT.SetGOActive(false);
            sliderArea.SetGOActive(false);
        }

        public void Show () {
            backgroundImageRT.anchoredPosition = defaultBGPos;
            freeModeToggle.SetGOActive(true);
            dividerRT.SetGOActive(true);
            sliderArea.SetGOActive(true);
        }

        public void UpdateSlider (int newMaxVal, bool alsoSetValue = false, int newValue = -1) {
            matrixSlider.maxValue = newMaxVal;
            if(alsoSetValue){
                matrixSlider.value = newValue;
                matrixSlider.onValueChanged.Invoke(matrixSlider.value);     // might not be necessary but it doesn't hurt or throw exceptions, so why bother?
            }
            matrixSliderRT.SetSizeDeltaX(newMaxVal * sliderStepWidth);
            matrixSliderRT.anchoredPosition = new Vector2(-matrixSliderMargin, 0f);
            var sliderAreaWidth = (sliderLabel.preferredWidth + 2 * matrixSliderMargin + matrixSliderRT.rect.width);
            rectTransform.SetSizeDeltaX(toggleRT.rect.width + dividerLeftMargin + dividerRightMargin + sliderAreaWidth);
            sliderArea.SetSizeDeltaX(sliderAreaWidth);
        }

        public void LoadColors (ColorScheme cs) {
            if(!initialized){
                return;
            }
            backgroundImage.color = cs.MatrixScreenBottomAreaBackground;
            Color fg = cs.MatrixScreenBottomAreaForegroundElement;
            sliderLabel.color = fg;
            toggleLabel.color = fg;
            toggleRing.color = fg;
            toggleCheckmark.color = fg;
            dividerImage.color = fg;
            sliderBackground.color = cs.MatrixScreenSliderBackground;
            sliderFill.color = cs.MatrixScreenSliderFill;
            matrixSlider.SetFadeTransition(0f, cs.MatrixScreenSliderHandle, cs.MatrixScreenSliderHandleHover, cs.MatrixScreenSliderHandleClick, Color.magenta);
            freeModeToggle.SetFadeTransition(0f, cs.MatrixScreenBottomAreaToggle, cs.MatrixScreenBottomAreaToggleHover, cs.MatrixScreenBottomAreaToggleClick, Color.magenta);
        }

        public void OnPointerEnter (PointerEventData eventData) {
            Show();
        }

        public void OnPointerExit (PointerEventData eventData) {
            Hide();
        }
    }

}   