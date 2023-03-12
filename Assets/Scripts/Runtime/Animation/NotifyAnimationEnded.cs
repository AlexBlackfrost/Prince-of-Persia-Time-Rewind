using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class NotifyAnimationEnded : StateMachineBehaviour {
    [Tooltip("If value is <0, the event is fired after transitioning to a new state. "+
             "If it is greater than 0 the event is fired at the specified normalized time " +  
             "(0 is the beggining of the animation and 1 is the end of the animation)")]
    [SerializeField] private float normalizedTimeFireEvent = -1;

    private bool eventFired;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        eventFired = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (normalizedTimeFireEvent >= 0 && stateInfo.normalizedTime >= normalizedTimeFireEvent && !eventFired ) {
            animator.NotifyAnimationEnded(stateInfo.shortNameHash);
            eventFired = true;
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (!eventFired) {
            animator.NotifyAnimationEnded(stateInfo.shortNameHash);
            eventFired = true;
        }
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
        // this only works if transition occurs from a state machine to another state machine.
        if (normalizedTimeFireEvent >= 0 && !eventFired) {
            animator.NotifyAnimationEnded(stateMachinePathHash);
        }
    }
}

