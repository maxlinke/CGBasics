using System.Collections.Generic;
using UnityEngine;

public class ModelPicker : MonoBehaviour {

    [Tooltip("These are at the beginning of the list"), SerializeField] ModelPreset[] interestingModelPresets;
    [Tooltip("These are at the end of the list"), SerializeField] ModelPreset[] boringModelPresets;

    static List<LoadedModel> loadedModels;

    void Awake () {
        if(loadedModels != null){
            Debug.LogError("Apparent singleton violation! Aborting...", this.gameObject);
            return;
        }
        loadedModels = new List<LoadedModel>();
        foreach(var preset in interestingModelPresets){
            loadedModels.Add(new LoadedModel(preset));
        }
        foreach(var preset in boringModelPresets){
            loadedModels.Add(new LoadedModel(preset));
        }
        // TODO streamingassets maybe (color hue from hash, then hsv with a fairly fixed value and saturation range)
    }

    void OnDestroy () {
        loadedModels = null;
    }

    private static List<Foldout.ButtonSetup> GetButtonSetups (System.Action<LoadedModel> buttonClick) {
        var buttonSetups = new List<Foldout.ButtonSetup>();
        foreach(var model in loadedModels){
            string modelName = model.name;
            string modelDesc = model.description;
            LoadedModel modelCopy = model;
            buttonSetups.Add(new Foldout.ButtonSetup(
                buttonName: modelName,
                buttonHoverMessage: modelDesc,
                buttonClickAction: () => {buttonClick?.Invoke(modelCopy);},
                buttonInteractable: true
            ));
        }
        return buttonSetups;
    }

    public static void Open (System.Action<Mesh, string> onMeshPicked, float scale) {
        var buttonSetups = GetButtonSetups((m) => {onMeshPicked?.Invoke(m.mesh, m.name);});
        Foldout.Create(buttonSetups, () => {onMeshPicked?.Invoke(null, null);}, scale);
    }

    public static void Open (System.Action<LoadedModel> onModelPicked, float scale) {
        var buttonSetups = GetButtonSetups(onModelPicked);
        Foldout.Create(buttonSetups, () => {onModelPicked?.Invoke(null);}, scale);
    }
	
}

public class LoadedModel {
    
    public readonly Mesh mesh;
    public readonly string name;
    public readonly Color color;
    public readonly Color specularColor;
    public readonly string description;

    public LoadedModel (Mesh mesh, string name, Color color, Color specularColor, string description) {
        this.mesh = mesh;
        this.name = name;
        this.color = color;
        this.specularColor = specularColor;
        this.description = description;
    }

    public LoadedModel (ModelPreset preset) {
        this.mesh = preset.mesh;
        this.name = preset.name;
        this.color = preset.color;
        this.specularColor = preset.specColor;
        this.description = preset.description;
    }

}