using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TransformRecord {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;

    public TransformRecord(Vector3 position, Quaternion rotation, Vector3 localScale) {
        this.position = position;
        this.rotation = rotation;
        this.localScale = localScale;
    }

    public TransformRecord(Transform transform) {
        this.position = transform.position;
        this.rotation = transform.rotation;
        this.localScale = transform.localScale;
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


    
    public RewindableTransform(Transform transform, bool onlyExecuteOnRewindStop = false) : 
        base(transform, onlyExecuteOnRewindStop:onlyExecuteOnRewindStop) {

    }

    public override void OnRewindStart() { }

    public override void OnRewindStop(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        Rewind(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
    }

    public override object Record() {
        TransformRecord recordedTransform = null;
        if(Value != null) {
            recordedTransform = new TransformRecord(Value.position, Value.rotation, Value.localScale);
        }
        return recordedTransform;
    }

    public override void Rewind(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        TransformRecord previousTransformRecord = (TransformRecord)previousRecord;
        TransformRecord nextTransformRecord = (TransformRecord)nextRecord;

        float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

        if(previousTransformRecord!= null && nextTransformRecord != null) {
            value.position = Vector3.Lerp(previousTransformRecord.position, nextTransformRecord.position, lerpAlpha);
            value.rotation = Quaternion.Slerp(previousTransformRecord.rotation, nextTransformRecord.rotation, lerpAlpha);
            value.localScale = Vector3.Lerp(previousTransformRecord.localScale, nextTransformRecord.localScale, lerpAlpha);
        } else {
            value = null;
        }
        
    }

}