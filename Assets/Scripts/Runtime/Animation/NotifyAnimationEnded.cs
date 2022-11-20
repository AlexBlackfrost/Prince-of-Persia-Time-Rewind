using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotifyAnimationEnded : StateMachineBehaviour {
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.NotifyAnimationEnded(stateInfo.shortNameHash);
    }
}

