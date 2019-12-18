using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO class "propertyGroup" or something like that
// has a header, a "description" (optional image?) and a list of properties i guess (action to setup) AND an optional "configure"-button (mode, diffuse, specular but NOT light)

public class LightingScreen : MonoBehaviour {

    [Header("Components")]
    [SerializeField] Image[] borders;

    MeshRenderer targetMR;
    MaterialPropertyBlock mpb;
    // MaterialPropertyBlock diffPropBlock;
    // MaterialPropertyBlock specPropBlock;

    Color objectColor;
    Color objectSpecColor;
    // also sliders/scrollableinputfields for intensity? automatically clamp value (in inputfield) to [0, Infinity]?
    Color ambientLightColor;
    Color mainLightColor;
    Color backLightColor;

    void Start () {
        
    }

    void Update () {
        if(mpb == null){
            mpb = new MaterialPropertyBlock();
        }

        targetMR.SetPropertyBlock(mpb);
    }

    Material CreateDiffuseMaterial (Shader shader) {
        var newMat = new Material(shader);
        newMat.hideFlags = HideFlags.HideAndDontSave;
        newMat.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One); // i don't like the tostring
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
