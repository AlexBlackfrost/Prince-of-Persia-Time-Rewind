using Cinemachine;
using HFSM;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeRewindState : State {
	[Serializable]
	public class TimeRewindSettings {
		public TimeRewinder TimeRewinder { get; set; }
		public Animator Animator { get; set; }
		public Transform Transform { get; set; }
		public StateMachine TimeForwardStateMachine { get; set; }
		public CinemachineFreeLook FreeLookCamera { get; set; }
		public CinemachineVirtualCamera timeRewindCamera;
		public Camera Camera { get; set; }
	}

	private TimeRewindSettings settings;
	private float elapsedTimeSinceLastRecord;
	private PlayerRecord previousRecord, nextRecord;
	private float rewindSpeed = 0.1f; 

	public TimeRewindState(TimeRewindSettings timeRewindSettings) : base() {
		this.settings = timeRewindSettings;
	}
    protected override void OnEnter() {
		previousRecord = RecordUtils.RecordPlayerData(settings.Transform,
													  settings.Camera,
													  settings.TimeForwardStateMachine,
													  settings.Animator,
													  null);
		previousRecord.deltaTime = 0.0f;
		
		settings.Animator.StopRecording();
		settings.Animator.StartPlayback();
		settings.Animator.playbackTime = settings.Animator.recorderStopTime;

		settings.timeRewindCamera.transform.position = settings.Camera.transform.position;
		settings.timeRewindCamera.transform.rotation = settings.Camera.transform.rotation;
		settings.timeRewindCamera.gameObject.SetActive(true);
		settings.FreeLookCamera.gameObject.SetActive(false);
	}

	protected override void OnUpdate() {
		if (settings.TimeRewinder.records.Count != 0) {
			nextRecord = settings.TimeRewinder.records.Peek();

			while (elapsedTimeSinceLastRecord > nextRecord.deltaTime && settings.TimeRewinder.records.Count != 0) {
				elapsedTimeSinceLastRecord -= nextRecord.deltaTime;
				previousRecord = nextRecord;
				nextRecord = settings.TimeRewinder.records.Pop();
			}

			RestorePlayerRecord(nextRecord);
			elapsedTimeSinceLastRecord += Time.deltaTime * rewindSpeed;
		}
	}

    protected override void OnExit() {
		settings.Animator.StopPlayback();

		settings.timeRewindCamera.gameObject.SetActive(false);
		settings.FreeLookCamera.gameObject.SetActive(true);

		RestoreStateMachine(nextRecord.stateMachine);

	}

	private void RestorePlayerRecord(PlayerRecord record) {
		RestoreCameraRecord(previousRecord.cameraRecord, nextRecord.cameraRecord, nextRecord.deltaTime);
		RestoreTransformRecord(settings.Transform, previousRecord.playerTransform, record.playerTransform, record.deltaTime);
		RestoreAnimationRecord(settings.Animator, previousRecord.animationRecord, nextRecord.animationRecord, record.deltaTime);
		Debug.Log("Rewinding... " + nextRecord.stateMachine.GetCurrentStateName());
	}

	private void RestoreTransformRecord(Transform transform, TransformRecord previousTransformRecord, 
										TransformRecord nextTransformRecord, float nextRecordDeltaTime) {

		float lerpAlpha = elapsedTimeSinceLastRecord / nextRecordDeltaTime;

		transform.position = Vector3.Lerp(previousTransformRecord.position, nextTransformRecord.position, lerpAlpha);
		transform.rotation = Quaternion.Slerp(previousTransformRecord.rotation, nextTransformRecord.rotation, lerpAlpha); 
		transform.localScale = Vector3.Lerp(previousTransformRecord.localScale, nextTransformRecord.localScale, lerpAlpha);
	}

	private void RestoreCameraRecord(CameraRecord previousCameraRecord, CameraRecord nextCameraRecord, float nextRecordDeltaTime) {
		TransformRecord previousTransformRecord = previousCameraRecord.cameraTransform;
		TransformRecord nextTransformRecord = nextCameraRecord.cameraTransform;
		float lerpAlpha = elapsedTimeSinceLastRecord / nextRecordDeltaTime;

		RestoreTransformRecord(settings.timeRewindCamera.transform, previousTransformRecord, nextTransformRecord, nextRecordDeltaTime);
    }

	private void RestoreAnimationRecord(Animator animator, AnimationRecord previousAnimationRecord, 
										AnimationRecord nextAnimationRecord, float deltaTime) {

		animator.playbackTime -= Time.deltaTime*rewindSpeed;
    }

	private void RestoreStateMachine(StateMachine stateMachine) {
		settings.TimeForwardStateMachine.CurrentStateObject = stateMachine.CurrentStateObject;
    }

}