using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTimeControl {
    private Transform transform;
    public TransformTimeControl(Transform transform) {
        this.transform = transform;
    }

    public void OnTimeRewindStart() {

    }

    public void OnTimeRewindStop(TransformRecord previousRecord, TransformRecord nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        RestoreTransformRecord(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
    }

    public void RestoreTransformRecord(TransformRecord previousRecord, TransformRecord nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

        transform.position = Vector3.Lerp(previousRecord.position, nextRecord.position, lerpAlpha);
        transform.rotation = Quaternion.Slerp(previousRecord.rotation, nextRecord.rotation, lerpAlpha);
        transform.localScale = Vector3.Lerp(previousRecord.localScale, nextRecord.localScale, lerpAlpha);
    }

    public void RestoreTransformRecord(Transform transform, TransformRecord previousRecord, TransformRecord nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

        transform.position = Vector3.Lerp(previousRecord.position, nextRecord.position, lerpAlpha);
        transform.rotation = Quaternion.Slerp(previousRecord.rotation, nextRecord.rotation, lerpAlpha);
        transform.localScale = Vector3.Lerp(previousRecord.localScale, nextRecord.localScale, lerpAlpha);
    }

    public TransformRecord RecordTransformData() {
        return new TransformRecord(transform.position,
                                   transform.rotation,
                                   transform.localScale);
    }
}