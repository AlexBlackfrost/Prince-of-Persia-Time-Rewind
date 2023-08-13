using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class NotifyAnimationStarted : StateMachineBehaviour {
    [Tooltip("If value is <0, the event is fired as soon as the state starts playing. " +
             "If it is greater than 0 the event is fired at the specified normalized time " +
             "(0 is the beggining of the animation and 1 is the end of the animation)")]
    [SerializeField] private float normalizedTimeFireEvent = -1;

    private bool eventFired;
     
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (normalizedTimeFireEvent <= 0) {
            animator.NotifyAnimationStarted(stateInfo.shortNameHash);
            eventFired = true;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (normalizedTimeFireEvent > 0 && stateInfo.normalizedTime >= normalizedTimeFireEvent && !eventFired && !TimeRewindManager.Instance.IsRewinding) {
            animator.NotifyAnimationStarted(stateInfo.shortNameHash);
            eventFired = true;
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (!eventFired && !TimeRewindManager.Instance.IsRewinding) {
            animator.NotifyAnimationStarted(stateInfo.shortNameHash);
            eventFired = true;
        }
    }

}

