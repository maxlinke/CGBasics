using System.Collections.Generic;
using UnityEngine;
using RNG = UnityEngine.Random;

public static class Extensions {

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

    public static void SetToFill (this RectTransform rt) {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.pivot = Vector2.one * 0.5f;
    }

    public static void SetToFillWithMargins (this RectTransform rt, float marginTop, float marginRight, float marginBottom, float marginLeft) {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = new Vector2(-marginRight-marginLeft, -marginTop-marginBottom);
        rt.anchoredPosition = new Vector2(marginLeft-marginRight, marginBottom-marginTop);
        rt.pivot = 0.5f * Vector2.one;
    }

    public static void SetToFillWithMargins (this RectTransform rt, float margin) {
        SetToFillWithMargins(rt, margin, margin, margin, margin);
    }

    ///<summary>Top, right, bottom, left</summary>
    public static void SetToFillWithMargins (this RectTransform rt, Vector4 margins) {
        SetToFillWithMargins(rt, margins.x, margins.y, margins.z, margins.w);
    }
	
}
