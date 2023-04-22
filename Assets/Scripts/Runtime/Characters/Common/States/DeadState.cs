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
    private int dieHash;
    public DeadState(DeadSettings settings) {
        this.settings = settings;
        dieHash = Animator.StringToHash("Die");
        AnimatorUtils.AnimationEnded += OnDieAnimationEnded;
    }

    protected override void OnEnter() {
        settings.Animator.SetTrigger(dieHash);
    }

    protected override void OnExit() {
        settings.Animator.speed = 1;    
    }

    private void OnDieAnimationEnded(int shortNameHash) {
        if(shortNameHash == dieHash) {
            settings.Animator.speed = 0;
        }
    }

}