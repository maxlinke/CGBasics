using UnityEngine;

public static class GLMatrixCreator {

    public static Matrix4x4 GetTranslationMatrix (Vector3 position) {
        return new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(position.x, position.y, position.z, 1)
        );
    }

    // there for "legacy" reasons i guess...
    public static Matrix4x4 GetLookAtMatrix (Vector3 eye, Vector3 center, Vector3 up) {
        Vector3 forward = (center - eye).normalized;
        return GetViewMatrix(eye, forward, up);        
    }

    public static Matrix4x4 GetViewMatrix (Vector3 pos, Vector3 forward, Vector3 up) {
        Vector3 right = Vector3.Cross(forward, up).normalized;
        up = Vector3.Cross(right, forward).normalized;

        // original from lwjgl
        // Matrix4x4 rotation = new Matrix4x4(
        //     new Vector4(right.x, up.x, -forward.x, 0),
        //     new Vector4(right.y, up.y, -forward.y, 0),
        //     new Vector4(right.z, up.z, -forward.z, 0),
        //     new Vector4(0, 0, 0, 1)
        // );

        // left, up, forward instead of right, up, back...
        Matrix4x4 rotation = new Matrix4x4(
            new Vector4(-right.x, up.x, forward.x, 0),
            new Vector4(-right.y, up.y, forward.y, 0),
            new Vector4(-right.z, up.z, forward.z, 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 translation = GetTranslationMatrix(pos * -1);

        // return translation * rotation;
        return rotation * translation;  // whoops
    }

    public static Matrix4x4 GetProjectionMatrix (float fov, float aspectRatio, float zNear, float zFar) {
        float tan = Mathf.Tan((fov * Mathf.PI) / 360f);
        
        // original from lwjgl
        // return new Matrix4x4(
        //     new Vector4(1f / (aspectRatio * tan),   0,          0,                                      0),
        //     new Vector4(0,                          1f / tan,   0,                                      0),
        //     new Vector4(0,                          0,          (-zFar - zNear) / ( zFar - zNear),      -1),
        //     new Vector4(0,                          0,          (-2f * zFar * zNear) / (zFar - zNear),  0)
        // );
        
        // "premultiplied" with negative -1 z-scale matrix
        return new Matrix4x4(       
            new Vector4(1f / (aspectRatio * tan),   0,          0,                                      0),
            new Vector4(0,                          1f / tan,   0,                                      0),
            new Vector4(0,                          0,          (zFar + zNear) / ( zFar - zNear),     1),
            new Vector4(0,                          0,          (-2f * zFar * zNear) / (zFar - zNear),  0)
        );
    }
	
}
