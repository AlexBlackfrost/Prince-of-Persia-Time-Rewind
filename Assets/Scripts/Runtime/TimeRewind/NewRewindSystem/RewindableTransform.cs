using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public RewindableTransform(Transform transform) : base(transform) { }

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