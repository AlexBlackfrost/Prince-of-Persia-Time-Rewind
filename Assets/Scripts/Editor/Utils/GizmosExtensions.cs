using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GizmosExtensions {
    // Credit: https://forum.unity.com/threads/drawing-capsule-gizmo.354634/
    public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, float _radius, float _height, Color _color = default(Color)) {
        if (_color != default(Color))
            Handles.color = _color;
        Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);
        using (new Handles.DrawingScope(angleMatrix)) {
            var pointOffset = (_height - (_radius * 2)) / 2;

            //draw sideways
            Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius);
            Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
            Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
            Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius);
            //draw frontways
            Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius);
            Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
            Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
            Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius);
            //draw center
            Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius);
            Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius);

        }
    }

    public static void DrawLine(Vector3 start, Vector3 end, float thickness, Color color = default(Color)) {
        Camera camera = Camera.current;
        if (camera != null && camera.clearFlags != CameraClearFlags.Depth && camera.clearFlags != CameraClearFlags.Nothing) {

            // Only draw the line when it is the closest thing to the camera
            // (Remove the Z-test code and other objects will not occlude the line.)
            var prevZTest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            Handles.color = color;
            float cameraDistance = HandleUtility.GetHandleSize((start + end) / 2.0f);
            Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, thickness*(1/cameraDistance) , new Vector3[] { start, end }   );

            Handles.zTest = prevZTest;
        }
    }

 
    public static void DrawArrow(Vector3 start, Vector3 end) {
        DrawArrow(start, end, 1);
    }

    public static void DrawArrow(Vector3 start, Vector3 end, float thickness) {
        DrawArrow(start, end, thickness, 5);
    }

    public static void DrawArrow(Vector3 start, Vector3 end, float thickness, float arrowSize) {
        DrawArrow(start, end, thickness, arrowSize, Color.red);
    }

    public static void DrawArrow(Vector3 start, Vector3 end, float thickness, float arrowSize, Color color) {
        float arrowThicknessScale = 10;
        // Draw arrow body
        DrawLine(start, end, thickness*arrowThicknessScale, color);

        // Draw arrowhead
        Vector3 arrowUpVector = Vector3.up;// (Camera.current.transform.position - end).normalized;

        float arrowAngle = 40;
        Vector3 leftArrowHeadDirection = Quaternion.AngleAxis(arrowAngle, arrowUpVector) * (start-end).normalized ;
        Vector3 rightArrowHeadDirection = Quaternion.AngleAxis(-arrowAngle, arrowUpVector) * (start-end).normalized;

        float arrowSizeScale = 0.5f;
        Vector3 leftArrowHeadEnd = end + leftArrowHeadDirection * arrowSize*arrowSizeScale;
        Vector3 rightArrowHeadEnd = end + rightArrowHeadDirection * arrowSize*arrowSizeScale;

        DrawLine(end, leftArrowHeadEnd, thickness*arrowThicknessScale, color);
        DrawLine(end, rightArrowHeadEnd, thickness*arrowThicknessScale, color);
    }
}