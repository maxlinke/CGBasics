using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour {
    
    [Header("Prefabs")]
    [SerializeField] MatrixScreen matrixScreenPrefab;
    [SerializeField] LightingScreen lightingScreenPrefab;

    [Header("Button Settings")]
    [SerializeField] RectTransform buttonParent;
    [SerializeField] GameObject buttonTemplate;
    [SerializeField, Range(0, 20)] float buttonVerticalMargin;

    List<Button> mainButtons;

    void Awake () {
        mainButtons = new List<Button>();
        PopulateButtonList();
        buttonTemplate.SetActive(false);


        void PopulateButtonList () {
            float newButtonY = 0;
            float buttonHeight = ((RectTransform)(buttonTemplate.transform)).sizeDelta.y;
            //from the bottom up
            CreateListButton("Quit", () => Application.Quit());
            CreateListButton("Settings", null);
            CreateListButton("Lighting Models", () => {OpenScreen(lightingScreenPrefab);});
            CreateListButton("Transformation Matrices", () => {OpenScreen(matrixScreenPrefab);});

            void CreateListButton (string title, System.Action onClick) {
                var newButton = Instantiate(buttonTemplate).GetComponent<Button>();
                newButton.onClick.AddListener(() => onClick?.Invoke());
                var newButtonRT = (RectTransform)(newButton.transform);
                newButtonRT.SetParent(buttonParent, false);
                newButtonRT.anchoredPosition = new Vector2(newButtonRT.anchoredPosition.x, newButtonY);
                var newButtonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                newButtonText.text = title;
                newButton.gameObject.name = buttonTemplate.name + $" ({title})";
                newButton.gameObject.SetActive(true);
                mainButtons.Add(newButton);
                newButtonY += buttonHeight + buttonVerticalMargin;
            }
        }

    }

    // ---------------- TEMP ------------------------

    public void OpenScreen (CloseableScreen screenPrefab) {
        gameObject.SetActive(false);
        var newScreen = Instantiate(screenPrefab);
        newScreen.SetupCloseAction(() => {
            Destroy(newScreen.gameObject);
            gameObject.SetActive(true);
        });
    }

}
