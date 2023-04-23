using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTimeControl {

    private AnimationRecord lastAnimationRecord;
    private TransitionRecord[] lastInterruptedTransitionRecordInLayer;
    private Animator animator;

	public AnimationTimeControl(Animator animator) {
		this.animator = animator;
		lastInterruptedTransitionRecordInLayer = new TransitionRecord[animator.layerCount];
		lastAnimationRecord.animationLayerRecords = new AnimationLayerRecord[animator.layerCount];
	}

    public void OnTimeRewindStart() {
        animator.speed = 0;
        animator.enabled = false;
    }

    public void OnTimeRewindStop(AnimationRecord previousRecord, AnimationRecord nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        RestoreAnimatorParameters(previousRecord); //Restore parameters before restoring animation record or it won't work.
        RestoreAnimationRecord(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
        animator.enabled = true;
        animator.speed = 1;
        animator.applyRootMotion = previousRecord.applyRootMotion;
    }

	public AnimationRecord RecordAnimationData() {
		AnimationParameter[] parameters = RecordAnimatorParameters(animator);
		AnimationLayerRecord[] animationLayerRecords = new AnimationLayerRecord[animator.layerCount];

		for (int layer = 0; layer < animator.layerCount; layer++) {
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
			AnimationLayerRecord animationLayerRecord = new AnimationLayerRecord(layer, animator.GetLayerWeight(layer), stateInfo.shortNameHash,
																				 stateInfo.normalizedTime, stateInfo.length);

			string output = "ShortNameHash: " + stateInfo.shortNameHash +
							" NormalizedTime: " + stateInfo.normalizedTime;

			if (animator.IsInTransition(layer)) {
				AnimatorTransitionInfo transitionInfo = animator.GetAnimatorTransitionInfo(layer);
				AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(layer);
				TransitionRecord transitionRecord = new TransitionRecord(nextStateInfo.shortNameHash,
																		 transitionInfo.normalizedTime,
																		 transitionInfo.duration,
																		 nextStateInfo.normalizedTime,
																		 nextStateInfo.length);

				animationLayerRecord.isInTransition = true;
				animationLayerRecord.transitionRecord = transitionRecord;

				output += " NextNameHash: " + nextStateInfo.shortNameHash +
							" NextStateNormalizedTime: " + nextStateInfo.normalizedTime +
							" TransitionDuration: " + transitionInfo.duration +
							" TransitionNormalizedTime: " + transitionInfo.normalizedTime;
			}

			animationLayerRecords[layer] = animationLayerRecord;
			Debug.Log(output);
		}

		AnimationRecord animationRecord = new AnimationRecord(parameters, animationLayerRecords, animator.applyRootMotion);
		TrackInterruptedTransitions(ref animationRecord, Time.deltaTime);
		return animationRecord;
	}

	private static AnimationParameter[] RecordAnimatorParameters(Animator animator) {
		AnimationParameter[] parameters = new AnimationParameter[animator.parameterCount];
		int i = 0;
		foreach (AnimatorControllerParameter parameter in animator.parameters) {
			object value = null;
			switch (parameter.type) {
				case AnimatorControllerParameterType.Float:
					value = animator.GetFloat(parameter.nameHash);
					break;

				case AnimatorControllerParameterType.Int:
					value = animator.GetInteger(parameter.nameHash);
					break;

				case AnimatorControllerParameterType.Bool:
				case AnimatorControllerParameterType.Trigger:
					value = animator.GetBool(parameter.nameHash);
					break;
			}
			parameters[i++] = new AnimationParameter(parameter.type, parameter.nameHash, value);

		}
		return parameters;
	}

	public void RestoreAnimatorParameters(AnimationRecord previousRecord) {
		foreach (AnimationParameter parameter in previousRecord.parameters) {
			switch (parameter.type) {
				case AnimatorControllerParameterType.Float:
					animator.SetFloat(parameter.nameHash, (float)parameter.value);
					break;

				case AnimatorControllerParameterType.Int:
					animator.SetInteger(parameter.nameHash, (int)parameter.value);
					break;

				case AnimatorControllerParameterType.Bool:
					animator.SetBool(parameter.nameHash, (bool)parameter.value);
					break;

				case AnimatorControllerParameterType.Trigger:
					if ((bool)parameter.value) {
						animator.SetTrigger(parameter.nameHash);
					}
					break;
			}
		}
	}

	public void RestoreAnimationRecord(AnimationRecord previousRecord, AnimationRecord nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
		for (int layer = 0; layer < animator.layerCount; layer++) {
			RestoreAnimationLayerRecord(animator, previousRecord.animationLayerRecords[layer], nextRecord.animationLayerRecords[layer],
										layer, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
		}
	}

	private void RestoreAnimationLayerRecord(Animator animator, AnimationLayerRecord previousAnimationLayerRecord, AnimationLayerRecord nextAnimationLayerRecord,
											 int layer, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {

		float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;
		float layerWeight = Mathf.Lerp(previousAnimationLayerRecord.layerWeight, nextAnimationLayerRecord.layerWeight, lerpAlpha);
		animator.SetLayerWeight(layer, layerWeight);

		if (previousAnimationLayerRecord.isInTransition &&
			nextAnimationLayerRecord.isInTransition &&
			!previousAnimationLayerRecord.IsInterruptingCurrentStateTransition &&
			!nextAnimationLayerRecord.IsInterruptingCurrentStateTransition
			/*previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash &&
			previousAnimationRecord.transitionRecord.nextStateNameHash == nextAnimationRecord.transitionRecord.nextStateNameHash &&
			previousAnimationRecord.normalizedTime != nextAnimationRecord.normalizedTime*/) {

			// Here we need to interpolate between two frames that belong to the same transition.

			TransitionRecord previousTransitionRecord = previousAnimationLayerRecord.transitionRecord;
			TransitionRecord nextTransitionRecord = nextAnimationLayerRecord.transitionRecord;


			float currentStateNormalizedTime = Mathf.Lerp(previousAnimationLayerRecord.normalizedTime,
														  nextAnimationLayerRecord.normalizedTime,
														  lerpAlpha);
			float nextStateNormalizedTime = Mathf.Lerp(previousTransitionRecord.nextStateNormalizedTime,
													   nextTransitionRecord.nextStateNormalizedTime,
													   lerpAlpha);
			float transitionNormalizedTime = Mathf.Lerp(previousTransitionRecord.normalizedTime,
														nextTransitionRecord.normalizedTime,
														lerpAlpha);

			animator.speed = 1;
			animator.Play(previousAnimationLayerRecord.shortNameHash, layer, currentStateNormalizedTime);
			animator.Update(0.0f);

			float nextStateFixedTime = nextStateNormalizedTime * previousTransitionRecord.nextStateDuration;
			// CrossFadeInFixedTime only works if transitionInfo.DurationUnity is fixed. If it's Normalized, use CrossFade instead.
			animator.CrossFadeInFixedTime(previousTransitionRecord.nextStateNameHash, previousTransitionRecord.transitionDuration,
										  layer, nextStateFixedTime, transitionNormalizedTime);
			animator.Update(0.0f);
			animator.speed = 0;
			Debug.Log("Transition previous original normalized time = " + previousTransitionRecord.normalizedTime);
			Debug.Log("Transition next original normalized time = " + nextTransitionRecord.normalizedTime);
			Debug.Log("Case0 Current anim short name hash: " + previousAnimationLayerRecord.shortNameHash +
					  " Next anim short name hash: " + nextTransitionRecord.nextStateNameHash +
					  " Current anim normalized time: " + currentStateNormalizedTime +
					  " Next anim normalized time: " + nextStateNormalizedTime +
					  " TransitionNormalizedTime: " + transitionNormalizedTime);

		} else if (previousAnimationLayerRecord.isInTransition && nextAnimationLayerRecord.isInTransition &&
				   previousAnimationLayerRecord.IsInterruptingCurrentStateTransition &&
				   nextAnimationLayerRecord.IsInterruptingCurrentStateTransition
				   /*previousAnimationRecord.shortNameHash == nextAnimationRecord.shortNameHash &&
				   previousAnimationRecord.transitionRecord.nextStateNameHash == nextAnimationRecord.transitionRecord.nextStateNameHash &&
				   previousAnimationRecord.normalizedTime == nextAnimationRecord.normalizedTime*/) {

			TransitionRecord interruptedTransition = previousAnimationLayerRecord.interruptedTransition;
			animator.speed = 1;
			animator.Play(previousAnimationLayerRecord.shortNameHash, layer, previousAnimationLayerRecord.normalizedTime);
			animator.Update(0.0f);
			animator.CrossFadeInFixedTime(interruptedTransition.nextStateNameHash, interruptedTransition.transitionDuration, layer,
										  interruptedTransition.nextStateNormalizedTime * interruptedTransition.nextStateDuration,
										  interruptedTransition.normalizedTime);
			animator.Update(0.0f);
			float nextStateNormalizedTime = Mathf.Lerp(previousAnimationLayerRecord.transitionRecord.nextStateNormalizedTime,
													   nextAnimationLayerRecord.transitionRecord.nextStateNormalizedTime,
													   lerpAlpha);
			float nextStateFixedTime = nextStateNormalizedTime * previousAnimationLayerRecord.transitionRecord.nextStateDuration;
			float normalizedTransitionTime = Mathf.Lerp(previousAnimationLayerRecord.transitionRecord.normalizedTime,
														nextAnimationLayerRecord.transitionRecord.normalizedTime,
														lerpAlpha);
			animator.CrossFadeInFixedTime(previousAnimationLayerRecord.transitionRecord.nextStateNameHash,
										  previousAnimationLayerRecord.transitionRecord.transitionDuration,
										  layer, nextStateFixedTime, normalizedTransitionTime);
			animator.Update(0.0f);
			animator.speed = 0;
			Debug.Log("Case1 Current anim short name hash: " + previousAnimationLayerRecord.shortNameHash +
					  " Next anim short name hash: " + previousAnimationLayerRecord.transitionRecord.nextStateNameHash +
					  " Current anim normalized time: " + previousAnimationLayerRecord.normalizedTime +
					  " Next anim normalized time: " + nextStateNormalizedTime +
					  " TransitionNormalizedTime: " + normalizedTransitionTime +
					  " Interrupted Next anim short name hash: " + interruptedTransition.nextStateNameHash +
					  " Interrupted Next anim normalized time: " + interruptedTransition.nextStateNormalizedTime +
					  " Interrupted TransitionNormalizedTime: " + interruptedTransition.normalizedTime);

		} else if (previousAnimationLayerRecord.isInTransition && nextAnimationLayerRecord.isInTransition &&
				   previousAnimationLayerRecord.IsInterruptingCurrentStateTransition &&
				   !nextAnimationLayerRecord.IsInterruptingCurrentStateTransition
				   /*previousAnimationRecord.transitionRecord.nextStateNameHash != nextAnimationRecord.transitionRecord.nextStateNameHash*/) {

			/* Here we need to interpolate between a frame belonging to a transition that was interrupted by another transition
			*  and the first frame of such transition
			*/

			animator.speed = 1;
			TransitionRecord previousTransitionRecord = previousAnimationLayerRecord.transitionRecord;
			TransitionRecord nextTransitionRecord = nextAnimationLayerRecord.transitionRecord;

			float previousTransitionNormalizedTime = previousTransitionRecord.normalizedTime -
													 elapsedTimeSinceLastRecord / previousTransitionRecord.transitionDuration;
			if (previousTransitionNormalizedTime < 0) {
				// Before the transition was interrupted

				float currentStateNormalizedTime = Mathf.Lerp(previousAnimationLayerRecord.normalizedTime,
															  nextAnimationLayerRecord.normalizedTime,
															  lerpAlpha);
				animator.Play(nextAnimationLayerRecord.shortNameHash, layer, currentStateNormalizedTime);
				animator.Update(0.0f);

				float nextStateNormalizedTime = nextTransitionRecord.nextStateNormalizedTime +
												(previousRecordDeltaTime - elapsedTimeSinceLastRecord) /
												nextTransitionRecord.nextStateDuration;
				float nextStateFixedTime = nextStateNormalizedTime * nextTransitionRecord.nextStateDuration;
				float transitionNormalizedTime = nextTransitionRecord.normalizedTime +
												 (previousRecordDeltaTime - elapsedTimeSinceLastRecord) /
												 nextTransitionRecord.transitionDuration;
				animator.CrossFadeInFixedTime(nextTransitionRecord.nextStateNameHash, nextTransitionRecord.transitionDuration,
											  layer, nextStateFixedTime, transitionNormalizedTime);
				animator.Update(0.0f);

				Debug.Log("Case2 pTtime<0 Current anim short name hash: " + nextAnimationLayerRecord.shortNameHash +
						  " Next anim short name hash: " + nextTransitionRecord.nextStateNameHash +
						  " current anim normalized time: " + currentStateNormalizedTime +
						  " Next anim normalized time: " + nextStateNormalizedTime +
						  " TransitionNormalizedTime: " + transitionNormalizedTime);
			} else {
				// After the transition was interrupted
				float currentStateNormalizedTime = Mathf.Lerp(previousAnimationLayerRecord.normalizedTime,
															  nextAnimationLayerRecord.normalizedTime,
															  lerpAlpha);
				animator.Play(nextAnimationLayerRecord.shortNameHash, layer, currentStateNormalizedTime);
				animator.Update(0.0f);

				float interruptedNextStateNormalizedTime = nextTransitionRecord.nextStateNormalizedTime +
														   (previousRecordDeltaTime - elapsedTimeSinceLastRecord) /
														   nextTransitionRecord.nextStateDuration;
				float interruptedNextStateFixedTime = interruptedNextStateNormalizedTime * nextTransitionRecord.nextStateDuration;
				float interruptedTransitionNormalizedTime = nextTransitionRecord.normalizedTime +
															(previousRecordDeltaTime - elapsedTimeSinceLastRecord) /
															nextTransitionRecord.transitionDuration;
				animator.CrossFadeInFixedTime(nextTransitionRecord.nextStateNameHash, nextTransitionRecord.transitionDuration,
											  layer, interruptedNextStateFixedTime, interruptedTransitionNormalizedTime);
				animator.Update(0.0f);

				float nextStateNormalizedTime = previousTransitionRecord.nextStateNormalizedTime -
												elapsedTimeSinceLastRecord / previousTransitionRecord.nextStateDuration;
				float nextStateFixedTime = nextStateNormalizedTime * previousTransitionRecord.nextStateDuration;
				float transitionNormalizedTime = previousTransitionRecord.normalizedTime -
												 elapsedTimeSinceLastRecord / previousTransitionRecord.transitionDuration;
				animator.CrossFadeInFixedTime(previousAnimationLayerRecord.shortNameHash, previousTransitionRecord.transitionDuration,
											  layer, nextStateFixedTime, transitionNormalizedTime);
				animator.Update(0.0f);
				Debug.Log("Case2 pTtime>=0 Current anim short name hash: " + previousAnimationLayerRecord.shortNameHash +
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
		} else if (!previousAnimationLayerRecord.isInTransition && nextAnimationLayerRecord.isInTransition &&
				   nextAnimationLayerRecord.IsInterruptingCurrentStateTransition) {

			/* Here we need to interpolate between the last frame of a transition (interrupting transition) 
			 * that interrupted another transition (interrupted transition), and the first frame after 
			 * the interrupting transition ends*/
			animator.speed = 0;
			float transitionNormalizedTime = nextAnimationLayerRecord.transitionRecord.normalizedTime +
											 (previousRecordDeltaTime - elapsedTimeSinceLastRecord) /
											 nextAnimationLayerRecord.transitionRecord.transitionDuration;
			//if (transitionNormalizedTime > 1) {
			float currentStateNormalizedTime = Mathf.Lerp(previousAnimationLayerRecord.normalizedTime,
														  nextAnimationLayerRecord.transitionRecord.nextStateNormalizedTime,
														  lerpAlpha);
			animator.Play(previousAnimationLayerRecord.shortNameHash, layer, currentStateNormalizedTime);
			animator.Update(0.0f);
			Debug.Log("Case3 nTtime>1 Current anim short name hash: " + previousAnimationLayerRecord.shortNameHash +
					  " current anim normalized time: " + currentStateNormalizedTime);
			/*} else {
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
						  " TransitionNormalizedTime: " + transitionNormalizedTime +
						  " Interrupted Next anim short name hash: " + nextAnimationRecord.interruptedTransition.nextStateNameHash +
						  " Interrupted Next anim normalized time: " + nextAnimationRecord.interruptedTransition.nextStateNormalizedTime +
						  " Interrupted TransitionNormalizedTime: " + nextAnimationRecord.interruptedTransition.normalizedTime);
			}*/
			animator.speed = 1;

		} else if (previousAnimationLayerRecord.isInTransition &&
				   nextAnimationLayerRecord.isInTransition &&
				   !previousAnimationLayerRecord.IsInterruptingCurrentStateTransition &&
				   !nextAnimationLayerRecord.IsInterruptingCurrentStateTransition &&
				   previousAnimationLayerRecord.shortNameHash != nextAnimationLayerRecord.shortNameHash) {


			/* Here we need to interpolate between frames belonging to different transitions, that is,
			interpolating between the first frame of a transition and the last frame of another transition.*/

			// I don't know if this scenario is possible.


		} else if (!previousAnimationLayerRecord.isInTransition && nextAnimationLayerRecord.isInTransition) {
			// Here we need to interpolate between the last frame of a transition and the first frame after the transition ends
			animator.speed = 1;
			TransitionRecord transitionRecord = nextAnimationLayerRecord.transitionRecord;
			float transitionNormalizedTime = transitionRecord.normalizedTime +
											 (previousRecordDeltaTime - elapsedTimeSinceLastRecord) /
											 transitionRecord.transitionDuration;
			if (transitionNormalizedTime < 1) {
				float nextStateNormalizedTime = Mathf.Lerp(previousAnimationLayerRecord.normalizedTime,
															  transitionRecord.nextStateNormalizedTime,
															  lerpAlpha);
				float nextStateFixedTime = nextStateNormalizedTime * transitionRecord.nextStateDuration;
				float currentNormalizedTime = nextAnimationLayerRecord.normalizedTime +
											  (previousRecordDeltaTime - elapsedTimeSinceLastRecord) / nextAnimationLayerRecord.duration;


				animator.Play(nextAnimationLayerRecord.shortNameHash, layer, currentNormalizedTime);
				animator.Update(0.0f);
				animator.CrossFadeInFixedTime(transitionRecord.nextStateNameHash, transitionRecord.transitionDuration,
											  layer, nextStateFixedTime, transitionNormalizedTime);
				animator.Update(0.0f);
				Debug.Log("Transition original normalized time = " + transitionRecord.normalizedTime +
						  " PreviousRecordDeltaTime = " + previousRecordDeltaTime +
						  " ElapsedTimeSinceLastRecord = " + elapsedTimeSinceLastRecord +
						  " Transition duration = " + transitionRecord.transitionDuration);
				Debug.Log("Case4 Ttime<1 Current anim normalized time: " + currentNormalizedTime +
						  " current anim duration: " + previousAnimationLayerRecord.duration +
						  " Next and previous record are same state: " + (previousAnimationLayerRecord.shortNameHash == nextAnimationLayerRecord.shortNameHash) +
						  " Next state normalized time: " + nextStateNormalizedTime +
						  " Transition normalizedTime: " + transitionNormalizedTime);
			} else {
				float normalizedTime = Mathf.Lerp(previousAnimationLayerRecord.normalizedTime,
												  nextAnimationLayerRecord.transitionRecord.nextStateNormalizedTime,
												  lerpAlpha);
				animator.Play(previousAnimationLayerRecord.shortNameHash, layer, normalizedTime);
				animator.Update(0.0f);
				Debug.Log("Case4 Ttime>=1 Current anim normalized time: " + normalizedTime +
						  " current anim duration: " + previousAnimationLayerRecord.duration +
						  " Next and previous record are same state: " +
						  (previousAnimationLayerRecord.shortNameHash == nextAnimationLayerRecord.shortNameHash));
			}


		} else if (!previousAnimationLayerRecord.isInTransition && !nextAnimationLayerRecord.isInTransition &&
					previousAnimationLayerRecord.shortNameHash == nextAnimationLayerRecord.shortNameHash) {
			// Here we need to interpolate between frames of the same animation that do not belong to a transition
			animator.speed = 1;
			float normalizedTime = Mathf.Lerp(previousAnimationLayerRecord.normalizedTime,
											  nextAnimationLayerRecord.normalizedTime,
											  lerpAlpha);
			animator.Play(previousAnimationLayerRecord.shortNameHash, layer, normalizedTime);
			animator.Update(0.0f);
			animator.speed = 0;
			Debug.Log("Case5 Current anim hash: " + previousAnimationLayerRecord.shortNameHash +
					  "Current anim normalized time: " + normalizedTime + " Next and previous record are same state: " +
					  (previousAnimationLayerRecord.shortNameHash == nextAnimationLayerRecord.shortNameHash));

		} else if (!previousAnimationLayerRecord.isInTransition && !nextAnimationLayerRecord.isInTransition &&
					previousAnimationLayerRecord.shortNameHash != nextAnimationLayerRecord.shortNameHash) {
			/* Here we need to interpolate between frames of the different animations that do not belong to a transition.
			 * This is a weird scenario that only occurs when a transition is so short (like 0 seconds) that it doesn't 
			 * get registered as such.*/

			animator.speed = 1;
			animator.Play(nextAnimationLayerRecord.shortNameHash, layer, nextAnimationLayerRecord.normalizedTime);
			animator.Update(0.0f);

			float transitionNormalizedTime = (previousRecordDeltaTime - elapsedTimeSinceLastRecord) / previousRecordDeltaTime;
			animator.CrossFadeInFixedTime(previousAnimationLayerRecord.shortNameHash, previousRecordDeltaTime,
										  layer, previousAnimationLayerRecord.normalizedTime, transitionNormalizedTime);
			animator.Update(0.0f);
			animator.speed = 0;
			Debug.Log("Case6 Current anim hash: " + previousAnimationLayerRecord.shortNameHash +
					  " Current anim normalized time: " + previousAnimationLayerRecord.normalizedTime +
					  " Next anim hash: " + nextAnimationLayerRecord.shortNameHash +
					  " Transiton normalizedTime: " + transitionNormalizedTime +
					  " Next anim normalized time: " + nextAnimationLayerRecord.normalizedTime +
					  " Next and previous record are same state: " + (previousAnimationLayerRecord.shortNameHash == nextAnimationLayerRecord.shortNameHash));

		} else if (previousAnimationLayerRecord.isInTransition && !nextAnimationLayerRecord.isInTransition) {
			/* Here we need to interpolate between a frame that does not belong to a transition and
			the first frame of a transition */
			animator.speed = 1;
			TransitionRecord transitionRecord = previousAnimationLayerRecord.transitionRecord;
			float transitionNormalizedTime = transitionRecord.normalizedTime -
											 elapsedTimeSinceLastRecord / transitionRecord.transitionDuration;

			if (transitionNormalizedTime > 0) {
				float nextStateNormalizedTime = transitionRecord.nextStateNormalizedTime -
												elapsedTimeSinceLastRecord / transitionRecord.nextStateDuration;
				float nextStateFixedTime = nextStateNormalizedTime * transitionRecord.nextStateDuration;
				float currentNormalizedTime = Mathf.Lerp(previousAnimationLayerRecord.normalizedTime,
														 nextAnimationLayerRecord.normalizedTime,
														 lerpAlpha);


				animator.Play(previousAnimationLayerRecord.shortNameHash, layer, currentNormalizedTime);
				animator.Update(0.0f);
				animator.CrossFadeInFixedTime(transitionRecord.nextStateNameHash, transitionRecord.transitionDuration,
											  layer, nextStateFixedTime, transitionNormalizedTime);
				animator.Update(0.0f);
				Debug.Log("Case7 Ttime>0 Current anim normalized time: " + currentNormalizedTime +
						  " current anim duration: " + previousAnimationLayerRecord.duration +
						  " Next and previous record are same state: " + (previousAnimationLayerRecord.shortNameHash == nextAnimationLayerRecord.shortNameHash) +
						  " Next state normalized time: " + nextStateNormalizedTime +
						  " Transition normalizedTime: " + transitionNormalizedTime);
			} else {
				float normalizedTime = Mathf.Lerp(previousAnimationLayerRecord.normalizedTime,
												  nextAnimationLayerRecord.normalizedTime,
												  lerpAlpha);
				animator.Play(previousAnimationLayerRecord.shortNameHash, layer, normalizedTime);
				animator.Update(0.0f);
				Debug.Log("Case7 Ttime<=0 Current anim normalized time: " + normalizedTime + " current anim duration: " +
							previousAnimationLayerRecord.duration + " Next and previous record are same state: " +
							(previousAnimationLayerRecord.shortNameHash == nextAnimationLayerRecord.shortNameHash));
			}
			animator.speed = 0;
		}
	}

	public void TrackInterruptedTransitions(ref AnimationRecord animationRecord, float recordDeltaTime) {
		for (int layer = 0; layer < animator.layerCount; layer++) {
			ref AnimationLayerRecord animationLayerRecord = ref animationRecord.animationLayerRecords[layer];
			ref AnimationLayerRecord lastAnimationLayerRecord = ref lastAnimationRecord.animationLayerRecords[layer];

			if (lastAnimationLayerRecord.isInTransition && animationLayerRecord.isInTransition &&
			   lastAnimationLayerRecord.shortNameHash == animationLayerRecord.shortNameHash &&
			   lastAnimationLayerRecord.transitionRecord.nextStateNameHash != animationLayerRecord.transitionRecord.nextStateNameHash) {

				lastInterruptedTransitionRecordInLayer[layer] = lastAnimationLayerRecord.transitionRecord;
				lastInterruptedTransitionRecordInLayer[layer].nextStateNormalizedTime += recordDeltaTime /
																						 lastInterruptedTransitionRecordInLayer[layer].nextStateDuration;
				lastInterruptedTransitionRecordInLayer[layer].normalizedTime += recordDeltaTime /
																				lastInterruptedTransitionRecordInLayer[layer].transitionDuration;
				
				ref AnimationLayerRecord[] animationLayerRecords = ref animationRecord.animationLayerRecords;
				animationLayerRecords[layer].interruptedTransition = lastInterruptedTransitionRecordInLayer[layer];
				animationLayerRecords[layer].IsInterruptingCurrentStateTransition = true;
			}

			if (lastAnimationLayerRecord.isInTransition && animationLayerRecord.isInTransition &&
			   lastAnimationLayerRecord.shortNameHash == animationLayerRecord.shortNameHash &&
			   lastAnimationLayerRecord.transitionRecord.nextStateNameHash == animationLayerRecord.transitionRecord.nextStateNameHash &&
			   lastAnimationLayerRecord.normalizedTime == animationLayerRecord.normalizedTime) {

	
				ref AnimationLayerRecord[] animationLayerRecords = ref animationRecord.animationLayerRecords;
				animationLayerRecords[layer].interruptedTransition = lastInterruptedTransitionRecordInLayer[layer];
				animationLayerRecords[layer].IsInterruptingCurrentStateTransition = true;

			}
		}
		lastAnimationRecord = animationRecord;
	}
}