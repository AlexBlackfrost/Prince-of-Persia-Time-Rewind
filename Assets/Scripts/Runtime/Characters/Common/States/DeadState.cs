using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : State{

    [Serializable]
    public class DeadSettings {
        public Animator Animator { get; set; }
        public Hurtbox Hurtbox { get; set; }
    }

    private DeadSettings settings;
    public DeadState(DeadSettings settings) {
        this.settings = settings;
        AnimatorUtils.AnimationEnded += OnDieAnimationEnded;
    }

    protected override void OnEnter() {
        settings.Animator.SetTrigger(AnimatorUtils.dieHash);
        settings.Hurtbox.enabled = false;
    }

    protected override void OnExit() {
        settings.Animator.speed = 1;    
        settings.Hurtbox.enabled = true;
    }

    private void OnDieAnimationEnded(int shortNameHash) {
        if(shortNameHash == AnimatorUtils.dieHash) {
            settings.Animator.speed = 0;
        }
    }

    public override object RecordFieldsAndProperties() {
        return null;
    }

    public override void RestoreFieldsAndProperties(object fieldsAndProperties) { }
}