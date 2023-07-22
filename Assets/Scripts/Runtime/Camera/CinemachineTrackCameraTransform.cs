using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineTrackCameraTransform : CinemachineExtension {
    public RewindableCamera RewindableCamera { get; set; }
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {
        if(vcam is CinemachineFreeLook && stage == CinemachineCore.Stage.Finalize) {
            RewindableCamera?.RecordTransformIfDifferent();

        }
    }
}