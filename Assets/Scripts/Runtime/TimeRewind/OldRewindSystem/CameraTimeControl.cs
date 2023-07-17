using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTimeControl{
    private Camera camera;
    private CinemachineVirtualCamera timeRewindCamera;
    private CinemachineFreeLook freeLookCamera;
    private TransformTimeControl transformTimeControl;

    public CameraTimeControl(Camera camera, CinemachineVirtualCamera timeRewindCamera, CinemachineFreeLook freeLookCamera) {
        this.camera = camera;
        this.timeRewindCamera = timeRewindCamera;
        this.freeLookCamera = freeLookCamera;
        transformTimeControl = new TransformTimeControl(camera.transform);
    }

    public void OnTimeRewindStart() {
        timeRewindCamera.transform.position = camera.transform.position;
        timeRewindCamera.transform.rotation = camera.transform.rotation;
        timeRewindCamera.gameObject.SetActive(true);
        freeLookCamera.gameObject.SetActive(false);
    }

    public void OnTimeRewindStop() {
        timeRewindCamera.gameObject.SetActive(false);
        freeLookCamera.gameObject.SetActive(true);
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