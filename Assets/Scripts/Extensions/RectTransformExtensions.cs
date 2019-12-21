using UnityEngine;

public static class RectTransformExtensions {

    public static void SetToPoint (this RectTransform rt) {
        rt.anchorMin = 0.5f * Vector2.one;
        rt.anchorMax = rt.anchorMin;
        rt.pivot = 0.5f * Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
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

    // TODO implement this everywhere
    public static void SetToFillWithMarginsFixed (this RectTransform rt, float marginTop, float marginRight, float marginBottom, float marginLeft) {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = new Vector2(-marginRight-marginLeft, -marginTop-marginBottom);
        rt.anchoredPosition = new Vector2((marginLeft-marginRight) / 2f, (marginBottom-marginTop) / 2);
        rt.pivot = 0.5f * Vector2.one;
    }

    public static void SetToFillWithMargins (this RectTransform rt, float margin) {
        SetToFillWithMargins(rt, margin, margin, margin, margin);
    }

    ///<summary>Top, right, bottom, left</summary>
    public static void SetToFillWithMargins (this RectTransform rt, Vector4 margins) {
        SetToFillWithMargins(rt, margins.x, margins.y, margins.z, margins.w);
    }

    public static void SetSizeDelta (this RectTransform rt, float x, float y) {
        rt.sizeDelta = new Vector2(x, y);
    }

    public static void SetSizeDeltaX (this RectTransform rt, float x) {
        rt.SetSizeDelta(x, rt.sizeDelta.y);
    }

    public static void SetSizeDeltaY (this RectTransform rt, float y) {
        rt.SetSizeDelta(rt.sizeDelta.x, y);
    }

    public static void SetAnchor (this RectTransform rt, Vector2 anchor) {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
    }

    public static void SetAnchor (this RectTransform rt, float x, float y) {
        rt.SetAnchor(new Vector2(x, y));
    }

    public static Vector2 AverageAnchor (this RectTransform rt) {
        return 0.5f * rt.anchorMin + 0.5f * rt.anchorMax;
    }

    public static void SetPivot (this RectTransform rt, float x, float y) {
        rt.pivot = new Vector2(x, y);
    }

    public static void SetAnchorAndPivot (this RectTransform rt, float x, float y) {
        rt.SetAnchorAndPivot(new Vector2(x, y));
    }

    public static void SetAnchorAndPivot (this RectTransform rt, Vector2 anchorAndPivot) {
        rt.SetAnchor(anchorAndPivot);
        rt.pivot = anchorAndPivot;
    }

    public static void SetAnchoredPosition (this RectTransform rt, float x, float y) {
        rt.anchoredPosition = new Vector2(x, y);
    }

    public static void SetAnchoredPositionX (this RectTransform rt, float x) {
        rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);
    }

    public static void SetAnchoredPositionY (this RectTransform rt, float y) {
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
    }

    public static void MatchOther (this RectTransform rt, RectTransform otherRT, bool includingParent = true) {
        if(includingParent){
            rt.SetParent(otherRT.parent, false);
        }
        rt.localScale = otherRT.localScale;
        rt.localRotation = otherRT.localRotation;
        rt.anchorMin = otherRT.anchorMin;
        rt.anchorMax = otherRT.anchorMax;
        rt.pivot = otherRT.pivot;
        rt.sizeDelta = otherRT.sizeDelta;
        rt.anchoredPosition = otherRT.anchoredPosition;
    }
	
}
