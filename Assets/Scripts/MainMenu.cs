using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class MainMenu : MonoBehaviour {
    
    [Header("Components")]
    [SerializeField] Canvas canvas;

    [Header("Button Settings")]
    [SerializeField] RectTransform buttonParent;
    [SerializeField] GameObject buttonTemplate;
    [SerializeField, Range(0, 20)] float buttonVerticalMargin;

    [Header("Outside References")]
    [SerializeField] GameObject backgroundContainer;

    List<Button> mainButtons;

    void Awake () {
        mainButtons = new List<Button>();
        PopulateButtonList();
        buttonTemplate.SetActive(false);


        void PopulateButtonList () {
            float newButtonY = 0;
            float buttonHeight = ((RectTransform)(buttonTemplate.transform)).sizeDelta.y;
            //from the bottom up
            //TODO access menus from menu singleton (reference to all canvases)
            //TODO also load these labels from file
            CreateListButton("Quit", () => Application.Quit());
            CreateListButton("Settings", () => {});
            CreateListButton("Fragment Shaders", () => {});
            CreateListButton("Vertex Shaders", () => {});

            void CreateListButton (string title, Action onClick) {
                var newButton = Instantiate(buttonTemplate).GetComponent<Button>();
                newButton.onClick.AddListener(() => onClick());
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

    public void Show () {
        gameObject.SetActive(true);
        backgroundContainer.SetActive(true);
    }

    public void Hide () {
        gameObject.SetActive(false);
        backgroundContainer.SetActive(false);
    }

}
