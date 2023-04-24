using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils {
    public static float MapRangeUnclamped(float value, float inMin, float inMax, float outMin, float outMax) {
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }
    
    public static float MapRangeClamped(float value, float inMin, float inMax, float outMin, float outMax) {
        value = Mathf.Clamp(value, inMin, inMax);
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }

    public static int NonNegativeMod(int x, int m) {
        return (x % m + m) % m;
    }

    public static float DistanceXZ(Vector3 position1, Vector3 position2) {
        position1.y = 0;
        position2.y = 0;
        return Vector3.Distance(position1, position2);
    }
}
