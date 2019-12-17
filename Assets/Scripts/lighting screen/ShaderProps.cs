using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShaderProps {

    public enum ID {
        _Color,
        _Roughness,
        _MinnaertExp,

        _SpecularColor,
        _SpecularIntensity,
        _SpecularHardness,
        _SpecularHardnessX,
        _SpecularHardnessY,

        _SrcBlend,
        _DstBlend,
        _ZWrite,
        _ZTest,
        _Cull
    }

    // and hover tooltips?
    public static string GetNameForID (ID id) {
        switch(id){
            case ID._Color: 
                return "Diffuse Color";

            default: throw new System.ArgumentException($"Unknown {nameof(ID)} \"{id}\"!");
        }
    }

    public static (float min, float max) GetRange (ID id) {
        (float min, float max) hardnessRange = (min: 0, max: 128);
        switch(id){
            case ID._Roughness: 
                return (min: 0, max: 1);
            case ID._MinnaertExp:
                return (min: 0, max: 4);
            case ID._SpecularIntensity:
                return (min: 0, max: 1);
            case ID._SpecularHardness:
                return hardnessRange;
            case ID._SpecularHardnessX:
                return hardnessRange;
            case ID._SpecularHardnessY:
                return hardnessRange;
            default: throw new System.ArgumentException($"{nameof(ID)} \"{id}\" doesn't have a range!");
        }
    }
	
}
