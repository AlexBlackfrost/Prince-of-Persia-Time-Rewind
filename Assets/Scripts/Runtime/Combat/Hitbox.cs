using UnityEngine;

public class Hitbox : MonoBehaviour {
    public enum HitboxType { Capsule }

    [field: SerializeField] public HitboxType Type { get; private set; }
    [field:SerializeField] public float Radius { get; private set; } = 0.5f;
    [field:SerializeField] public float Height { get; private set; } = 2f;
    [field:SerializeField] public Vector3 Offset { get; private set; } = Vector3.zero;
    [field:SerializeField] public Vector3 Rotation { get; private set; } = Vector3.zero;

    [SerializeField] private LayerMask layerMask;


    public bool CheckHit(bool sweep) {
        switch (Type) {
            case HitboxType.Capsule:
                //Physics.CapsuleCastAll();
                break;
        }
        return false;
    }

}