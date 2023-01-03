using UnityEngine;

public static class VectorExtensions{

    public static Vector2 XX(this Vector3 vector3) {
        return new Vector2(vector3.x, vector3.x);
    }

    public static Vector2 XY(this Vector3 vector3) {
        return new Vector2(vector3.x, vector3.y);
    }

    public static Vector2 XZ(this Vector3 vector3) {
        return new Vector2(vector3.x, vector3.z);
    }

    public static Vector2 YX(this Vector3 vector3) {
        return new Vector2(vector3.y, vector3.x);
    }

    public static Vector2 YY(this Vector3 vector3) {
        return new Vector2(vector3.y, vector3.y);
    }

    public static Vector2 YZ(this Vector3 vector3) {
        return new Vector2(vector3.y, vector3.z);
    }

    public static Vector2 ZX(this Vector3 vector3) {
        return new Vector2(vector3.z, vector3.x);
    }

    public static Vector2 ZY(this Vector3 vector3) {
        return new Vector2(vector3.z, vector3.y);
    }

    public static Vector2 ZZ(this Vector3 vector3) {
        return new Vector2(vector3.z, vector3.z);
    }
}

