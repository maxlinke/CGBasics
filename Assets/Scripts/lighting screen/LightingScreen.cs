using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO class "propertyGroup" or something like that
// has a header, a "description" (optional image?) and a list of properties i guess (action to setup) AND an optional "configure"-button (mode, diffuse, specular but NOT light)

public class LightingScreen : MonoBehaviour {

    private enum LM {
        None,           // diff: solid color, spec: null
        DiffLambert,
        DiffOrenNayer,
        DiffMinnaert,
        DiffWrap,
        SpecPhong,
        SpecBlinnPhong,
        SpecCookTorr,
        SpecWardIso,
        SpecWardAniso
    }

    // class for lighting model? has name (for foldout) and material?
    // two dictionaries<LM, LightingModel> ? diff and spec...

    [Header("Components")]
    [SerializeField] Image[] borders;

    [Header("Lighting Models")]
    [SerializeField] LightingModel solidColorLM;
    [SerializeField] LightingModel[] lightingModels;

    MeshRenderer targetMR;
    MaterialPropertyBlock mpb;

    Color diffColor;
    Color specColor;
    // also sliders/scrollableinputfields for intensity? automatically clamp value (in inputfield) to [0, Infinity]?
    Color ambientLightColor;
    Color mainLightColor;
    Color backLightColor;

    void Start () {
        
    }

    void Update () {
        
        SetupMaterialPropertyBlock();
        targetMR.SetPropertyBlock(mpb);
    }

    void SetupMaterialPropertyBlock () {
        if(mpb == null){
            mpb = new MaterialPropertyBlock();
        }
        mpb.SetColor(ShaderProps.diffuseColor.propID, diffColor);
        mpb.SetColor(ShaderProps.specularColor.propID, specColor);
        // TODO the floats
    }

    Material CreateDiffuseMaterial (Shader shader) {
        var newMat = new Material(shader);
        newMat.hideFlags = HideFlags.HideAndDontSave;
        newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        newMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        newMat.SetInt("_ZWrite", 1);
        newMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        return newMat;
    }

    Material CreateSpecularMaterial (Shader shader) {
        var newMat = new Material(shader);
        newMat.hideFlags = HideFlags.HideAndDontSave;
        newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        newMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        newMat.SetInt("_ZWrite", 0);
        newMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Equal);
        return newMat;
    }

    void CreateModelGroup () {

    }

    void CreateLightGroup () {

    }

    void CreateDiffuseGroup () {
        // enable config button (() => {...});
        // create slider ("_Roughness", 0, 1, (value) => {this.roughness = value;});
    }

    void CreateSpecularGroup () {

    }
	
}
