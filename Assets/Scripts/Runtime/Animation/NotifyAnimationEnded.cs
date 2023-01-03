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
        if (stateInfo.shortNameHash == Animator.StringToHash("Land")) {
            Debug.Log("Land enter");
        }else if(stateInfo.shortNameHash == Animator.StringToHash("Fall")){
            Debug.Log("Fall enter");
        } else if (stateInfo.shortNameHash == Animator.StringToHash("WallRunRight")) {
            Debug.Log("WallRunRight enter");
        } else if (stateInfo.shortNameHash == Animator.StringToHash("WallRunLeft")) {
            Debug.Log("WallRunLeft enter");
        } else {
            Debug.Log("other enter");
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (normalizedTimeFireEvent >= 0 && stateInfo.normalizedTime > normalizedTimeFireEvent && !eventFired ) {
            animator.NotifyAnimationEnded(stateInfo.shortNameHash);
            eventFired = true;
            if (stateInfo.shortNameHash == Animator.StringToHash("Land")) {
                Debug.Log("Land fired event");
            } else if (stateInfo.shortNameHash == Animator.StringToHash("Fall")) {
                Debug.Log("Fall fired event");
            } else if (stateInfo.shortNameHash == Animator.StringToHash("WallRunRight")) {
                Debug.Log("WallRunRight fired event");
            } else if (stateInfo.shortNameHash == Animator.StringToHash("WallRunLeft")) {
                Debug.Log("WallRunLeft fired event");
            } else {
                Debug.Log("other fired event");
            }
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (stateInfo.shortNameHash == Animator.StringToHash("Land")) {
            Debug.Log("Land exit");
        } else if (stateInfo.shortNameHash == Animator.StringToHash("Fall")) {
            Debug.Log("Fall exit");
        } else if (stateInfo.shortNameHash == Animator.StringToHash("WallRunRight")) {
            Debug.Log("WallRunRight exit");
        } else if (stateInfo.shortNameHash == Animator.StringToHash("WallRunLeft")) {
            Debug.Log("WallRunLeft exit");
        } else {
            Debug.Log("other exit");
        }
        if (!eventFired) {
            if (stateInfo.shortNameHash == Animator.StringToHash("Land")) {
                Debug.Log("Land fired event");
            } else if (stateInfo.shortNameHash == Animator.StringToHash("Fall")) {
                Debug.Log("Fall fired event");
            } else if (stateInfo.shortNameHash == Animator.StringToHash("WallRunRight")) {
                Debug.Log("WallRunRight fired event");
            } else if (stateInfo.shortNameHash == Animator.StringToHash("WallRunLeft")) {
                Debug.Log("WallRunLeft fired event");
            } else {
                Debug.Log("other fired event");
            }
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

