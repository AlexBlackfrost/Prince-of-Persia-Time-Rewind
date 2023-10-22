using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagedState : State{
    [Serializable]
    public class DamagedSettings {
        public Animator Animator { get; set; }
        public CharacterMovement CharacterMovement { get; set; }
        public ParticleSystem Blood{ get; set; }
    }

    private DamagedSettings settings;
    public DamagedState(DamagedSettings settings) {
        this.settings = settings;

    }

    protected override void OnEnter() {
        settings.Animator.SetTrigger(AnimatorUtils.damagedHash);
        settings.Blood.Play();
    }

    protected override void OnExit() {
        //settings.Animator.SetBool(AnimatorUtils.damagedHash, false);
    }

    protected override void OnUpdate() {
        settings.CharacterMovement.Move(Vector3.zero);
    }
    public override object RecordFieldsAndProperties() {
        return null;
    }

    public override void RestoreFieldsAndProperties(object fieldsAndProperties) { }
}
