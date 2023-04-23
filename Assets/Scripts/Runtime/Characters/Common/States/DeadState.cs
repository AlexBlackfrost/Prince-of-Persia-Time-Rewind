using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : State{

    [Serializable]
    public class DeadSettings {
        public Animator Animator { get; set; }
    }

    private DeadSettings settings;
    public DeadState(DeadSettings settings) {
        this.settings = settings;
        AnimatorUtils.AnimationEnded += OnDieAnimationEnded;
    }

    protected override void OnEnter() {
        settings.Animator.SetTrigger(AnimatorUtils.dieHash);
    }

    protected override void OnExit() {
        settings.Animator.speed = 1;    
    }

    private void OnDieAnimationEnded(int shortNameHash) {
        if(shortNameHash == AnimatorUtils.dieHash) {
            settings.Animator.speed = 0;
        }
    }

}