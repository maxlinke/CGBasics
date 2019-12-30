using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuWindowOverlay : WindowOverlay {

    const string githubPage = "https://github.com/maxlinke";
    const KeyCode closeConfirm = KeyCode.Y;
    const KeyCode closeAbort = KeyCode.N;
    
    bool abortIsFirstOption = true;

    public Button closeButton { get; private set; }
    public Button resolutionButton { get; private set; }

    bool initialized = false;
    bool closeDropdownOpen = false;

    public void Initialize (MainMenu mainMenu) {
        if(initialized){
            Debug.LogError("Duplicate init call, aborting!");
            return;
        }
        InitializeLists();
        CreateResetButtonAndLabel("Created by Max Linke", githubPage, () => {Application.OpenURL(githubPage);});
        resetButtonIcon.sprite = UISprites.UIInfo;
        #if !UNITY_WEBGL
            windowDresser.Begin(uiParent, Vector2.one, Vector2.down, Vector2.zero);
            closeButton = CreateSpecialButton(UISprites.MatrixDelete, "Close", "Close the application", () => { CloseRequested(); });
            resolutionButton = CreateSpecialButton(UISprites.UIConfig, "Resolution", "Choose a different resolution", () => {
                List<Foldout.ButtonSetup> setups = new List<Foldout.ButtonSetup>();
                var fullScreenButtonName = (Screen.fullScreen ? "Go Windowed" : "Go Fullscreen");
                var fullScreenButtonSetValue = !Screen.fullScreen;
                setups.Add(new Foldout.ButtonSetup(
                    buttonName: fullScreenButtonName,
                    buttonHoverMessage: fullScreenButtonName,
                    buttonClickAction: () => {Screen.fullScreen = fullScreenButtonSetValue;},
                    buttonInteractable: true
                ));
                foreach(var res in Screen.resolutions){
                    string sizeString = $"{res.width}x{res.height}";
                    string refreshString = $"{res.refreshRate}Hz";
                    var resCopy = res;
                    setups.Add(new Foldout.ButtonSetup(
                        // buttonName: $"{sizeString} <size=67%>{refreshString}</size>",
                        buttonName: $"{sizeString} ({refreshString})",
                        buttonHoverMessage: $"Update the resolution to {sizeString} pixels",
                        buttonClickAction: () => {
                            Screen.SetResolution(resCopy.width, resCopy.height, Screen.fullScreen, resCopy.refreshRate);
                        }, buttonInteractable: true
                    ));
                }
                Foldout.Create(setups, null);
            });
            windowDresser.End();
        #endif
        this.initialized = true;
    }

    public void CloseRequested () {
        if(closeDropdownOpen){
            Debug.LogError("Duplicate close requests, aborting!");
            return;
        }
        var setups = new List<Foldout.ButtonSetup>();
        var abortOption = new Foldout.ButtonSetup(
            buttonName: $"[{closeAbort}] Abort",
            buttonHoverMessage: "Don't close",
            buttonClickAction: AbortClose,
            buttonInteractable: true
        );
        var confirmOption = new Foldout.ButtonSetup(
            buttonName: $"[{closeConfirm}] Confirm",
            buttonHoverMessage: "Exit the application",
            buttonClickAction: CloseApplication,
            buttonInteractable: true
        );
        if(abortIsFirstOption){
            setups.Add(abortOption);
            setups.Add(confirmOption);
        }else{
            setups.Add(confirmOption);
            setups.Add(abortOption);
        }
        Foldout.Create(setups, AbortClose);
        closeDropdownOpen = true;
    }

    void Update () {
        if(closeDropdownOpen){
            if(Input.GetKeyDown(closeConfirm)){
                Foldout.GetInstance()[abortIsFirstOption ? 1 : 0].onClick.Invoke();
            }else if(Input.GetKeyDown(closeAbort)){
                Foldout.GetInstance()[abortIsFirstOption ? 0 : 1].onClick.Invoke();
            }
        }
    }

    void CloseApplication () {
        closeDropdownOpen = false;
        #if UNITY_EDITOR
            Debug.Log("I'd close this if I could.");
        #else
            Application.Quit;
        #endif
    }

    void AbortClose () {
        closeDropdownOpen = false;
        #if UNITY_EDITOR
            Debug.Log("Abort requested.");
        #endif
    }

}
