using Cinemachine;
using HFSM;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class TimeControlStateMachine : StateMachine {
	[Serializable]
	public class TimeControlSettings {
		public TimeRewinder TimeRewinder { get; set; }
		[field: SerializeField] public CinemachineFreeLook FreeLookCamera { get; set; }
		public CinemachineVirtualCamera timeRewindCamera;
		public Camera Camera { get; set; }
		public InputController InputController { get; set; }
		public Transform Transform { get; set; }
		public CharacterMovement CharacterMovement { get; set; }
		public Animator Animator { get; set; }
		public Dictionary<Type, StateObject> StateObjects { get; set; }
	}

	private TimeControlSettings settings;
	private bool timeIsRewinding;
	private float elapsedTimeSinceLastRecord;
	private PlayerRecord previousRecord, nextRecord;
	private float rewindSpeed = 0.1f;
	private NoneState noneState;
	private CinemachineBrain cinemachineBrain;

	private AnimationRecord lastAnimationRecord;
	private TransitionRecord lastInterruptedTransitionRecord;

	public TimeControlStateMachine(UpdateMode updateMode, TimeControlSettings settings, params StateObject[] states) : base(updateMode, states) {
		this.settings = settings;
		noneState = new NoneState();

		cinemachineBrain = settings.Camera.GetComponent<CinemachineBrain>();

		timeIsRewinding = false;
		TimeRewindManager.TimeRewindStart += OnTimeRewindStart;
		TimeRewindManager.TimeRewindStop += OnTimeRewindStop;
		
	}

    protected override void OnLateUpdate() {
		bool timeRewindPressed = settings.InputController.IsTimeRewindPressed();
		if (timeRewindPressed && !timeIsRewinding) {
			TimeRewindManager.StartTimeRewind();
		} else if (!timeRewindPressed && timeIsRewinding) {
			TimeRewindManager.StopTimeRewind();
        }
		timeIsRewinding = timeRewindPressed;

        if (timeIsRewinding) {
			RewindPlayerRecord();
			cinemachineBrain.ManualUpdate();
		} else {
			cinemachineBrain.ManualUpdate();
			SavePlayerRecord();
        }
	}

	private void OnTimeRewindStart() {
		elapsedTimeSinceLastRecord = 0;
		previousRecord = settings.TimeRewinder.records.Pop();
		nextRecord = settings.TimeRewinder.records.Peek();

		// Animation
		settings.Animator.speed = 0;
		settings.Animator.enabled = false;

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
		settings.Animator.enabled = true;
		settings.Animator.speed = 1;
		settings.Animator.applyRootMotion = previousRecord.animationRecord.applyRootMotion;
		RestoreAnimatorParameters();

		// Camera
		settings.timeRewindCamera.gameObject.SetActive(false);
		settings.FreeLookCamera.gameObject.SetActive(true);

		// State machine
		RestoreStateMachineRecord(settings.StateObjects, previousRecord.stateMachineRecord);
    }

    private void SavePlayerRecord() {
		PlayerRecord playerRecord = RecordUtils.RecordPlayerData(settings.Transform,
																 settings.Camera,
																 this,
																 settings.Animator,
																 settings.CharacterMovement);

		if(lastAnimationRecord.isInTransition && playerRecord.animationRecord.isInTransition &&
		   lastAnimationRecord.shortNameHash == playerRecord.animationRecord.shortNameHash && 
		   lastAnimationRecord.transitionRecord.nextStateNameHash != playerRecord.animationRecord.transitionRecord.nextStateNameHash) {

			lastInterruptedTransitionRecord = lastAnimationRecord.transitionRecord;
			lastInterruptedTransitionRecord.nextStateNormalizedTime += playerRecord.deltaTime / lastInterruptedTransitionRecord.nextStateDuration;
			lastInterruptedTransitionRecord.normalizedTime += playerRecord.deltaTime / lastInterruptedTransitionRecord.transitionDuration;
			AnimationRecord animationRecord = playerRecord.animationRecord;
			animationRecord.interruptedTransition = lastInterruptedTransitionRecord;
			animationRecord.IsInterruptingCurrentStateTransition = true;
			playerRecord.animationRecord = animationRecord;
        }

		if(lastAnimationRecord.isInTransition && playerRecord.animationRecord.isInTransition &&
		   lastAnimationRecord.shortNameHash == playerRecord.animationRecord.shortNameHash &&
		   lastAnimationRecord.transitionRecord.nextStateNameHash == playerRecord.animationRecord.transitionRecord.nextStateNameHash &&
		   lastAnimationRecord.normalizedTime == playerRecord.animationRecord.normalizedTime) {

			AnimationRecord animationRecord = playerRecord.animationRecord;
			animationRecord.interruptedTransition = lastInterruptedTransitionRecord;
			animationRecord.IsInterruptingCurrentStateTransition = true;
			playerRecord.animationRecord = animationRecord;
		}

		lastAnimationRecord = playerRecord.animationRecord;

		settings.TimeRewinder.records.Push(playerRecord);
	}

	private void RewindPlayerRecord() {
		while (elapsedTimeSinceLastRecord > previousRecord.deltaTime && settings.TimeRewinder.records.Count > 2) {
			elapsedTimeSinceLastRecord -= previousRecord.deltaTime;
			previousRecord = settings.TimeRewinder.records.Pop();
			nextRecord = settings.TimeRewinder.records.Peek(); 
		}

		RestorePlayerRecord(previousRecord, nextRecord);
		elapsedTimeSinceLastRecord += Time.deltaTime * rewindSpeed;
	}


	private void RestorePlayerRecord(PlayerRecord previousRecord, PlayerRecord nextRecord) {
		RestoreTransformRecord(settings.Transform, previousRecord.playerTransform, nextRecord.playerTransform, 
							   previousRecord.deltaTime);

		RestoreCameraRecord(settings.timeRewindCamera, previousRecord.cameraRecord, nextRecord.cameraRecord, 
							previousRecord.deltaTime);

		RestoreAnimationRecord(settings.Animator, previousRecord.animationRecord, nextRecord.animationRecord, 
							   previousRecord.deltaTime);

		RestoreCharacterMovementRecord(settings.CharacterMovement, previousRecord.characterMovementRecord,
									   nextRecord.characterMovementRecord, previousRecord.deltaTime);


		Debug.Log("Rewinding... " + nextRecord.stateMachineRecord.hierarchy[0].ToString());
	}

	private void RestoreTransformRecord(Transform transform, TransformRecord previousTransformRecord,
										TransformRecord nextTransformRecord, float previousRecordDeltaTime) {

		float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;
        
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
			!previousAnimationRecord.IsInterruptingCurrentStateTransition &&
			!nextAnimationRecord.IsInterruptingCurrentStateTransition
			/*previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash &&
			previousAnimationRecord.transitionRecord.nextStateNameHash == nextAnimationRecord.transitionRecord.nextStateNameHash &&
			previousAnimationRecord.normalizedTime != nextAnimationRecord.normalizedTime*/) {

			// Here we need to interpolate between two frames that belong to the same transition.

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
			Debug.Log("Case0 Current anim short name hash: " + previousAnimationRecord.shortNameHash +
					  " Next anim short name hash: " + nextTransitionRecord.nextStateNameHash +
					  " Current anim normalized time: " + currentStateNormalizedTime +
					  " Next anim normalized time: " + nextStateNormalizedTime +
					  " TransitionNormalizedTime: " + transitionNormalizedTime);

		} else if (previousAnimationRecord.isInTransition && nextAnimationRecord.isInTransition &&
				   previousAnimationRecord.IsInterruptingCurrentStateTransition &&
				   nextAnimationRecord.IsInterruptingCurrentStateTransition
				   /*previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash &&
				   previousAnimationRecord.transitionRecord.nextStateNameHash == nextAnimationRecord.transitionRecord.nextStateNameHash &&
				   previousAnimationRecord.normalizedTime == nextAnimationRecord.normalizedTime*/) {

			TransitionRecord interruptedTransition = previousAnimationRecord.interruptedTransition;
			animator.speed = 1;
			animator.Play(previousAnimationRecord.shortNameHash, layer, previousAnimationRecord.normalizedTime);
			animator.Update(0.0f);
			animator.CrossFadeInFixedTime(interruptedTransition.nextStateNameHash, interruptedTransition.transitionDuration, layer,
										  interruptedTransition.nextStateNormalizedTime * interruptedTransition.nextStateDuration,
										  interruptedTransition.normalizedTime);
			animator.Update(0.0f);
			float nextStateNormalizedTime = Mathf.Lerp(previousAnimationRecord.transitionRecord.nextStateNormalizedTime,
													   nextAnimationRecord.transitionRecord.nextStateNormalizedTime,
													   lerpAlpha);
			float nextStateFixedTime = nextStateNormalizedTime * previousAnimationRecord.transitionRecord.nextStateDuration;
			float normalizedTransitionTime = Mathf.Lerp(previousAnimationRecord.transitionRecord.normalizedTime,
														nextAnimationRecord.transitionRecord.normalizedTime,
														lerpAlpha);
			animator.CrossFadeInFixedTime(previousAnimationRecord.transitionRecord.nextStateNameHash,
										  previousAnimationRecord.transitionRecord.transitionDuration,
										  layer, nextStateFixedTime, normalizedTransitionTime);
			animator.Update(0.0f);
			animator.speed = 0;
			Debug.Log("Case1 Current anim short name hash: " + previousAnimationRecord.shortNameHash +
					  " Next anim short name hash: " + previousAnimationRecord.transitionRecord.nextStateNameHash +
					  " Current anim normalized time: " + previousAnimationRecord.normalizedTime +
					  " Next anim normalized time: " + nextStateNormalizedTime +
					  " TransitionNormalizedTime: " + normalizedTransitionTime +
					  " Interrupted Next anim short name hash: " + interruptedTransition.nextStateNameHash +
					  " Interrupted Next anim normalized time: " + interruptedTransition.nextStateNormalizedTime+
					  " Interrupted TransitionNormalizedTime: " + interruptedTransition.normalizedTime);

		} else if (previousAnimationRecord.isInTransition && nextAnimationRecord.isInTransition &&
				   previousAnimationRecord.IsInterruptingCurrentStateTransition &&
				   !nextAnimationRecord.IsInterruptingCurrentStateTransition
				   /*previousAnimationRecord.transitionRecord.nextStateNameHash != nextAnimationRecord.transitionRecord.nextStateNameHash*/) {

			/* Here we need to interpolate between a frame belonging to a transition that was interrupted by another transition
			*  and the first frame of such transition
			*/

			animator.speed = 1;
			TransitionRecord previousTransitionRecord = previousAnimationRecord.transitionRecord;
			TransitionRecord nextTransitionRecord = nextAnimationRecord.transitionRecord;

			float previousTransitionNormalizedTime = previousTransitionRecord.normalizedTime -
													 elapsedTimeSinceLastRecord / previousTransitionRecord.transitionDuration;
			if (previousTransitionNormalizedTime < 0) {
				// Before the transition was interrupted

				float currentStateNormalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
															  nextAnimationRecord.normalizedTime,
															  lerpAlpha);
				animator.Play(nextAnimationRecord.shortNameHash, layer, currentStateNormalizedTime);
				animator.Update(0.0f);

				float nextStateNormalizedTime = nextTransitionRecord.nextStateNormalizedTime +
												(previousRecord.deltaTime - elapsedTimeSinceLastRecord) /
												nextTransitionRecord.nextStateDuration;
				float nextStateFixedTime = nextStateNormalizedTime * nextTransitionRecord.nextStateDuration;
				float transitionNormalizedTime = nextTransitionRecord.normalizedTime +
												 (previousRecord.deltaTime - elapsedTimeSinceLastRecord) /
												 nextTransitionRecord.transitionDuration;
				animator.CrossFadeInFixedTime(nextTransitionRecord.nextStateNameHash, nextTransitionRecord.transitionDuration,
											  layer, nextStateFixedTime, transitionNormalizedTime);
				animator.Update(0.0f);

				Debug.Log("Case2 pTtime<0 Current anim short name hash: " + nextAnimationRecord.shortNameHash +
						  " Next anim short name hash: " + nextTransitionRecord.nextStateNameHash +
						  " current anim normalized time: " + currentStateNormalizedTime +
						  " Next anim normalized time: " + nextStateNormalizedTime +
						  " TransitionNormalizedTime: " + transitionNormalizedTime);
			} else {
				// After the transition was interrupted
				float currentStateNormalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
															  nextAnimationRecord.normalizedTime,
															  lerpAlpha);
				animator.Play(nextAnimationRecord.shortNameHash, layer, currentStateNormalizedTime);
				animator.Update(0.0f);

				float interruptedNextStateNormalizedTime = nextTransitionRecord.nextStateNormalizedTime +
														   (previousRecord.deltaTime - elapsedTimeSinceLastRecord) /
														   nextTransitionRecord.nextStateDuration;
				float interruptedNextStateFixedTime = interruptedNextStateNormalizedTime * nextTransitionRecord.nextStateDuration;
				float interruptedTransitionNormalizedTime = nextTransitionRecord.normalizedTime +
															(previousRecord.deltaTime - elapsedTimeSinceLastRecord) /
															nextTransitionRecord.transitionDuration;
				animator.CrossFadeInFixedTime(nextTransitionRecord.nextStateNameHash, nextTransitionRecord.transitionDuration,
											  layer, interruptedNextStateFixedTime, interruptedTransitionNormalizedTime);
				animator.Update(0.0f);

				float nextStateNormalizedTime = previousTransitionRecord.nextStateNormalizedTime -
												elapsedTimeSinceLastRecord / previousTransitionRecord.nextStateDuration;
				float nextStateFixedTime = nextStateNormalizedTime * previousTransitionRecord.nextStateDuration;
				float transitionNormalizedTime = previousTransitionRecord.normalizedTime -
												 elapsedTimeSinceLastRecord / previousTransitionRecord.transitionDuration;
				animator.CrossFadeInFixedTime(previousAnimationRecord.shortNameHash, previousTransitionRecord.transitionDuration,
											  layer, nextStateFixedTime, transitionNormalizedTime);
				animator.Update(0.0f);
				Debug.Log("Case2 pTtime>=0 Current anim short name hash: " + previousAnimationRecord.shortNameHash +
						  " Next anim short name hash: " + previousTransitionRecord.nextStateNameHash +
						  " current anim normalized time: " + currentStateNormalizedTime +
						  " Next anim normalized time: " + nextStateNormalizedTime +
						  " TransitionNormalizedTime: " + transitionNormalizedTime);
			}
			animator.speed = 0;
			/*
			float interruptedNextNormalizedTime = interruptedTransition.nextStateNormalizedTime +
												  (previousRecord.deltaTime - elapsedTimeSinceLastRecord) /
												  interruptedTransition.nextStateDuration;
			float interupptedNextFixedTime = interruptedNextNormalizedTime * interruptedTransition.nextStateDuration;
			float interruptedTransitionNormalizedTime = interruptedTransition.normalizedTime +
														(previousRecord.deltaTime - elapsedTimeSinceLastRecord) /
														interruptedTransition.transitionDuration;
			animator.CrossFadeInFixedTime(interruptedTransition.nextStateNameHash, interruptedTransition.transitionDuration, layer,
										  interupptedNextFixedTime,
										  interruptedTransitionNormalizedTime);
			animator.Update(0.0f);
			*/
		} else if (!previousAnimationRecord.isInTransition && nextAnimationRecord.isInTransition &&
				   nextAnimationRecord.IsInterruptingCurrentStateTransition) {

			/* Here we need to interpolate between the last frame of a transition (interrupting transition) 
			 * that interrupted another transition (interrupted transition), and the first frame after 
			 * the interrupting transition ends*/
			animator.speed = 0;
			float transitionNormalizedTime = nextAnimationRecord.transitionRecord.normalizedTime +
											 (previousRecord.deltaTime - elapsedTimeSinceLastRecord) /
											 nextAnimationRecord.transitionRecord.transitionDuration;
			if (transitionNormalizedTime > 1) {
				float currentStateNormalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
															  nextAnimationRecord.transitionRecord.nextStateNormalizedTime,
															  lerpAlpha);
				animator.Play(previousAnimationRecord.shortNameHash, layer, currentStateNormalizedTime);
				animator.Update(0.0f);
				Debug.Log("Case3 nTtime>1 Current anim short name hash: " + previousAnimationRecord.shortNameHash +
						  " current anim normalized time: " + currentStateNormalizedTime);
			} else {
				animator.Play(nextAnimationRecord.shortNameHash, layer, nextAnimationRecord.normalizedTime);
				animator.Update(0.0f);
				float nextInterruptedStateFixedTime = nextAnimationRecord.interruptedTransition.nextStateNormalizedTime *
													  nextAnimationRecord.interruptedTransition.nextStateDuration;

				animator.CrossFadeInFixedTime(nextAnimationRecord.interruptedTransition.nextStateNameHash,
											  nextAnimationRecord.interruptedTransition.transitionDuration,
											  layer, nextInterruptedStateFixedTime,
											  nextAnimationRecord.interruptedTransition.normalizedTime);
				animator.Update(0.0f);

				float nextStateNormalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
														   nextAnimationRecord.transitionRecord.nextStateNormalizedTime,
														   lerpAlpha);
				float nextStateFixedTime = nextStateNormalizedTime * nextAnimationRecord.transitionRecord.nextStateDuration;
				animator.CrossFadeInFixedTime(nextAnimationRecord.transitionRecord.nextStateNameHash,
											  nextAnimationRecord.transitionRecord.transitionDuration,
											  layer, nextStateFixedTime, transitionNormalizedTime);
				animator.Update(0.0f);
				Debug.Log("Case3 nTtime<=1 Current anim short name hash: " + nextAnimationRecord.shortNameHash +
						  " Next anim short name hash: " + nextAnimationRecord.transitionRecord.nextStateNameHash +
						  " current anim normalized time: " + nextAnimationRecord.normalizedTime +
						  " Next anim normalized time: " + nextStateNormalizedTime +
						  " TransitionNormalizedTime: " + transitionNormalizedTime);
			}
			animator.speed = 1;
		
		} else if (previousAnimationRecord.isInTransition &&
				   nextAnimationRecord.isInTransition &&
				   !previousAnimationRecord.IsInterruptingCurrentStateTransition &&
				   !nextAnimationRecord.IsInterruptingCurrentStateTransition &&
				   previousAnimationRecord.shortNameHash != nextAnimationRecord.shortNameHash) {


			/* Here we need to interpolate between frames belonging to different transitions, that is,
			interpolating between the first frame of a transition and the last frame of another transition.*/

			// I don't know if this scenario is possible.


		} else if (!previousAnimationRecord.isInTransition && nextAnimationRecord.isInTransition) {
			// Here we need to interpolate between the last frame of a transition and the first frame after the transition ends
			settings.Animator.speed = 1;
			TransitionRecord transitionRecord = nextAnimationRecord.transitionRecord;
			float transitionNormalizedTime = transitionRecord.normalizedTime +
											 (previousRecord.deltaTime - elapsedTimeSinceLastRecord) /
											 transitionRecord.transitionDuration;
			if (transitionNormalizedTime < 1) {
				float nextStateNormalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
															  transitionRecord.nextStateNormalizedTime,
															  lerpAlpha);
				float nextStateFixedTime = nextStateNormalizedTime * transitionRecord.nextStateDuration;
				float currentNormalizedTime = nextAnimationRecord.normalizedTime +
											  (previousRecord.deltaTime - elapsedTimeSinceLastRecord) / nextAnimationRecord.duration;


				animator.Play(nextAnimationRecord.shortNameHash, layer, currentNormalizedTime);
				animator.Update(0.0f);
				animator.CrossFadeInFixedTime(transitionRecord.nextStateNameHash, transitionRecord.transitionDuration,
											  layer, nextStateFixedTime, transitionNormalizedTime);
				animator.Update(0.0f);
				Debug.Log("Transition original normalized time = " + transitionRecord.normalizedTime +
						  " PreviousRecordDeltaTime = " + previousRecord.deltaTime +
						  " ElapsedTimeSinceLastRecord = " + elapsedTimeSinceLastRecord +
						  " Transition duration = " + transitionRecord.transitionDuration);
				Debug.Log("Case4 Ttime<1 Current anim normalized time: " + currentNormalizedTime +
						  " current anim duration: " + previousAnimationRecord.duration +
						  " Next and previous record are same state: " + (previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash) +
						  " Next state normalized time: " + nextStateNormalizedTime +
						  " Transition normalizedTime: " + transitionNormalizedTime);
			} else {
				float normalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
												  nextAnimationRecord.transitionRecord.nextStateNormalizedTime,
												  lerpAlpha);
				animator.Play(previousAnimationRecord.shortNameHash, layer, normalizedTime);
				animator.Update(0.0f);
				Debug.Log("Case4 Ttime>=1 Current anim normalized time: " + normalizedTime +
						  " current anim duration: " + previousAnimationRecord.duration +
						  " Next and previous record are same state: " + 
						  (previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash));
			}


		} else if (!previousAnimationRecord.isInTransition && !nextAnimationRecord.isInTransition) {
			// Here we need to interpolate between frames that do not belong to a transition
			settings.Animator.speed = 1;
			float normalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
											  nextAnimationRecord.normalizedTime,
											  lerpAlpha);
			animator.Play(previousAnimationRecord.shortNameHash, layer, normalizedTime);
			animator.Update(0.0f);
			settings.Animator.speed = 0;
			Debug.Log("Case5 Current anim hash: " + previousAnimationRecord.shortNameHash +
					  "Current anim normalized time: " + normalizedTime + " Next and previous record are same state: " + 
					  (previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash));
		
		} else if (previousAnimationRecord.isInTransition && !nextAnimationRecord.isInTransition) {
			/* Here we need to interpolate between a frame that does not belong to a transition and
			the first frame of a transition */ 
			settings.Animator.speed = 1;
			TransitionRecord transitionRecord = previousAnimationRecord.transitionRecord;
			float transitionNormalizedTime = transitionRecord.normalizedTime -
											 elapsedTimeSinceLastRecord / transitionRecord.transitionDuration;

			if (transitionNormalizedTime > 0) {
				float nextStateNormalizedTime = transitionRecord.nextStateNormalizedTime -
												elapsedTimeSinceLastRecord / transitionRecord.nextStateDuration;
				float nextStateFixedTime = nextStateNormalizedTime * transitionRecord.nextStateDuration;
				float currentNormalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
														 nextAnimationRecord.normalizedTime,
														 lerpAlpha);


				animator.Play(previousAnimationRecord.shortNameHash, layer, currentNormalizedTime);
				animator.Update(0.0f);
				animator.CrossFadeInFixedTime(transitionRecord.nextStateNameHash, transitionRecord.transitionDuration,
											  layer, nextStateFixedTime, transitionNormalizedTime);
				animator.Update(0.0f);
				Debug.Log("Case6 Ttime>0 Current anim normalized time: " + currentNormalizedTime +
						  " current anim duration: " + previousAnimationRecord.duration +
						  " Next and previous record are same state: " + (previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash) +
						  " Next state normalized time: " + nextStateNormalizedTime +
						  " Transition normalizedTime: " + transitionNormalizedTime);
			} else {
				float normalizedTime = Mathf.Lerp(previousAnimationRecord.normalizedTime,
												  nextAnimationRecord.normalizedTime,
												  lerpAlpha);
				animator.Play(previousAnimationRecord.shortNameHash, layer, normalizedTime);
				animator.Update(0.0f);
				Debug.Log("Case6 Ttime<=0 Current anim normalized time: " + normalizedTime + " current anim duration: " +
							previousAnimationRecord.duration + " Next and previous record are same state: " + 
							(previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash));
			}
			settings.Animator.speed = 0;
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

	private void RestoreStateMachineRecord(Dictionary<Type, StateObject> stateObjects, StateMachineRecord stateMachineRecord) {
		for(int i=0; i < stateMachineRecord.hierarchy.Length-1; i++) {
			Type id = stateMachineRecord.hierarchy[i];
			StateMachine stateMachine = (StateMachine)stateObjects[id];
			stateMachine.IsActive = true;
			stateMachine.CurrentStateObject = stateObjects[stateMachineRecord.hierarchy[i + 1]];
			stateMachine.RestoreFieldsAndProperties(stateMachineRecord.stateObjectRecords[i]);
		}
		int leaftStateIndex = stateMachineRecord.hierarchy.Length - 1;
		Type leaftStateId = stateMachineRecord.hierarchy[leaftStateIndex];
		StateObject leafState = stateObjects[leaftStateId];
		leafState.IsActive = true;
		leafState.RestoreFieldsAndProperties(stateMachineRecord.stateObjectRecords[leaftStateIndex]);
		CurrentStateObject = stateObjects[stateMachineRecord.hierarchy[0]];

		/*
		StateObject newCurrentStateObject = settings.StateObjects[stateMachine.CurrentStateObject.GetType()];
		newCurrentStateObject.RestorePropertiesValues(stateMachine.CurrentStateObject);
		CurrentStateObject = newCurrentStateObject;*/
		//copy stateMachine.CurrentStateObject values to the original stateobject that is referenced by transitions objects and so on
	}


	private void RestoreCharacterMovementRecord(CharacterMovement characterMovement,
												CharacterMovementRecord previousCharacterMovementRecord,
												CharacterMovementRecord nextCharacterMovementRecord,
												float previousRecordDeltaTime) {

		float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;
		Vector3 velocity = Vector3.Lerp(previousCharacterMovementRecord.velocity,
										nextCharacterMovementRecord.velocity,
										lerpAlpha);
		characterMovement.Velocity = velocity;
    }

}