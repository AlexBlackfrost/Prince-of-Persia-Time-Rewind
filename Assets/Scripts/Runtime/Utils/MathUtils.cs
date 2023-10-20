using System;
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

    public static float GetHdrColorIntensity(Color linearColorHdr) {
        const byte MAX_BYTE_FOR_OVEREXPOSED_COLOR = 191;
        float maxColorComponent = linearColorHdr.maxColorComponent;
        float intensity = 0;
        // replicate Photoshops's decomposition behaviour
        if (! (maxColorComponent == 0f || maxColorComponent <= 1f && maxColorComponent >= 1 / 255f) ) {
            // calibrate exposure to the max float color component
            float scaleFactor = MAX_BYTE_FOR_OVEREXPOSED_COLOR / maxColorComponent;
            intensity = Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);
        }
        return intensity;
    }

    public static void DecomposeHdrColor(Color linearColorHdr, out Color32 baseLinearColor, out float exposure) {
        const byte MAX_BYTE_FOR_OVEREXPOSED_COLOR = 191;
        const byte MAX_BYTE_LINEAR_COLOR = 255;
        baseLinearColor = linearColorHdr;
        float maxColorComponent = linearColorHdr.maxColorComponent;
        // replicate Photoshops's decomposition behaviour
        if (maxColorComponent == 0f || maxColorComponent <= 1f && maxColorComponent >= 1 / 255f) {
            exposure = 0f;

            baseLinearColor.r = (byte)Mathf.RoundToInt(linearColorHdr.r * 255f);
            baseLinearColor.g = (byte)Mathf.RoundToInt(linearColorHdr.g * 255f);
            baseLinearColor.b = (byte)Mathf.RoundToInt(linearColorHdr.b * 255f);
        } else {
            // calibrate exposure to the max float color component
            float scaleFactor = MAX_BYTE_FOR_OVEREXPOSED_COLOR / maxColorComponent;
            exposure = Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);

            scaleFactor = MAX_BYTE_LINEAR_COLOR / maxColorComponent;
            // maintain maximal integrity of byte values to prevent off-by-one errors when scaling up a color one component at a time
            baseLinearColor.r = Math.Min(MAX_BYTE_FOR_OVEREXPOSED_COLOR, (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.r));
            baseLinearColor.g = Math.Min(MAX_BYTE_FOR_OVEREXPOSED_COLOR, (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.g));
            baseLinearColor.b = Math.Min(MAX_BYTE_FOR_OVEREXPOSED_COLOR, (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.b));
        }
    }
}
