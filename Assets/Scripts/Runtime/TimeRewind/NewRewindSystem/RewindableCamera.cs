using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindableCamera : RewindableVariableBase<TransformRecord> {
    private Camera camera;
    private CinemachineFreeLook freeLookCamera;
    private CinemachineVirtualCamera timeRewindCamera;

    public override TransformRecord Value {
        get {
            return value;
        }
        set {
            base.value = value;
        }
    }

    public void RecordTransformIfDifferent() {
        if (CameraTransformHasChanged()) {
            Value.position = camera.transform.position;
            Value.rotation = camera.transform.rotation;
            Value.localScale = camera.transform.localScale;
            IsModified = true;
        }
    }

    private bool CameraTransformHasChanged() {
        return Value.position != camera.transform.position || 
               Value.rotation != camera.transform.rotation || 
               Value.localScale != camera.transform.localScale;
    }

    public RewindableCamera(CinemachineFreeLook freeLookCamera, CinemachineVirtualCamera timeRewindCamera) : base() {
        camera = Camera.main;
        Value = new TransformRecord(camera.transform);
        freeLookCamera.GetComponent<CinemachineTrackCameraTransform>().RewindableCamera = this;
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