using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindableCamera : RewindableVariableBase<TransformRecord> {
    private Camera camera;
    private CinemachineFreeLook freeLookCamera;
    private CinemachineVirtualCamera timeRewindCamera;
    public override bool IsModified {
        get {
            bool differentFromPreviousFrame = camera.transform.position != Value.position || 
                                              camera.transform.rotation != Value.rotation || 
                                              camera.transform.localScale != Value.localScale;
            if (differentFromPreviousFrame) {
                RecordTransform(camera.transform);
            }
            return differentFromPreviousFrame; 
        }
        set {
            base.IsModified = value;
        }
    }

    public void RecordTransform(Transform transform) {
        Value = new TransformRecord(transform.position, transform.rotation, transform.localScale);
        IsModified = true;
    }

    public RewindableCamera(CinemachineFreeLook freeLookCamera, CinemachineVirtualCamera timeRewindCamera) : base() {
        camera = Camera.main;
        RecordTransform(camera.transform);
        this.freeLookCamera = freeLookCamera;
        this.timeRewindCamera = timeRewindCamera;
    }

    public override void OnRewindStart() {
        timeRewindCamera.transform.position = camera.transform.position;
        timeRewindCamera.transform.rotation = camera.transform.rotation;
        timeRewindCamera.gameObject.SetActive(true);
        freeLookCamera.gameObject.SetActive(false);
    }

    public override void OnRewindStop(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        timeRewindCamera.gameObject.SetActive(false);
        freeLookCamera.gameObject.SetActive(true);
        Rewind(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
    }

    public override object Record() {
        return Value;
    }

    public override void Rewind(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        TransformRecord previousTransformRecord = (TransformRecord)previousRecord;
        TransformRecord nextTransformRecord = (TransformRecord)nextRecord;

        float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

        timeRewindCamera.transform.position = Vector3.Lerp(previousTransformRecord.position, nextTransformRecord.position, lerpAlpha);
        timeRewindCamera.transform.rotation = Quaternion.Slerp(previousTransformRecord.rotation, nextTransformRecord.rotation, lerpAlpha);
        timeRewindCamera.transform.localScale = Vector3.Lerp(previousTransformRecord.localScale, nextTransformRecord.localScale, lerpAlpha);
        
        
    }


}