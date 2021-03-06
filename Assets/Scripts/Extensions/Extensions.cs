﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RNG = UnityEngine.Random;

public static class Extensions {

    public static void SetGOActive (this Component component, bool newActiveState) {
        component.gameObject.SetActive(newActiveState);
    }

    public static bool GOActiveSelf (this Component component) {
        return component.gameObject.activeSelf;
    }

    public static void SetKeywordEnabled (this Material material, string keyword, bool enabledValue) {
        if(enabledValue){
            material.EnableKeyword(keyword);
        }else{
            material.DisableKeyword(keyword);
        }
    }

    public static Coroutine DoNextFrame (this MonoBehaviour runner, System.Action doNextFrameAction) {
        return runner.StartCoroutine(WaitAndDo());

        IEnumerator WaitAndDo () {
            yield return null;
            doNextFrameAction?.Invoke();
        }
    }

    public static Coroutine WaitNumberOfFrames (this MonoBehaviour runner, System.Action doAction, int waitFrames) {
        return runner.StartCoroutine(WaitAndDo());

        IEnumerator WaitAndDo () {
            int i=0;
            while(i<waitFrames){
                yield return null;
                i++;
            }
            doAction?.Invoke();
        }
    }

    public static string ShortenNumberString (this string inputString) {
        string output = inputString;
        if(float.TryParse(inputString, out var parsed)){
            string pString = parsed.ToString();
            if(pString.Contains(".")){
                while(pString[pString.Length-1] == '0'){
                    pString = pString.Substring(0, pString.Length - 1);
                }
                if(pString[pString.Length-1] == '.'){
                    pString = pString.Substring(0, pString.Length - 1);
                }
            }
            output = pString;
        }
        return output;
    }

    public static int IndexOf<T> (this T[] array, T element) {
        for(int i=0; i<array.Length; i++){
            if(array[i].Equals(element)){
                return i;
            }
        }
        return -1;
    }

    public static T FromStringHash<T> (this T[] array, string inputString) {
        int absHash = System.Math.Abs(inputString.GetHashCode());
        return array[absHash % array.Length];
    }

    public static T Random<T> (this T[] array) {
        return array[RNG.Range(0, array.Length)];
    }

    public static T Random<T> (this List<T> list) {
        return list[RNG.Range(0, list.Count)];
    }

    public static T RandomKey<T, U> (this Dictionary<T, U> map) {
        var c = map.Keys.Count;
        var r = RNG.Range(0, c);
        int i = 0;
        foreach(var key in map.Keys){
            if(i==r){
                return key;
            }
            i++;
        }
        throw new System.NotImplementedException("This is a programmer error!");
    }

    public static U RandomValue<T, U> (this Dictionary<T, U> map) {
        var c = map.Values.Count;
        var r = RNG.Range(0, c);
        int i = 0;
        foreach(var val in map.Values){
            if(i==r){
                return val;
            }
            i++;
        }
        throw new System.NotImplementedException("This is a programmer error!");
    }

    public static void ResetLocalScale (this Transform t) {
        t.localScale = Vector3.one;
    }

    public static void SetFadeTransition (this Selectable selectable, float fadeDuration, Color defaultColor, Color hoverColor, Color clickColor, Color disabledColor) {
        selectable.transition = Selectable.Transition.ColorTint;
        var colorBlock = new ColorBlock();
        colorBlock.fadeDuration = fadeDuration;
        colorBlock.colorMultiplier = 1f;
        colorBlock.normalColor = defaultColor;
        colorBlock.highlightedColor = hoverColor;
        colorBlock.pressedColor = clickColor;
        colorBlock.disabledColor = disabledColor;
        selectable.colors = colorBlock;
    }

    public static void SetFadeTransition (this Selectable selectable, Color color) {
        selectable.SetFadeTransition(0f, color, color, color, color);
    }

    public static void SetFadeTransitionDefaultAndDisabled (this Selectable selectable, Color defaultColor, Color disabledColor) {
        selectable.SetFadeTransition(0f, defaultColor, defaultColor, defaultColor, disabledColor);
    }

    public static Color WithHalfAlpha (this Color col) {
        return col.WithOpacity(0.5f);
    }

    public static Color WithOpacity (this Color col, float opacity) {
        opacity = Mathf.Clamp01(opacity);
        return new Color(col.r, col.g, col.b, col.a * opacity);
    }

    public static Color AlphaOver (this Color col, Color otherCol) {
        return new Color(
            Mathf.Lerp(col.r, otherCol.r, otherCol.a),
            Mathf.Lerp(col.g, otherCol.g, otherCol.a),
            Mathf.Lerp(col.b, otherCol.b, otherCol.a),
            Mathf.Clamp01(col.a + otherCol.a * (1f - col.a)));
    }

    public static Color InvertRGBA (this Color col) {
        return new Color(1 - col.r, 1 - col.g, 1 - col.b, 1 - col.a);
    }

    public static Color InvertRGB (this Color col) {
        return new Color(1 - col.r, 1 - col.g, 1 - col.b, col.a);
    }

    public static Color InvertValue (this Color col) {
        Color.RGBToHSV(col, out var h, out var s, out var v);
        var inv = Color.HSVToRGB(h, s, 1 - v);
        return new Color(inv.r, inv.g, inv.b, col.a);
    }

    public static float Luminance (this Color col) {
        return 0.299f * col.r + 0.587f * col.g + 0.114f * col.b;
    }

    public static float UniformLocalScale (this Transform transform) {
        Vector3 vec = transform.localScale;
        if(vec.x == vec.y && vec.y == vec.z){
            return vec.x;
        }else{
            Debug.LogWarning("Scale is not uniform even though it was asked for!");
            return (vec.x + vec.y + vec.z) / 3f;
        }
    }

    public static void LogInfo (this Mesh inputMesh) {
        string debugLog = $"mesh: {inputMesh.name} \n";
        AppendInfo("triangles", inputMesh.triangles);
        AppendInfo("vertices", inputMesh.vertices);
        AppendInfo("normals", inputMesh.normals);
        AppendInfo("uv", inputMesh.uv);
        AppendInfo("uv2", inputMesh.uv2);
        AppendInfo("uv3", inputMesh.uv3);
        AppendInfo("colors", inputMesh.colors);
        AppendInfo("colors32", inputMesh.colors32);
        Debug.Log(debugLog);

        void AppendInfo<T> (string inputArrayName, T[] inputArray){
            debugLog += $"{inputArrayName}: {((inputArray == null) ? "null" : inputArray.Length.ToString())}\n";
        }
    }

    public static Mesh CreateClone (this Mesh inputMesh, bool flatShading, bool normalsAsVertexColors) {
        var output = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();
        if(flatShading){
            for(int i=0; i<inputMesh.triangles.Length; i+=3){
                var v1 = inputMesh.vertices[inputMesh.triangles[i+0]];
                var v2 = inputMesh.vertices[inputMesh.triangles[i+1]];
                var v3 = inputMesh.vertices[inputMesh.triangles[i+2]];
                var n = Vector3.Cross(v2-v1, v3-v2);
                vertices.AddRange(new Vector3[]{v1, v2, v3});
                normals.AddRange(new Vector3[]{n, n, n});
                triangles.AddRange(new int[]{i, i+1, i+2});
            }
        }else{
            for(int i=0; i<inputMesh.triangles.Length; i++){
                triangles.Add(inputMesh.triangles[i]);
            }
            for(int i=0; i<inputMesh.vertices.Length; i++){
                vertices.Add(inputMesh.vertices[i]);
            }
            for(int i=0; i<inputMesh.normals.Length; i++){
                normals.Add(inputMesh.normals[i]);
            }
        }
        if(normalsAsVertexColors){
            for(int i=0; i<inputMesh.normals.Length; i++){
                var n = (inputMesh.normals[i] + Vector3.one) / 2f;
                colors.Add(new Color(n.x, n.y, n.z));
            }
        }else{
            for(int i=0; i<inputMesh.colors.Length; i++){
                colors.Add(inputMesh.colors[i]);
            }
        }
        output.hideFlags = HideFlags.HideAndDontSave;
        output.vertices = vertices.ToArray();
        output.normals = normals.ToArray();
        output.colors = colors.ToArray();
        output.triangles = triangles.ToArray();
        return output;
    }
	
}
