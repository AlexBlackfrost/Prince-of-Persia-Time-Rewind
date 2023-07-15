using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineRewindableCamera : CinemachineExtension {

    [Tooltip("When to record the camera transform")]
    public CinemachineVirtualCamera timeRewindCamera;
    [SerializeField] private CinemachineCore.Stage m_ApplyAfter = CinemachineCore.Stage.Finalize;

    private RewindableCamera rewindableCamera;

    protected override void Awake() {
        base.Awake();
        CinemachineFreeLook freeLookCamera = GetComponent<CinemachineFreeLook>();
        rewindableCamera = new RewindableCamera(freeLookCamera, timeRewindCamera);
    }
    /// <summary>
    /// Applies the specified offset to the camera state
    /// </summary>
    /// <param name="vcam">The virtual camera being processed</param>
    /// <param name="stage">The current pipeline stage</param>
    /// <param name="state">The current virtual camera state</param>
    /// <param name="deltaTime">The current applicable deltaTime</param>
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {

        Debug.Log("Camera extension rotation: " + vcam.transform.rotation);
        rewindableCamera.RecordTransform(vcam.transform);
    }

}
