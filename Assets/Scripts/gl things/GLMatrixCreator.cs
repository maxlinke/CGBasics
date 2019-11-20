using UnityEngine;

public static class GLMatrixCreator {

    public static Matrix4x4 GetTranslationMatrix (Vector3 position) {
        return Matrix4x4.identity;
    }

    public static Matrix4x4 GetViewMatrix (Vector3 eye, Vector3 center, Vector3 up) {
        Vector3 forward = (center - eye).normalized;
        Vector3 right = Vector3.Cross(forward, up).normalized;
        up = Vector3.Cross(right, forward).normalized;
        Matrix4x4 rotation = new Matrix4x4(
            new Vector4(right.x, up.x, -forward.x, 0),
            new Vector4(right.y, up.y, -forward.y, 0),
            new Vector4(right.z, up.z, -forward.z, 0),
            new Vector4(0, 0, 0, 1)
        );
        Matrix4x4 translation = GetTranslationMatrix(eye * -1);
        return translation * rotation;
    }

    public static Matrix4x4 GetProjectionMatrix (float fov, float aspectRatio, float zNear, float zFar) {
        float tan = Mathf.Tan((fov * Mathf.PI) / 360f);
        return new Matrix4x4(
            new Vector4(1f / (aspectRatio * tan), 0, 0, 0),
            new Vector4(0, 1f / tan, 0f, 0),
            new Vector4(0, 0, (-zFar - zNear) / ( zFar - zNear), -1f),
            new Vector4(0, 0, (-2f * zFar * zNear) / (zFar - zNear), 0)
        );
    }
	
}
