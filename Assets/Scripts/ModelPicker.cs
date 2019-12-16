using System.Collections.Generic;
using UnityEngine;

public class ModelPicker : MonoBehaviour {

    [SerializeField] ModelPreset[] modelPresets;

    static List<LoadedModel> loadedModels;

    void Awake () {
        if(loadedModels != null){
            Debug.LogError("Apparent singleton violation! Aborting...", this.gameObject);
            return;
        }
        loadedModels = new List<LoadedModel>();
        foreach(var preset in modelPresets){
            loadedModels.Add(new LoadedModel(preset));
        }
        // TODO streamingassets maybe (color hue from hash, then hsv with a fairly fixed value and saturation range)
    }

    void OnDestroy () {
        loadedModels = null;
    }

    public static void Open (System.Action<Mesh, string> onMeshPicked, float scale) {
        var buttonSetups = new List<Foldout.ButtonSetup>();
        foreach(var defaultMesh in loadedModels){
            string meshName = defaultMesh.name;
            Mesh mesh = defaultMesh.mesh;
            buttonSetups.Add(new Foldout.ButtonSetup(
                buttonName: meshName,
                buttonHoverMessage: meshName,
                buttonClickAction: () => {onMeshPicked?.Invoke(mesh, meshName);},
                buttonInteractable: true
            ));
        }
        Foldout.Create(buttonSetups, () => {onMeshPicked?.Invoke(null, null);}, scale);
    }
	
}

public class LoadedModel {
    
    public readonly Mesh mesh;
    public readonly string name;
    public readonly Color color;

    public LoadedModel (Mesh mesh, string name, Color color) {
        this.mesh = mesh;
        this.name = name;
        this.color = color;
    }

    public LoadedModel (ModelPreset preset) {
        this.mesh = preset.mesh;
        this.name = preset.name;
        this.color = preset.color;
    }

}