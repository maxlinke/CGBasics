using System.Collections.Generic;
using UnityEngine;

public class ModelPicker : MonoBehaviour {

    [Tooltip("These are at the beginning of the list"), SerializeField] ModelPreset[] interestingModelPresets;
    [Tooltip("These are at the end of the list"), SerializeField] ModelPreset[] boringModelPresets;
    [SerializeField] bool logInfoOnLoad;

    static List<LoadedModel> loadedModels;

    void Awake () {
        if(loadedModels != null){
            Debug.LogError("Apparent singleton violation! Aborting...", this.gameObject);
            return;
        }
        loadedModels = new List<LoadedModel>();
        LoadPresets(interestingModelPresets);
        LoadPresets(boringModelPresets);

        void LoadPresets (IEnumerable<ModelPreset> inputCollection) {
            foreach(var preset in inputCollection){
                #if UNITY_WEBGL
                    if(preset.includeInWebGLBuilds){
                        var lm = new LoadedModel(preset);
                        if(logInfoOnLoad){
                            lm.mesh.LogInfo();
                        }
                        loadedModels.Add(lm);
                    }
                #else
                    var lm = new LoadedModel(preset);
                    if(logInfoOnLoad){
                        lm.mesh.LogInfo();
                    }
                    loadedModels.Add(lm);
                #endif
            }
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
    public readonly Mesh flatMesh;
    public readonly string name;
    public readonly Color color;
    public readonly Color specularColor;
    public readonly string description;

    public LoadedModel (Mesh mesh, string name, Color color, Color specularColor, string description) {
        this.mesh = mesh;
        this.flatMesh = CreateFlatShadedMesh(mesh);
        this.name = name;
        this.color = color;
        this.specularColor = specularColor;
        this.description = description;
    }

    public LoadedModel (ModelPreset preset) {
        this.mesh = preset.mesh;
        this.flatMesh = (preset.flatMesh != null) ? preset.flatMesh : CreateFlatShadedMesh(preset.mesh);
        this.name = preset.name;
        this.color = preset.color;
        this.specularColor = preset.specColor;
        this.description = preset.description;
    }

    Mesh CreateFlatShadedMesh (Mesh inputMesh) {
        Debug.Log("creating flat mesh. this might take a while...");
        // unity wiky says apply the triangles last.
        var output = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        for(int i=0; i<inputMesh.triangles.Length; i+=3){
            var v1 = inputMesh.vertices[inputMesh.triangles[i+0]];
            var v2 = inputMesh.vertices[inputMesh.triangles[i+1]];
            var v3 = inputMesh.vertices[inputMesh.triangles[i+2]];
            var n = Vector3.Cross(v2-v1, v3-v2);
            vertices.AddRange(new Vector3[]{v1, v2, v3});
            normals.AddRange(new Vector3[]{n, n, n});
            triangles.AddRange(new int[]{i, i+1, i+2});
        }
        output.hideFlags = HideFlags.HideAndDontSave;
        output.vertices = vertices.ToArray();
        output.normals = normals.ToArray();
        output.triangles = triangles.ToArray();
        return output;
    }

}