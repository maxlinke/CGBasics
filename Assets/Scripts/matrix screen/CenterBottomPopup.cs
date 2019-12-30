using System.Collections.Generic;
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
        [SerializeField] Image outlineImage;
        [SerializeField] Toggle freeModeToggle;
        [SerializeField] RectTransform toggleRT;
        [SerializeField] Image toggleCheckmark;
        [SerializeField] TextMeshProUGUI toggleLabel;
        [SerializeField] TextMeshProUGUI toggleLabelDropShadow;
        [SerializeField] RectTransform dividerRT;
        [SerializeField] Image dividerImage;
        [SerializeField] RectTransform sliderArea;
        [SerializeField] RectTransform matrixSliderRT;
        [SerializeField] Slider matrixSlider;
        [SerializeField] TextMeshProUGUI sliderLabel;
        [SerializeField] TextMeshProUGUI sliderLabelDropShadow;
        [SerializeField] Image sliderBackground;
        [SerializeField] Image sliderFill;
        [SerializeField] GameObject expandArrowContainer;
        [SerializeField] Image expandArrow;
        [SerializeField] Image expandArrowDropShadow;

        [Header("Settings")]
        [SerializeField] float hiddenYOffset;
        [SerializeField] float toggleLabelMarginLeft;
        [SerializeField] float toggleLabelMarginRight;
        [SerializeField] float dividerLeftMargin;
        [SerializeField] float dividerRightMargin;
        [SerializeField] float matrixSliderMargin;
        [SerializeField] float sliderStepWidth;

        [Header("Notches")]
        [SerializeField] Sprite notchSprite;
        [SerializeField] float notchSize;

        MatrixScreen matrixScreen;

        Vector2 defaultBGPos;
        Vector2 hiddenBGPos;

        RectTransform notchParent;
        Color fillBackground;
        Color fillForeground;
        List<(RectTransform rt, Image img)> notches;

        public bool initialized { get; private set; }
        public Toggle FreeModeToggle => freeModeToggle;
        public RectTransform rectTransform => m_rectTransform;

        public void Initialize (MatrixScreen matrixScreen, bool freeModeInit, System.Action<bool> onFreeModeToggled, System.Action<float> onSliderValueChanged) {
            this.matrixScreen = matrixScreen;
            defaultBGPos = backgroundImageRT.anchoredPosition;
            hiddenBGPos = new Vector2(defaultBGPos.x, defaultBGPos.y - rectTransform.rect.height + hiddenYOffset);
            
            notchParent = new GameObject("Notch Parent", typeof(RectTransform)).GetComponent<RectTransform>();
            notchParent.SetParent(matrixSliderRT, false);
            notchParent.SetSiblingIndex(matrixSlider.handleRect.parent.GetSiblingIndex());
            notchParent.MatchOther((RectTransform)(matrixSlider.handleRect.parent), false);
            notches = new List<(RectTransform, Image)>();

            matrixSlider.onValueChanged.AddListener((newVal) => {
                UpdateNotchColors();
                onSliderValueChanged?.Invoke(newVal);
            });

            freeModeToggle.isOn = freeModeInit;
            freeModeToggle.onValueChanged.AddListener((b) => {
                onFreeModeToggled.Invoke(b);
            });
            toggleLabel.text = "Free Mode";
            toggleLabelDropShadow.text = toggleLabel.text;
            toggleLabel.rectTransform.anchoredPosition = new Vector2(toggleLabelMarginLeft, 0f);
            toggleLabelDropShadow.rectTransform.MatchOther(toggleLabel.rectTransform);
            toggleLabelDropShadow.rectTransform.anchoredPosition += new Vector2(1, -1);
            toggleRT.SetSizeDeltaX(toggleLabel.preferredWidth + toggleLabelMarginLeft + toggleLabelMarginRight + freeModeToggle.targetGraphic.GetComponent<RectTransform>().rect.width);
            dividerRT.anchoredPosition = new Vector2(toggleRT.rect.width + dividerLeftMargin, 0f);
            sliderLabel.text = "Matrix Weights";
            sliderLabelDropShadow.text = sliderLabel.text;
            sliderLabelDropShadow.rectTransform.MatchOther(sliderLabel.rectTransform);
            sliderLabelDropShadow.rectTransform.anchoredPosition += new Vector2(1, -1);

            freeModeToggle.gameObject.AddComponent<UIHoverEventCaller>().SetActions((ped) => {
                string message;
                if(freeModeToggle.isOn){
                    message = "Deactivate free mode and reset all matrices back to default.";
                }else{
                    message = "Enter free mode. All matrices become fully editable.";
                }
                BottomLog.DisplayMessage(message);
            }, (ped) => {
                BottomLog.ClearDisplay();
            });
            freeModeToggle.onValueChanged.AddListener((b) => {
                BottomLog.ClearDisplay();
            });

            matrixSlider.gameObject.AddComponent<UIHoverEventCaller>().SetActions((ped) => {
                BottomLog.DisplayMessage("Set the matrix weights. Unweighted matrices will be interpreted as identity.");
            }, (ped) => {
                BottomLog.ClearDisplay();
            });
            
            Hide();
            initialized = true;
        }

        public void Hide () {
            backgroundImageRT.anchoredPosition = hiddenBGPos;
            outlineImage.SetGOActive(false);
            freeModeToggle.SetGOActive(false);
            dividerRT.SetGOActive(false);
            sliderArea.SetGOActive(false);
            expandArrowContainer.SetActive(true);
        }

        public void Show () {
            backgroundImageRT.anchoredPosition = defaultBGPos;
            outlineImage.SetGOActive(true);
            freeModeToggle.SetGOActive(true);
            dividerRT.SetGOActive(true);
            sliderArea.SetGOActive(true);
            expandArrowContainer.SetActive(false);
        }

        public void UpdateSlider (int newMaxVal, bool alsoSetValue = false, int newValue = -1) {
            matrixSlider.maxValue = newMaxVal;
            UpdateNotchCount();
            UpdateNotchColors();
            if(alsoSetValue && newValue != -1){
                matrixSlider.value = newValue;
            }
            matrixSliderRT.SetSizeDeltaX(newMaxVal * sliderStepWidth);
            matrixSliderRT.anchoredPosition = new Vector2(-matrixSliderMargin, 0f);
            var sliderAreaWidth = (sliderLabel.preferredWidth + 2 * matrixSliderMargin + matrixSliderRT.rect.width);
            rectTransform.SetSizeDeltaX(toggleRT.rect.width + dividerLeftMargin + dividerRightMargin + sliderAreaWidth);
            sliderArea.SetSizeDeltaX(sliderAreaWidth);
        }

        void UpdateNotchCount () {
            int sliderMin = (int)(matrixSlider.minValue);
            int sliderMax = (int)(matrixSlider.maxValue);
            float iMax = sliderMax - sliderMin;
            for(int i=0; i<=iMax; i++){
                RectTransform notchRT;
                if(i>=notches.Count){
                    notchRT = new GameObject("Notch", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                    var notchImg = notchRT.GetComponent<Image>();
                    notchImg.sprite = notchSprite;
                    notches.Add((notchRT, notchImg));
                    notchRT.SetParent(notchParent, false);
                    notchRT.localScale = Vector3.one;
                    notchRT.pivot = 0.5f * Vector2.one;
                    notchRT.sizeDelta = notchSize * Vector2.one;
                }else{
                    notchRT = notches[i].rt;
                }
                notchRT.SetAnchor(((float)i) / iMax, 0.5f);
                notchRT.anchoredPosition = Vector2.zero;
            }
            for(int i=notches.Count-1; i>iMax; i--){
                Destroy(notches[i].rt.gameObject);
                notches.RemoveAt(i);
            }
        }

        void UpdateNotchColors () {
            for(int i=0; i<notches.Count; i++){
                notches[i].img.color = (i > matrixSlider.value) ? fillBackground : fillForeground;
            }
        }

        public void LoadColors (ColorScheme cs) {
            if(!initialized){
                return;
            }
            outlineImage.color = cs.MatrixScreenBottomAreaOutline;
            backgroundImage.color = cs.MatrixScreenBottomAreaBackground;
            dividerImage.color = cs.MatrixScreenBottomAreaDivider;
            Color fg = cs.MatrixScreenBottomAreaForegroundElement;
            sliderLabel.color = fg;
            toggleLabel.color = fg;
            toggleCheckmark.color = fg;
            Color ds = cs.MatrixScreenBottomAreaTextDropShadow;
            toggleLabelDropShadow.color = ds;
            sliderLabelDropShadow.color = ds;
            fillBackground = cs.MatrixScreenSliderBackground;
            fillForeground = cs.MatrixScreenSliderFill;
            sliderBackground.color = cs.MatrixScreenSliderBackground;
            sliderFill.color = cs.MatrixScreenSliderFill;
            matrixSlider.SetFadeTransition(0f, cs.MatrixScreenSliderHandle, cs.MatrixScreenSliderHandleHover, cs.MatrixScreenSliderHandleClick, Color.magenta);
            freeModeToggle.SetFadeTransition(0f, cs.MatrixScreenBottomAreaToggle, cs.MatrixScreenBottomAreaToggleHover, cs.MatrixScreenBottomAreaToggleClick, Color.magenta);
            expandArrow.color = sliderFill.color;
            expandArrowDropShadow.color = ds;
            UpdateNotchColors();
        }
        public void OnPointerEnter (PointerEventData eventData) {
            Show();
        }

        public void OnPointerExit (PointerEventData eventData) {
            Hide();
        }
    }

}   