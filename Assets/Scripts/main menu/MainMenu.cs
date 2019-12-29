using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour {
    
    [Header("Components")]
    [SerializeField] MainMenuWindowOverlay windowOverlay;

    [Header("Prefabs")]
    [SerializeField] MatrixScreen matrixScreenPrefab;
    [SerializeField] LightingScreen lightingScreenPrefab;

    [Header("TEMP")]
    [SerializeField] Button matrixButton;
    [SerializeField] Button lightingButton;

    bool initialized = false;
    List<Button> mainButtons;

    void Start () {
        Initialize();
    }

    void Initialize () {
        if(initialized){
            Debug.LogError("Duplicate init call! Aborting...");
            return;
        }
        mainButtons = new List<Button>();
        // PopulateButtonList();
        matrixButton.onClick.AddListener(() => {OpenScreen(matrixScreenPrefab);});
        lightingButton.onClick.AddListener(() => {OpenScreen(lightingScreenPrefab);});
        windowOverlay.Initialize(this);
        this.initialized = true;
        LoadColors(ColorScheme.current);

        void PopulateButtonList () {
            // float newButtonY = 0;
            // float buttonHeight = ((RectTransform)(buttonTemplate.transform)).sizeDelta.y;
            //from the bottom up
            CreateListButton("Quit", () => Application.Quit());
            CreateListButton("Settings", null);
            CreateListButton("Lighting Models", () => {OpenScreen(lightingScreenPrefab);});
            CreateListButton("Matrix Transformations", () => {OpenScreen(matrixScreenPrefab);});
            CreateListButton("TEST", () => {Application.OpenURL("https://github.com/maxlinke/CGBasics/releases");});

            void CreateListButton (string title, System.Action onClick) {
                // var newButton = Instantiate(buttonTemplate).GetComponent<Button>();
                // newButton.onClick.AddListener(() => onClick?.Invoke());
                // var newButtonRT = (RectTransform)(newButton.transform);
                // newButtonRT.SetParent(buttonParent, false);
                // newButtonRT.anchoredPosition = new Vector2(newButtonRT.anchoredPosition.x, newButtonY);
                // var newButtonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                // newButtonText.text = title;
                // newButton.gameObject.name = buttonTemplate.name + $" ({title})";
                // newButton.gameObject.SetActive(true);
                // mainButtons.Add(newButton);
                // newButtonY += buttonHeight + buttonVerticalMargin;
            }
        }

        void OpenScreen (CloseableScreen screenPrefab) {
            gameObject.SetActive(false);
            var newScreen = Instantiate(screenPrefab);
            newScreen.SetupCloseAction(() => {
                Destroy(newScreen.gameObject);
                gameObject.SetActive(true);
            });
        }
    }

    void OnEnable () {
        if(!initialized){
            return;
        }
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColors;
    }

    void LoadColors (ColorScheme cs) {
        windowOverlay.LoadColors(cs);
    }

    public void CloseRequested () {
        Debug.Log("close requested");
        // TODO open overlay canvas, temporarily disable the buttons (or set EventSystem.currentlyselected to null...) (could be done by the "really close?" dialog by setting NO as default)
    }

}
