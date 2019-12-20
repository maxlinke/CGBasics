using UnityEngine;

public static class UIUtils {

    public static Vector2 GetFullscreenCursorBoxPivot (Vector2 dimensions) {
        float leftSpace = Input.mousePosition.x;
        float bottomSpace = Input.mousePosition.y;
        float rightSpace = Screen.width - leftSpace;
        float topSpace = Screen.height - bottomSpace;

        float width = dimensions.x;
        float height = dimensions.y;

        bool toRight = (rightSpace >= width || leftSpace < width);
        bool toBottom = (bottomSpace >= height ? true : ((topSpace >= height) ? false : (bottomSpace >= topSpace)));
        float pivotX = toRight ? 0 : 1;
        float pivotY = toBottom ? 1 : 0;

        return new Vector2(pivotX, pivotY);
    }
	
}
