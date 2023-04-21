using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Hitbox))]
public class HitboxEditor : Editor{

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    static void OnDrawGizmos(Hitbox hitbox, GizmoType gizmoType) {
        switch (hitbox.Type) {

            case Hitbox.HitboxType.Capsule:

                GizmosExtensions.DrawWireCapsule(hitbox.transform.position + hitbox.transform.TransformDirection(hitbox.Offset), 
                                                 hitbox.transform.rotation * Quaternion.Euler(hitbox.Rotation), 
                                                 hitbox.Radius, hitbox.Height, Color.red);
                break;
        }
    }
}