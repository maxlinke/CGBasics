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

        [Header("Notches")]
        [SerializeField] Sprite notchSprite;
        [SerializeField] Vector2 sliderNotchSize;
        [SerializeField] Vector2 sliderNotchOffset;     // TODO make some screenshots of the things i tried before ending up at the fancy final solution
        [SerializeField] bool doubleSidedNotches;

        MatrixScreen matrixScreen;

        Vector2 defaultBGPos;
        Vector2 hiddenBGPos;

        bool onValueChangedBlocked = false;
        RectTransform notchParent;
        Color fillBackground;
        Color fillForeground;
        List<Image> notches;

        public bool initialized { get; private set; }
        public RectTransform rectTransform => m_rectTransform;

        public void Initialize (MatrixScreen matrixScreen, System.Action<float> onSliderValueChanged) {
            this.matrixScreen = matrixScreen;
            defaultBGPos = backgroundImageRT.anchoredPosition;
            hiddenBGPos = new Vector2(defaultBGPos.x, defaultBGPos.y - rectTransform.rect.height + hiddenYOffset);
            
            notchParent = new GameObject("Notch Parent", typeof(RectTransform)).GetComponent<RectTransform>();
            notchParent.SetParent(matrixSliderRT, false);
            notchParent.SetAsFirstSibling();
            notchParent.MatchOther((RectTransform)(matrixSlider.handleRect.parent), false);
            notches = new List<Image>();

            matrixSlider.onValueChanged.AddListener((newVal) => {
                if(!onValueChangedBlocked){
                    DeleteOldAndCreateNewNotches();         // TODO a more efficient solution maybe if i feel like it
                    onSliderValueChanged?.Invoke(newVal);
                }
            });

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
            DeleteOldAndCreateNewNotches();
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

        void DeleteOldAndCreateNewNotches () {
            onValueChangedBlocked = true;
            for(int i=notchParent.childCount-1; i>=0; i--){
                Destroy(notchParent.GetChild(i).gameObject);
            }
            notches.Clear();
            int origVal = (int)(matrixSlider.value);
            CreateNotches(1, origVal);
            if(doubleSidedNotches){
                CreateNotches(-1, origVal);
            }
            matrixSlider.value = origVal;
            onValueChangedBlocked = false;

            void CreateNotches (int yMultiplier, int actualSliderValue) {
                for(int i=(int)(matrixSlider.minValue); i<= matrixSlider.maxValue; i++){
                    matrixSlider.value = i;
                    var newNotchRT = new GameObject("Notch", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                    newNotchRT.SetParent(notchParent, false);
                    newNotchRT.localScale = new Vector3(1, yMultiplier, 1);
                    newNotchRT.SetAnchor(matrixSlider.handleRect.AverageAnchor());
                    newNotchRT.pivot = matrixSlider.handleRect.pivot;
                    newNotchRT.anchoredPosition = matrixSlider.handleRect.anchoredPosition + Vector2.Scale(sliderNotchOffset, new Vector2(1, yMultiplier));
                    newNotchRT.sizeDelta = sliderNotchSize;
                    var newNotch = newNotchRT.GetComponent<Image>();
                    newNotch.sprite = notchSprite;
                    newNotch.raycastTarget = false;
                    if(i<=actualSliderValue){
                        newNotch.color = fillForeground;
                    }else{
                        newNotch.color = fillBackground;
                    }
                    notches.Add(newNotch);
                }
            }
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
            fillBackground = cs.MatrixScreenSliderBackground;
            fillForeground = cs.MatrixScreenSliderFill;
            sliderBackground.color = cs.MatrixScreenSliderBackground;
            sliderFill.color = cs.MatrixScreenSliderFill;
            matrixSlider.SetFadeTransition(0f, cs.MatrixScreenSliderHandle, cs.MatrixScreenSliderHandleHover, cs.MatrixScreenSliderHandleClick, Color.magenta);
            freeModeToggle.SetFadeTransition(0f, cs.MatrixScreenBottomAreaToggle, cs.MatrixScreenBottomAreaToggleHover, cs.MatrixScreenBottomAreaToggleClick, Color.magenta);
            DeleteOldAndCreateNewNotches();
        }

        public void OnPointerEnter (PointerEventData eventData) {
            Show();
        }

        public void OnPointerExit (PointerEventData eventData) {
            Hide();
        }
    }

}   