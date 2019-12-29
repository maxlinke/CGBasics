using UnityEngine;
using UnityEngine.UI;

public class MainMenuWindowOverlay : WindowOverlay {

    const string githubPage = "https://github.com/maxlinke";

    public Button closeButton { get; private set; }
    public Button resolutionButton { get; private set; }

    bool initialized = false;

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
            closeButton = CreateSpecialButton(UISprites.UITemp, "Close", "Close the application", () => {mainMenu.CloseRequested();});
            resolutionButton = CreateSpecialButton(UISprites.UITemp, "Resolution", "Choose a different resolution", () => {
                // TODO resolution picker thingy (foldout)
            });
            windowDresser.End();
        #endif
        this.initialized = true;
    }


}
