using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParriedState : State{
    [Serializable]
    public class ParriedSettings {
        public Animator Animator { get; set; }
        public Sword Sword {get;set; }
    }

    private ParriedSettings settings;
    public ParriedState(ParriedSettings settings) {
        this.settings = settings;
    }

    protected override void OnEnter() {
        settings.Animator.SetBool(AnimatorUtils.parriedHash, true);
        settings.Sword.PlaySwordClashVFX();
    }

    protected override void OnExit() {
        settings.Animator.SetBool(AnimatorUtils.parriedHash, false);
    }

    public override object RecordFieldsAndProperties() {
        return null;
    }

    public override void RestoreFieldsAndProperties(object fieldsAndProperties) { }
}