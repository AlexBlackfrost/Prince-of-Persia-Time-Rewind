using System.Collections.Generic;
using UnityEngine;

public struct HitData {
    public IHittable hittableObject;
    public Vector3 hitPosition;
    public HitData(IHittable hittableObject, Vector3 hitPosition) {
        this.hittableObject = hittableObject;
        this.hitPosition = hitPosition;
    }
}

public class Hitbox : MonoBehaviour {

    public enum HitboxType { Capsule }

    #region Capsule
    [field: SerializeField] public HitboxType Type { get; private set; }
    [field:SerializeField] public float Radius { get; private set; } = 0.5f;
    [field:SerializeField] public float Height { get; private set; } = 2f;
    [field:SerializeField] public Vector3 Offset { get; private set; } = Vector3.zero;
    [field:SerializeField] public Vector3 Rotation { get; private set; } = Vector3.zero;

    [SerializeField] private LayerMask layerMask;

    private Vector3 point1;
    private Vector3 point2;
    private HashSet<IHittable> emptySet = new HashSet<IHittable>();
    #endregion

    public HitData[] CheckHit() {
        return CheckHit(emptySet);
    }

    public HitData[] CheckHit(HashSet<IHittable> ignoreObjects) {
        HitData[] hitData = null;

        switch (Type) {

            case HitboxType.Capsule:
                hitData = CheckHitCapsule(ignoreObjects);
                break;
        }

        return hitData;
    }

    private HitData[] CheckHitCapsule(HashSet<IHittable> ignoreObjects) {
        point1 = transform.position - transform.rotation * Quaternion.Euler(Rotation) * Vector3.up * (Height / 2.0f - Radius) + transform.TransformDirection(Offset);
        point2 = transform.position + transform.TransformDirection(Offset) + transform.rotation * Quaternion.Euler(Rotation) * Vector3.up * (Height / 2.0f - Radius);
        Vector3 direction = (point2 - point1).normalized;
        RaycastHit[] raycastHits = Physics.CapsuleCastAll(point1, point2, Radius, direction, 0.01f, layerMask, QueryTriggerInteraction.Collide);
        return GetHitData(raycastHits, ignoreObjects);
    }

    private HitData[] GetHitData(RaycastHit[] raycastHits, HashSet<IHittable> ignoreObjects) {
        List<HitData> hitsData = null;

        foreach(RaycastHit raycastHit in raycastHits) {
            IHittable hittable = raycastHit.collider.gameObject.GetComponent<IHittable>();

            if (hittable != null && !ignoreObjects.Contains(hittable)) {
                Vector3 hitLocation;

                /* "For colliders that overlap the capsule at the start of the sweep, RaycastHit.normal is set opposite to the direction of the sweep, 
                 * RaycastHit.distance is set to zero, and the zero vector gets returned in RaycastHit.point" */
                if(raycastHit.point == Vector3.zero && raycastHit.distance == 0) {
                    hitLocation = raycastHit.collider.ClosestPoint(point1);
                } else {
                    hitLocation = raycastHit.point;
                }

                HitData hitData = new HitData(hittable, hitLocation);
                if (hitsData == null) {
                    hitsData = new List<HitData>();
                }
                hitsData.Add(hitData);
            }
        }

        return hitsData?.ToArray();
    }

    private void OnDrawGizmos() {
        /*Gizmos.DrawSphere(point1, Radius - 0.05f);
        Gizmos.DrawSphere(point2, Radius - 0.05f);

        HitData[] hitsData = CheckHitCapsule();
        if (hitsData != null) {
            foreach(HitData hitData in hitsData) {
                Gizmos.DrawSphere(hitData.hitPosition, 0.2f);
                Debug.Log(hitData.hitPosition);
            }
        }*/
    }

}