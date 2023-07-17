using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct TransformRecord {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;

    public TransformRecord(Vector3 position, Quaternion rotation, Vector3 localScale) {
        this.position = position;
        this.rotation = rotation;
        this.localScale = localScale;
    }

    public override string ToString() {
        return "Position: " + position + "\nRotation: " + rotation + "\nScale: " + localScale + "\n";
    }
}
public class RewindableTransform : RewindableVariableBase<Transform> {
    public Vector3 position {
        get {
            return Value.position;
        }
        set {
            Value.position = value;
            IsModified = true;
        }
    }

    public Quaternion rotation {
        get {
            return Value.rotation;
        }
        set {
            Value.rotation = value;
            IsModified = true;
        }
    }

    public Vector3 localScale {
        get {
            return Value.localScale;
        }
        set {
            Value.localScale = value;
            IsModified = true;
        }
    }

    public override bool IsModified {
        get {
            if (!isModified && animator.applyRootMotion) {
                isModified = true; 
            }
            return isModified;
        }
        set {
            base.IsModified = value;
        }
    }

    private Animator animator;
    
    public RewindableTransform(Transform transform, Animator animator) : base(transform) {
        this.animator = animator;
    }

    public override void OnRewindStart() { }

    public override void OnRewindStop(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        Rewind(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
    }

    public override object Record() {
        return new TransformRecord(Value.position, Value.rotation, Value.localScale);
    }

    public override void Rewind(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        TransformRecord previousTransformRecord = (TransformRecord)previousRecord;
        TransformRecord nextTransformRecord = (TransformRecord)nextRecord;

        float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

        Value.position = Vector3.Lerp(previousTransformRecord.position, nextTransformRecord.position, lerpAlpha);
        Value.rotation = Quaternion.Slerp(previousTransformRecord.rotation, nextTransformRecord.rotation, lerpAlpha);
        Value.localScale = Vector3.Lerp(previousTransformRecord.localScale, nextTransformRecord.localScale, lerpAlpha);
    }


}