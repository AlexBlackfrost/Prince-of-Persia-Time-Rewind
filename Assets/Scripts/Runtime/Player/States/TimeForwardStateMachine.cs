using Cinemachine;
using HFSM;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class TimeForwardStateMachine : StateMachine {
	[Serializable]
	public class TimeForwardSettings {
		public TimeRewinder TimeRewinder { get; set; }
		[field: SerializeField] public CinemachineFreeLook FreeLookCamera { get; set; }
		public CinemachineVirtualCamera timeRewindCamera;
		public Transform Transform { get; set; }
		public Camera Camera { get; set; }
		public Animator Animator { get; set; }
		public InputController InputController { get; set; }
		public Dictionary<Type, StateObject> StateObjects { get; set; }
		public SkinnedMeshRenderer SkinnedMeshRenderer { get; set; }
		public CharacterMovement CharacterMovement { get; set; }
	}

	private TimeForwardSettings settings;
	private bool timeIsRewinding;
	private float elapsedTimeSinceLastRecord;
	private PlayerRecord previousRecord, nextRecord;
	private float rewindSpeed = 0.1f;
	private NoneState noneState;
	private int recordedFrames = 0;

	public TimeForwardStateMachine(UpdateMode updateMode, TimeForwardSettings settings, params StateObject[] states) : base(updateMode, states) {
		this.settings = settings;
		noneState = new NoneState();
		
		timeIsRewinding = false;
		settings.InputController.TimeRewind.performed += OnTimeRewindPressed;
		TimeRewindManager.TimeRewindStart += OnTimeRewindStart;
		TimeRewindManager.TimeRewindStop += OnTimeRewindStop;
	}

    protected override void OnUpdate() {
		if (timeIsRewinding) {
			RewindPlayerRecord();
		}
	}

    protected override void OnLateUpdate() {
        if (timeIsRewinding) {
			//RewindPlayerRecord();
        } else {
			if(settings.TimeRewinder.records.Count != 0) {
				PlayerRecord playerRecord = settings.TimeRewinder.records.Pop();
				CameraRecord cameraRecord = playerRecord.cameraRecord;
				cameraRecord.cameraTransform = RecordUtils.RecordCameraData(settings.Camera).cameraTransform;
				playerRecord.cameraRecord = cameraRecord;
				settings.TimeRewinder.records.Push(playerRecord);
			}
			
			SavePlayerRecord();
        }
	}

	protected override void OnEnter() {}

	private void OnTimeRewindPressed(CallbackContext ctx) {
		timeIsRewinding = !timeIsRewinding;
        if (timeIsRewinding) {
			TimeRewindManager.StartTimeRewind();
		} else {
			TimeRewindManager.StopTimeRewind();
		}
	}

	private void OnTimeRewindStart() {
		elapsedTimeSinceLastRecord = 0;
		previousRecord = settings.TimeRewinder.records.Pop();

		// Animation
		settings.Animator.speed = 0;
		settings.Animator.enabled = false;
		//settings.Animator.applyRootMotion = false;
		//settings.Animator.Update(0);

		// Camera
		settings.timeRewindCamera.transform.position = settings.Camera.transform.position;
		settings.timeRewindCamera.transform.rotation = settings.Camera.transform.rotation;
		settings.timeRewindCamera.gameObject.SetActive(true);
		settings.FreeLookCamera.gameObject.SetActive(false);

		// State machine
		CurrentStateObject.Exit();
		// Do not change state using ChangeState() so that OnStateEnter is not triggered after rewind stops.
		CurrentStateObject = noneState;
	}

	private void OnTimeRewindStop() {
		// Animation
		//settings.Animator.enabled = true;
		settings.Animator.speed = 1;
		settings.Animator.applyRootMotion = previousRecord.animationRecord.applyRootMotion;
		RestoreAnimatorParameters();
        int layer = 0;
		float lerpAlpha = elapsedTimeSinceLastRecord / nextRecord.deltaTime;
        if (previousRecord.animationRecord.isInTransition) {
			TransitionRecord previousTransitionRecord = previousRecord.animationRecord.transitionRecord;
			TransitionRecord nextTransitionRecord = nextRecord.animationRecord.transitionRecord;
			float currentStateNormalizedTime = Mathf.Lerp(previousRecord.animationRecord.normalizedTime,
														  nextRecord.animationRecord.normalizedTime,
														  lerpAlpha);
			float nextStateNormalizedTime = Mathf.Lerp(previousTransitionRecord.nextStateNormalizedTime, 
													   nextTransitionRecord.nextStateNormalizedTime, 
													   lerpAlpha);
			float transitionNormalizedTime = Mathf.Lerp(previousTransitionRecord.normalizedTime,
														nextTransitionRecord.normalizedTime,
														lerpAlpha);
			/*settings.Animator.CrossFadeInFixedTime(previousTransitionRecord.nextStateNameHash,
												   previousTransitionRecord.transitionDuration ,
												   layer,
												   normalizedTime);*/
			Debug.Log("Is in transition, normalized time: "+ nextStateNormalizedTime);
			settings.Animator.Play(previousRecord.animationRecord.shortNameHash,
								   layer,
								   currentStateNormalizedTime);
			//settings.Animator.Update(0.0f);

			settings.Animator.CrossFade(previousTransitionRecord.nextStateNameHash, 
										previousTransitionRecord.transitionDuration, 
										layer,
										nextStateNormalizedTime,
										transitionNormalizedTime);
		} else {
			float normalizedTime = Mathf.Lerp(previousRecord.animationRecord.normalizedTime, 
											  nextRecord.animationRecord.normalizedTime, 
											  lerpAlpha);
			settings.Animator.Play(previousRecord.animationRecord.shortNameHash,
								   layer,
								   normalizedTime);
			settings.Animator.Update(0.0f); //Call Update so that root motion is applied
		}


		// Camera
		settings.timeRewindCamera.gameObject.SetActive(false);
		settings.FreeLookCamera.gameObject.SetActive(true);

		// State machine
		RestoreStateMachine(nextRecord.stateMachine);
    }

    private void SavePlayerRecord() {
		PlayerRecord playerRecord = RecordUtils.RecordPlayerData(settings.Transform,
																 settings.Camera,
																 this,
																 settings.Animator,
																 settings.SkinnedMeshRenderer.bones);

		settings.TimeRewinder.records.Push(playerRecord);
	}

	private void RewindPlayerRecord() {
		if (settings.TimeRewinder.records.Count != 0) {
			nextRecord = settings.TimeRewinder.records.Peek();

			while (elapsedTimeSinceLastRecord > previousRecord.deltaTime && settings.TimeRewinder.records.Count != 0) {
				//elapsedTimeSinceLastRecord -= nextRecord.deltaTime;
				//previousRecord = nextRecord;
				//nextRecord = settings.TimeRewinder.records.Pop();
				elapsedTimeSinceLastRecord -= previousRecord.deltaTime;
				previousRecord = settings.TimeRewinder.records.Pop();
				nextRecord = settings.TimeRewinder.records.Peek(); 
			}

			RestorePlayerRecord(previousRecord, nextRecord);
			elapsedTimeSinceLastRecord += Time.deltaTime * rewindSpeed;
		}
	}


	private void RestorePlayerRecord(PlayerRecord previousRecord, PlayerRecord nextRecord) {
		RestoreTransformRecord(settings.Transform, previousRecord.playerTransform, nextRecord.playerTransform, 
							   previousRecord.deltaTime);

		RestoreCameraRecord(settings.timeRewindCamera, previousRecord.cameraRecord, nextRecord.cameraRecord, 
							previousRecord.deltaTime);

		RestoreAnimationRecord(settings.Animator, previousRecord.animationRecord, nextRecord.animationRecord, 
							   previousRecord.deltaTime);

		Debug.Log("Rewinding... " + nextRecord.stateMachine.GetCurrentStateName());
	}

	private void RestoreTransformRecord(Transform transform, TransformRecord previousTransformRecord,
										TransformRecord nextTransformRecord, float previousRecordDeltaTime) {

		float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;
        
		//settings.CharacterMovement.CharacterController.Move(Vector3.Lerp(previousTransformRecord.position, nextTransformRecord.position, lerpAlpha) - transform.position);
		transform.position = Vector3.Lerp(previousTransformRecord.position, nextTransformRecord.position, lerpAlpha);
		transform.rotation = Quaternion.Slerp(previousTransformRecord.rotation, nextTransformRecord.rotation, lerpAlpha);
		transform.localScale = Vector3.Lerp(previousTransformRecord.localScale, nextTransformRecord.localScale, lerpAlpha);
        
	} 

	private void RestoreCameraRecord(CinemachineVirtualCamera timeRewindCamera, CameraRecord previousCameraRecord, 
									 CameraRecord nextCameraRecord, float previousRecordDeltaTime) {

		TransformRecord previousTransformRecord = previousCameraRecord.cameraTransform;
		TransformRecord nextTransformRecord = nextCameraRecord.cameraTransform;
		float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

		RestoreTransformRecord(timeRewindCamera.transform, previousTransformRecord, nextTransformRecord, previousRecordDeltaTime);
	}

	private void RestoreAnimationRecord(Animator animator, AnimationRecord previousAnimationRecord, 
										AnimationRecord nextAnimationRecord, float previousRecordDeltaTime) {

		float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime; 
		int layer = 0;
		if (previousAnimationRecord.isInTransition &&
			nextAnimationRecord.isInTransition &&
			previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash) {

			TransitionRecord previousTransitionRecord = previousRecord.animationRecord.transitionRecord;
			TransitionRecord nextTransitionRecord = nextRecord.animationRecord.transitionRecord;


			float currentStateNormalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
														  nextAnimationRecord.normalizedTime,
														  lerpAlpha);
			float nextStateNormalizedTime = Mathf.Lerp(previousTransitionRecord.nextStateNormalizedTime,
													   nextTransitionRecord.nextStateNormalizedTime,
													   lerpAlpha);
			float transitionNormalizedTime = Mathf.Lerp(previousTransitionRecord.normalizedTime,
														nextTransitionRecord.normalizedTime,
														lerpAlpha);

			// replace next and current
			//settings.Animator.speed = 1;
			/*
			Debug.Log("Is in transition, normalized time: " + nextStateNormalizedTime);
			float currentLength = previousAnimationRecord.duration;
			float currentTime = currentStateNormalizedTime * currentLength;
			float transitionTime = nextStateNormalizedTime * previousTransitionRecord.nextStateDuration;
			float transitionLength = transitionTime / transitionNormalizedTime;

			float playTime = ((currentTime - transitionTime) / currentLength) % 1;
			animator.Play(previousAnimationRecord.shortNameHash, layer, playTime);
			animator.Update(0f);

			//set crossfade time and update to the required point
			float crossPlayTime = (transitionTime / previousTransitionRecord.nextStateDuration)% 1f;
			animator.CrossFade(previousTransitionRecord.nextStateNameHash, transitionLength/currentLength, layer, crossPlayTime);
			animator.Update(transitionTime);*/
			
			settings.Animator.speed = 1;
			animator.Play(previousAnimationRecord.shortNameHash, layer, currentStateNormalizedTime);
			animator.Update(0.0f);

			float nextStateFixedTime = nextStateNormalizedTime * previousTransitionRecord.nextStateDuration;
			// CrossFadeInFixedTime only works if transitionInfo.DurationUnity is fixed. If it's Normalized, use CrossFade instead.
			animator.CrossFadeInFixedTime(previousTransitionRecord.nextStateNameHash, previousTransitionRecord.transitionDuration, 
										  layer, nextStateFixedTime, transitionNormalizedTime);
			animator.Update(0.0f);
			settings.Animator.speed = 0;
			Debug.Log("Transition previous original normalized time = " + previousTransitionRecord.normalizedTime);
			Debug.Log("Transition next original normalized time = " + nextTransitionRecord.normalizedTime);
			Debug.Log("Case 0 Current anim short name hash: " + previousAnimationRecord.shortNameHash +
					  " Next anim short name hash: " + nextTransitionRecord.nextStateNameHash +
					  " Current anim normalized time: " + currentStateNormalizedTime +
					  " Next anim normalized time: " + nextStateNormalizedTime +
					  " TransitionNormalizedTime: " + transitionNormalizedTime); 

		}else if (previousAnimationRecord.isInTransition &&
				  nextAnimationRecord.isInTransition &&
				  previousAnimationRecord.shortNameHash != nextAnimationRecord.shortNameHash) {

			// Here we need to interpolate between frames belonging to different transitions, that is,
			// interpolating between the first frame of a transition and the last frame of another transition
			
			TransitionRecord previousTransitionRecord = previousRecord.animationRecord.transitionRecord;
			TransitionRecord nextTransitionRecord = nextRecord.animationRecord.transitionRecord;


			float currentStateNormalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
														  nextAnimationRecord.normalizedTime,
														  lerpAlpha);
			float nextStateNormalizedTime = Mathf.Lerp(previousTransitionRecord.nextStateNormalizedTime,
													   nextTransitionRecord.nextStateNormalizedTime,
													   lerpAlpha);
			float transitionNormalizedTime = Mathf.Lerp(previousTransitionRecord.normalizedTime,
														nextTransitionRecord.normalizedTime,
														lerpAlpha);
			settings.Animator.speed = 1;
			float normalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
											  nextAnimationRecord.transitionRecord.nextStateNormalizedTime,
											  lerpAlpha);
			animator.Play(nextAnimationRecord.shortNameHash, layer, normalizedTime);
			animator.Update(0.0f);
			settings.Animator.speed = 0;
			Debug.Log("Case1 Current anim short name hash: " + previousAnimationRecord.shortNameHash +
					  " Next anim short name hash: " + nextTransitionRecord.nextStateNameHash +
					  " previous anim normalized time: " + previousAnimationRecord.normalizedTime +
					  " Next anim normalized time: " + nextAnimationRecord.transitionRecord.nextStateNormalizedTime +
					  " TransitionNormalizedTime: " + transitionNormalizedTime);


		} else if(!previousAnimationRecord.isInTransition && nextAnimationRecord.isInTransition) {
			// Here we need to interpolate between the last frame of a transition and the first frame after the transition ends
			settings.Animator.speed = 1;
			TransitionRecord transitionRecord = nextAnimationRecord.transitionRecord;
			float transitionNormalizedTime = transitionRecord.normalizedTime + 
											 (previousRecord.deltaTime-elapsedTimeSinceLastRecord) / 
											 transitionRecord.transitionDuration;
            if (transitionNormalizedTime < 1) {
				float nextStateNormalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
															  transitionRecord.nextStateNormalizedTime,
															  lerpAlpha);
				float nextStateFixedTime = nextStateNormalizedTime * transitionRecord.nextStateDuration;
				float currentNormalizedTime = nextAnimationRecord.normalizedTime +
												(previousRecord.deltaTime - elapsedTimeSinceLastRecord) / nextAnimationRecord.duration;
				float currentFixedTime = currentNormalizedTime * nextAnimationRecord.duration;
				float transitionFixedTime = transitionNormalizedTime * transitionRecord.transitionDuration;
				

				animator.Play(nextAnimationRecord.shortNameHash, layer, currentNormalizedTime); 
				animator.Update(0.0f);
				animator.CrossFadeInFixedTime(transitionRecord.nextStateNameHash, transitionRecord.transitionDuration,
											  layer, nextStateFixedTime, transitionNormalizedTime);
				animator.Update(0.0f);
				Debug.Log("Transition original normalized time = " + transitionRecord.normalizedTime + 
						  " PreviousRecordDeltaTime = " + previousRecord.deltaTime + 
						  " ElapsedTimeSinceLastRecord = " + elapsedTimeSinceLastRecord + 
						  " Transition duration = " + transitionRecord.transitionDuration);
				Debug.Log("Case 2Ttime<1 Current anim normalized time: " + currentNormalizedTime +
						  " current anim duration: " + previousAnimationRecord.duration +
						  " Next and previous record are same state: " + (previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash)+
						  " Next state normalized time: " + nextStateNormalizedTime +
						  " Transition normalizedTime: " +  transitionNormalizedTime);
			} else {
				float normalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
												  nextAnimationRecord.transitionRecord.nextStateNormalizedTime,
												  lerpAlpha);
				animator.Play(previousAnimationRecord.shortNameHash, layer, normalizedTime);
				animator.Update(0.0f);
				Debug.Log("Case2 Ttime>=1 Current anim normalized time: " + normalizedTime + 
						  " current anim duration: " + previousAnimationRecord.duration + 
						  " Next and previous record are same state: " + (previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash));
			}
			
		
		} else { 
			// (previousAnimationRecord.isInTransition && !nextAnimationRecord.isInTransition) || (!previousAnimationRecord.isInTransition && !nextAnimationRecord.isInTransition)
			// Here we need to interpolate between frames 
			// 1. where the previous frame is the first frame of a transition and the next frame is not part of a transition 
			// or
			// 2. that are not part of a transition
			settings.Animator.speed = 1;
			float normalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime, 
											  nextAnimationRecord.normalizedTime, 
											  lerpAlpha);
			animator.Play(previousAnimationRecord.shortNameHash, layer, normalizedTime);
			animator.Update(0.0f);
			settings.Animator.speed = 0;
			Debug.Log("Case3 Current anim normalized time: " + normalizedTime + " current anim duration: " + previousAnimationRecord.duration + " Next and previous record are same state: " + (previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash));
		}
	}

	private void RestoreAnimatorParameters() {
		foreach (AnimationParameter parameter in previousRecord.animationRecord.parameters) {
			switch (parameter.type) {
				case AnimatorControllerParameterType.Float:
					settings.Animator.SetFloat(parameter.nameHash, (float)parameter.value);
					break;

				case AnimatorControllerParameterType.Int:
					settings.Animator.SetInteger(parameter.nameHash, (int)parameter.value);
					break;

				case AnimatorControllerParameterType.Bool:
					settings.Animator.SetBool(parameter.nameHash, (bool)parameter.value);
					break;

				case AnimatorControllerParameterType.Trigger:
					if ((bool)parameter.value) {
						settings.Animator.SetTrigger(parameter.nameHash);
					}
					break;
			}
		}
	}

	private void RestoreStateMachine(StateMachine stateMachine) {
		StateObject newCurrentStateObject = settings.StateObjects[stateMachine.CurrentStateObject.GetType()];
		newCurrentStateObject.RestorePropertiesValues(stateMachine.CurrentStateObject);
		CurrentStateObject = newCurrentStateObject;
		//copy stateMachine.CurrentStateObject values to the original stateobject that is referenced by transitions objects and so on
	}

}