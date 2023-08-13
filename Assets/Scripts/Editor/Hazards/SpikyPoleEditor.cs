using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpikyPole))]
public class SpikyPoleEditor : Editor {

    [DrawGizmo(GizmoType.Selected)]
    public static void OnDrawGizmos(SpikyPole spikyPole, GizmoType gizmoType) {
        DrawSpikyPoleAtTargetLocation(spikyPole);
        DrawSpikyPolePath(spikyPole);
    }

    private static void DrawSpikyPoleAtTargetLocation(SpikyPole spikyPole) {
        if (Application.isPlaying) {
            Gizmos.DrawCube(spikyPole.InitialPosition + spikyPole.transform.forward * spikyPole.Displacement, Vector3.one);
        } else {
            Gizmos.DrawCube(spikyPole.transform.position + spikyPole.transform.forward * spikyPole.Displacement, Vector3.one);
        }
    }

    private static void DrawSpikyPolePath(SpikyPole spikyPole) {
        Gizmos.color = Color.red;
        Vector3 fromPosition = Vector3.zero;
        if (Application.isPlaying) {
            fromPosition = spikyPole.InitialPosition;
        } else {
            fromPosition = spikyPole.transform.position;
        }
        Vector3 toPosition = fromPosition + spikyPole.transform.forward * spikyPole.Displacement;
        GizmosExtensions.DrawArrow(fromPosition, toPosition, 1, 1, Color.red);
    }

}