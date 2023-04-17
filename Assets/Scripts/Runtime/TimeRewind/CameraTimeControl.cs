using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTimeControl{
    private Camera camera;
    private CinemachineVirtualCamera timeRewindCamera;
    private TransformTimeControl transformTimeControl;

    public CameraTimeControl(Camera camera, CinemachineVirtualCamera timeRewindCamera) {
        this.camera = camera;
        this.timeRewindCamera = timeRewindCamera;
        transformTimeControl = new TransformTimeControl(camera.transform);
    }

    public void OnTimeRewindStart() {

    }

    public void OnTimeRewindStop() {

    }

    public void RestoreCameraRecord(CameraRecord previousRecord, CameraRecord nextRecord, float previousRecordDeltaTime,
                                        float elapsedTimeSinceLastRecord) {

        TransformRecord previousTransformRecord = previousRecord.cameraTransform;
        TransformRecord nextTransformRecord = nextRecord.cameraTransform;
        float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

        transformTimeControl.RestoreTransformRecord(timeRewindCamera.transform, previousTransformRecord, nextTransformRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
    }

    public CameraRecord RecordCameraData() {
        return new CameraRecord(transformTimeControl.RecordTransformData());
    }
}