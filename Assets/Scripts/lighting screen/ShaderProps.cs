﻿using System.Collections.Generic;
using UnityEngine;

public static class ShaderProps {

    public static ColorProp diffuseColor { get; private set; }
    public static ColorProp specularColor { get; private set; }

    public static FloatProp roughness { get; private set; }
    public static FloatProp minnaertExp { get; private set; }

    public static FloatProp specIntensity { get; private set; }
    public static FloatProp specHardness { get; private set; }
    public static FloatProp specHardnessX { get; private set; }
    public static FloatProp specHardnessY { get; private set; }

    // TODO should the colors be in the material settings (so lambert has at least ONE thing) or should the two colors be part of the object?
    private static List<Prop> diffProps;
    private static List<Prop> specProps;

    static ShaderProps () {
        diffuseColor = new ColorProp("_Color", "Diffuse Color", Color.white);
        specularColor = new ColorProp("_SpecularColor", "Specular Color", Color.white);

        roughness = new FloatProp("_Roughness", "Roughness", 0.5f, 0f, 1f);
        minnaertExp = new FloatProp("_MinnaertExp", "Minneart Exponent", 1.5f, 0f, 4f);

        float specMin = 0f;
        float specMax = 128f;
        float specDefault = 64f;
        specIntensity = new FloatProp("_SpecularIntensity", "Intensity", 1f, 0f, 1f);
        specHardness = new FloatProp("_SpecularHardness", "Hardness", specDefault, specMin, specMax);
        specHardnessX = new FloatProp("_SpecularHardnessX", "Hardness (X)", specDefault, specMin, specMax);
        specHardnessY = new FloatProp("_SpecularHardnessY", "Hardness (Y)", specDefault, specMin, specMax);

        diffProps = new List<Prop>();
        diffProps.Add(diffuseColor);
        diffProps.Add(roughness);
        diffProps.Add(minnaertExp);
        
        specProps = new List<Prop>();
        specProps.Add(specularColor);
        specProps.Add(specIntensity);
        specProps.Add(specHardness);
        specProps.Add(specHardnessX);
        specProps.Add(specHardnessY);
    }

    public static IEnumerator<Prop> DiffuseProps () {
        foreach(var prop in diffProps){
            yield return prop;
        }
    }

    public static IEnumerator<Prop> SpecularProps () {
        foreach(var prop in specProps){
            yield return prop;
        }
    }

    public abstract class Prop {
        public readonly string propName;
        public readonly int propID;
        public readonly string niceName;
        public Prop (string propName, string niceName) {
            this.propName = propName;
            this.propID = Shader.PropertyToID(propName);
            this.niceName = niceName;
        }
    }

    public class FloatProp : Prop {
        public readonly float defaultValue;
        public readonly float minValue;
        public readonly float maxValue;
        public FloatProp (string propName, string niceName, float defaultVal, float minVal, float maxVal) : base(propName, niceName) {
            this.defaultValue = defaultVal;
            this.minValue = minVal;
            this.maxValue = maxVal;
        }
    }

    public class ColorProp : Prop {
        public readonly Color defaultValue;
        public ColorProp (string propName, string niceName, Color defaultVal) : base(propName, niceName) {
            this.defaultValue = defaultVal;
        }
    }
	
}