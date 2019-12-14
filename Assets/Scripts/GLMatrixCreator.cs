using UnityEngine;

public static class GLMatrixCreator {

    // a Matrix4x4 constructed from Vector4s takes the vectors as COLUMNS, not rows.
    // so the costructors you see here have to be transposed to see the REAL matrix...

    public static Matrix4x4 GetTranslationMatrix (Vector3 position) {
        return new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(position.x, position.y, position.z, 1)
        );
    }

    public static Matrix4x4 GetAxisRotationMatrix (Vector3 axis, float angle){
        float radAngle = Mathf.Deg2Rad * angle;
        Vector3 u = axis.normalized;
        float cos = Mathf.Cos(radAngle);
        float sin = Mathf.Sin(radAngle);
        float r11 = cos + ((u.x * u.x) * (1 - cos));
        float r12 = ((u.x * u.y) * (1 - cos)) - (u.z * sin);
        float r13 = ((u.x * u.z) * (1 - cos)) + (u.y * sin);
        Vector4 r1 = new Vector4(r11, r12, r13, 0);
        float r21 = ((u.y * u.x) * (1 - cos)) + (u.z * sin);
        float r22 = cos + ((u.y * u.y) * (1 - cos));
        float r23 = ((u.y * u.z) * (1 - cos)) - (u.x * sin);
        Vector4 r2 = new Vector4(r21, r22, r23, 0);
        float r31 = ((u.z * u.x) * (1 - cos)) - (u.y * sin);
        float r32 = ((u.z * u.y) * (1 - cos)) + (u.x * sin);
        float r33 = cos + ((u.z * u.z) * (1 - cos));
        Vector4 r3 = new Vector4(r31, r32, r33, 0);
        Vector4 r4 = new Vector4(0, 0, 0, 1);
        return new Matrix4x4(r1, r2, r3, r4);
	}

    public static Matrix4x4 GetXRotationMatrix (float angle){
        float radAngle = Mathf.Deg2Rad * angle;
        float cos = Mathf.Cos(radAngle);
        float sin = Mathf.Sin(radAngle);
        return new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, cos, sin, 0),
            new Vector4(0, -sin, cos, 0),
            new Vector4(0, 0, 0, 1)
        );
	}

	public static Matrix4x4 GetYRotationMatrix (float angle){
        float radAngle = Mathf.Deg2Rad * angle;
        float cos = Mathf.Cos(radAngle);
        float sin = Mathf.Sin(radAngle);
        return new Matrix4x4(
            new Vector4(cos, 0, -sin, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(sin, 0, cos, 0),
            new Vector4(0, 0, 0, 1)
        );
	}

	public static Matrix4x4 GetZRotationMatrix (float angle){
        float radAngle = Mathf.Deg2Rad * angle;
        float cos = Mathf.Cos(radAngle);
        float sin = Mathf.Sin(radAngle);
        return new Matrix4x4(
            new Vector4(cos, sin, 0, 0),
            new Vector4(-sin, cos, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1)
        );
	}

    public static Matrix4x4 GetRotationMatrix (Vector3 eulerAngles){  // formerly called GetZXYRotationMatrix
        float radX = Mathf.Deg2Rad * eulerAngles.x;
        float radY = Mathf.Deg2Rad * eulerAngles.y;
        float radZ = Mathf.Deg2Rad * eulerAngles.z;
        float sx = Mathf.Sin(radX);
        float cx = Mathf.Cos(radX);
        float sy = Mathf.Sin(radY);
        float cy = Mathf.Cos(radY);
        float sz = Mathf.Sin(radZ);
        float cz = Mathf.Cos(radZ);
        return new Matrix4x4(
            new Vector4((cz * cy) + (sx * sy * sz), (sz * cx), (-sy * cz) + (sx * sz * cy), 0),
            new Vector4((-sz * cy) + (cz * sx * sy), (cz * cx), (sz * sy) + (cy * cz * sx), 0),
            new Vector4((cx * sy), -sx, (cx * cy), 0),
            new Vector4(0, 0, 0, 1)
        );
    }

    public static Matrix4x4 GetScaleMatrix (Vector3 scale) {
        return new Matrix4x4(
            new Vector4(scale.x, 0, 0, 0),
            new Vector4(0, scale.y, 0, 0),
            new Vector4(0, 0, scale.z, 0),
            new Vector4(0, 0, 0, 1)
        );
    }

    public static Matrix4x4 GetModelMatrix (Vector3 position, Vector3 eulerAngles, Vector3 scale) {
        return GetTranslationMatrix(position) * GetRotationMatrix(eulerAngles) * GetScaleMatrix(scale);
    }

    public static Matrix4x4 GetLookAtMatrix (Vector3 eye, Vector3 center, Vector3 up) {
        Vector3 forward = (center - eye).normalized;
        return GetViewMatrix(eye, forward, up);        
    }

    public static Matrix4x4 GetViewMatrix (Vector3 pos, Vector3 forward, Vector3 up) {
        Vector3 right = Vector3.Cross(forward, up).normalized;
        up = Vector3.Cross(right, forward).normalized;
        Matrix4x4 rotation = new Matrix4x4(
            new Vector4(-right.x, up.x, forward.x, 0),
            new Vector4(-right.y, up.y, forward.y, 0),
            new Vector4(-right.z, up.z, forward.z, 0),
            new Vector4(0, 0, 0, 1)
        );
        Matrix4x4 translation = GetTranslationMatrix(pos * -1);
        return rotation * translation;
    }

    public static Matrix4x4 GetProjectionMatrix (float fov, float aspectRatio, float zNear, float zFar) {
        float tan = Mathf.Tan((fov * Mathf.PI) / 360f);
        return new Matrix4x4(       
            new Vector4(1f / (aspectRatio * tan), 0, 0, 0),
            new Vector4(0, 1f / tan, 0, 0),
            new Vector4(0, 0, (zFar + zNear) / ( zFar - zNear), 1),
            new Vector4(0, 0, (-2f * zFar * zNear) / (zFar - zNear),  0)
        );
    }

    public static Matrix4x4 GetOrthoProjectionMatrix (float orthoSize, float aspect, float zNear, float zFar) {
        return new Matrix4x4(
            new Vector4(2f / (orthoSize * aspect), 0, 0, 0),
            new Vector4(0, 2f / orthoSize, 0, 0),
            new Vector4(0, 0, 2f / (zFar - zNear), 0),
            new Vector4(0, 0, -(zFar + zNear) / (zFar - zNear), 1)
        );
    }
	
}
